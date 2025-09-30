using System.Reflection;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using vsbd_core;
using vsbd.Attributes;
using vsbd.Models;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

app.MapPost("/start" , (RoslynCompileRequest req) =>
{
    
}

app.MapPost("/compile" ,(RoslynCompileRequest req) =>
{
    var projectDir = Path.GetFullPath(req.ProjectDir ?? ".");
    var outDir = Path.GetFullPath(req.OutDir ?? Path.Combine(AppContext.BaseDirectory, "plugins"));
    Directory.CreateDirectory(outDir);

    var asmName = string.IsNullOrWhiteSpace(req.AssemblyName) ? "vsbd-nodes" : req.AssemblyName!;
    var dllPath = Path.Combine(outDir, $"{asmName}.dll");
    var pdbPath = Path.Combine(outDir, $"{asmName}.pdb");

    if (!Directory.Exists(projectDir))
        return Results.BadRequest(new RoslynCompileResponse(false, null, null, new[] { $"ProjectDir not found: {projectDir}" }));

    var csFiles = Directory.EnumerateFiles(projectDir, "*.cs", SearchOption.AllDirectories)
        .Where(p => !p.Contains($"{Path.DirectorySeparatorChar}bin{Path.DirectorySeparatorChar}")
                 && !p.Contains($"{Path.DirectorySeparatorChar}obj{Path.DirectorySeparatorChar}"))
        .ToArray();

    if (csFiles.Length == 0)
        return Results.BadRequest(new RoslynCompileResponse(false, null, null, new[] { "No .cs files found." }));

    var parseOptions = new CSharpParseOptions(LanguageVersion.CSharp9)
        .WithPreprocessorSymbols(req.DefineConstants ?? Array.Empty<string>());

    var trees = csFiles.Select(f => CSharpSyntaxTree.ParseText(File.ReadAllText(f), parseOptions, f)).ToList();

    var tpa = (string?)AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES") ?? "";
    var refs = tpa.Split(Path.PathSeparator, StringSplitOptions.RemoveEmptyEntries)
                  .Where(p => p.EndsWith(".dll", StringComparison.OrdinalIgnoreCase))
                  .Select(p => (MetadataReference)MetadataReference.CreateFromFile(p))
                  .ToList();

    if (req.AdditionalRefs is { Length: > 0 })
    {
        foreach (var r in req.AdditionalRefs)
        {
            var full = Path.GetFullPath(Path.Combine(projectDir, r));
            if (File.Exists(full)) refs.Add(MetadataReference.CreateFromFile(full));
            else if (File.Exists(r)) refs.Add(MetadataReference.CreateFromFile(Path.GetFullPath(r)));
            else return TypedResults.BadRequest(new RoslynCompileResponse(false, null, null, new[] { $"Missing reference: {r}" }));
        }
    }

    refs.Add(MetadataReference.CreateFromFile(@"C:\\Users\\Przemek.P\\RiderProjects\\vsbd\\vsbd-core\\bin\\Debug\\net9.0\\vsbd-core.dll"));
        
    var compOptions = new CSharpCompilationOptions(
        OutputKind.DynamicallyLinkedLibrary,
        optimizationLevel: req.Optimize ? OptimizationLevel.Release : OptimizationLevel.Debug,
        allowUnsafe: req.AllowUnsafe,
        deterministic: true);

    var compilation = CSharpCompilation.Create(
        assemblyName: asmName,
        syntaxTrees: trees,
        references: refs,
        options: compOptions);

    using var dll = File.Create(dllPath);
    var emitResult = compilation.Emit(peStream: dll);

    var diags = emitResult.Diagnostics
        .Where(d => d.Severity is DiagnosticSeverity.Error or DiagnosticSeverity.Warning)
        .Select(d =>
        {
            var span = d.Location.GetLineSpan();
            var file = span.Path ?? "";
            var line = span.StartLinePosition.Line + 1;
            var col = span.StartLinePosition.Character + 1;
            return $"{d.Severity} {d.Id}: {d.GetMessage()} ({file}:{line},{col})";
        })
        .ToArray();

    if (!emitResult.Success)
        return Results.BadRequest(new RoslynCompileResponse(false, null, null, diags));

    return Results.Ok(new RoslynCompileResponse(true, dllPath, pdbPath, diags));
});

app.MapGet("/types", (string dllPath) =>
{
    var full = Path.GetFullPath(dllPath);
    if (!File.Exists(full))
        return Results.BadRequest(new { error = "dll not found", path = full });

    var pluginDir = Path.GetDirectoryName(full)!;

    var coreAsm = typeof(vsbd_core.NodeBase).Assembly;
    var coreName = coreAsm.GetName().Name;

    var alc = new System.Runtime.Loader.AssemblyLoadContext("vsbd-nodes", isCollectible: true);
    
    alc.Resolving += (ctx, name) =>
    {
        if (string.Equals(name.Name, coreName, StringComparison.OrdinalIgnoreCase))
            return coreAsm;

        var candidate = Path.Combine(pluginDir, $"{name.Name}.dll");
        return File.Exists(candidate) ? ctx.LoadFromAssemblyPath(candidate) : null;
    };

    var asm = alc.LoadFromAssemblyPath(full);

    try
    {
        List<NodeDto> nodes = new List<NodeDto>();
        
        var types = asm.GetExportedTypes()
            .Where(t => t.IsClass && !t.IsAbstract)
            .OrderBy(n => n)
            .ToArray();

        foreach (var type in types)
        {
            var node = new NodeDto();
            node.Name = type.FullName ?? "No Name";
            
            var outputs = type.GetCustomAttribute(typeof(NodeOutput));
            
            if (outputs != null)
            {
                var t = outputs.GetType();
                var prop = t.GetProperty("Outputs");
                var val = prop.GetValue(outputs) as Type[];

                node.Outputs = val.Select(x => x.FullName).ToArray();
            }
            
            var inputs = type.GetCustomAttribute(typeof(NodeInput));
            
            if (inputs != null)
            {
                var t = inputs.GetType();
                var prop = t.GetProperty("Inputs");
                var val = prop.GetValue(inputs) as Type[];

                node.Inputs = val.Select(x => x.FullName).ToArray();
            }
            
            nodes.Add(node);
        }

        return TypedResults.Ok(nodes);
    }
    catch (ReflectionTypeLoadException ex)
    {
        var errors = ex.LoaderExceptions.Select(e => e.Message).ToArray();
        return Results.BadRequest(new { error = "type load failed", loaderErrors = errors });
    }
});


app.Run();

record RoslynCompileRequest(
    string? ProjectDir,
    string? AssemblyName,
    string? OutDir,
    bool Optimize = true,
    bool AllowUnsafe = false,
    string[]? AdditionalRefs = null,
    string[]? DefineConstants = null);

record RoslynCompileResponse(bool Success, string? DllPath, string? PdbPath, string[] Diagnostics);
