using System.Reflection;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using vsbd_core;

public class BuildService
{
    const string AssemblyName = "vsbd-nodes";

    public async Task<BuildResult> Compile()
    {
        var baseDir = AppContext.BaseDirectory;
        var outDir = Path.Combine(baseDir, "Libraries");
        Directory.CreateDirectory(outDir);

        var dllPath = Path.Combine(outDir, $"{AssemblyName}.dll");
        var pdbPath = Path.Combine(outDir, $"{AssemblyName}.pdb");
        var projectDir = @"/home/puzonne/vsbd/vsbd-nodes/";

        if (!Directory.Exists(projectDir))
            return new BuildResult(false, $"ProjectDir not found: {projectDir}");

        var csFiles = Directory.EnumerateFiles(projectDir, "*.cs", SearchOption.AllDirectories)
            .Where(p => !p.Contains($"{Path.DirectorySeparatorChar}bin{Path.DirectorySeparatorChar}")
                     && !p.Contains($"{Path.DirectorySeparatorChar}obj{Path.DirectorySeparatorChar}"))
            .ToArray();

        if (csFiles.Length == 0)
            return new BuildResult(false, $"No .cs files found.");

        var parseOptions = new CSharpParseOptions(LanguageVersion.CSharp9)
            .WithPreprocessorSymbols("DEBUG", "TRACE");

        var trees = csFiles.Select(f => CSharpSyntaxTree.ParseText(System.IO.File.ReadAllText(f), parseOptions, f)).ToList();

        var tpa = (string?)AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES") ?? "";
        var refs = tpa.Split(Path.PathSeparator, StringSplitOptions.RemoveEmptyEntries)
                      .Where(p => p.EndsWith(".dll", StringComparison.OrdinalIgnoreCase))
                      .Select(p => (MetadataReference)MetadataReference.CreateFromFile(p))
                      .ToList();

        var coreDll = Path.Combine(baseDir, "Libraries", "vsbd-core.dll");
        if (!File.Exists(coreDll))
            return new BuildResult(false, $"Core not found: {coreDll}");

        refs.Add(MetadataReference.CreateFromFile(coreDll));

        var compOptions = new CSharpCompilationOptions(
            OutputKind.DynamicallyLinkedLibrary,
            optimizationLevel: OptimizationLevel.Debug,
            allowUnsafe: true,
            deterministic: true);

        var compilation = CSharpCompilation.Create(
            assemblyName: AssemblyName,
            syntaxTrees: trees,
            references: refs,
            options: compOptions);

        using var dll = System.IO.File.Create(dllPath);
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
            return new BuildResult(false, string.Join("\n", diags));

        return new BuildResult(true, string.Join("\n", diags));
    }

    public async Task<NodeResult> GetNodes()
    {
        var full = Path.Combine(AppContext.BaseDirectory, "Libraries", $"{AssemblyName}.dll");
        if (!File.Exists(full))
            return new NodeResult(false, "dll not found");

        var pluginDir = Path.GetDirectoryName(full)!;
        var coreAsm = typeof(vsbd_core.NodeBase).Assembly;
        var coreName = coreAsm.GetName().Name;

        var alc = new System.Runtime.Loader.AssemblyLoadContext(AssemblyName, isCollectible: true);
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
            var nodes = new List<Node>();

            var inputAttrType = typeof(NodeInputAttribute);
            var outputAttrType = typeof(NodeOutputAttribute);

            foreach (var type in asm.GetExportedTypes().Where(t => t.IsClass && !t.IsAbstract))
            {
                var inputAttrs = type.GetCustomAttributes(inputAttrType, inherit: false)
                                     .Cast<NodeInputAttribute>()
                                     .ToArray();

                var inputs = inputAttrs
                    .SelectMany(a => a.Inputs)
                    .Select(x => x.FullName)
                    .ToArray();

                var outputAttrs = type.GetCustomAttributes(outputAttrType, inherit: false)
                                      .Cast<NodeOutputAttribute>()
                                      .ToArray();

                var outputs = outputAttrs
                    .SelectMany(a => a.Outputs)
                    .Select(x => x.FullName)
                    .ToArray();

                nodes.Add(new Node(type.FullName!)
                {
                    Inputs = inputs,
                    Outputs = outputs
                });
            }


            return new NodeResult(true, null, nodes.ToArray());
        }
        catch (ReflectionTypeLoadException ex)
        {
            var errors = ex.LoaderExceptions.Select(e => e.Message).ToArray();
            return new NodeResult(false, string.Join("\n", errors));
        }
    }
}