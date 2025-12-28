using System.Reflection;
using System.Runtime.Loader;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using vsbd_core;

public class BuildService
{
    private readonly ILogger<BuildService> _logger;
    private readonly IWebHostEnvironment _env;

    public BuildService(ILogger<BuildService> logger, IWebHostEnvironment env)
    {
        _env = env;
        _logger = logger;
    }

    static bool IsSigned(Assembly a)
        => a.GetName().GetPublicKeyToken()?.Length > 0;

    public async Task<BuildResult> Compile(string projectDir, string projectId)
    {
        var assemblyName = $"vsbd-nodes-{projectId}";

        var baseDir = AppContext.BaseDirectory;
        var outDir = Path.Combine(baseDir, "Libraries", projectId);
        Directory.CreateDirectory(outDir);

        var dllPath = Path.Combine(outDir, $"{assemblyName}.dll");

        var csFiles = Directory.EnumerateFiles(projectDir, "*.cs", SearchOption.AllDirectories)
            .Where(p => !p.Contains($"{Path.DirectorySeparatorChar}bin{Path.DirectorySeparatorChar}")
                     && !p.Contains($"{Path.DirectorySeparatorChar}obj{Path.DirectorySeparatorChar}"))
            .ToArray();

        if (csFiles.Length == 0)
            return new BuildResult(false, "No .cs files found.");

        var parseOptions = new CSharpParseOptions(LanguageVersion.CSharp12);
        var trees = csFiles.Select(f =>
            CSharpSyntaxTree.ParseText(File.ReadAllText(f), parseOptions, f)
        );

        var tpa = (string?)AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES") ?? "";
        var refs = tpa.Split(Path.PathSeparator)
            .Select(p => MetadataReference.CreateFromFile(p))
            .ToList();

        var coreAsm = typeof(NodeBase).Assembly;
        refs.Add(MetadataReference.CreateFromFile(coreAsm.Location));

        var compilation = CSharpCompilation.Create(
            assemblyName,
            trees,
            refs,
            new CSharpCompilationOptions(
                OutputKind.DynamicallyLinkedLibrary,
                strongNameProvider: null
            )
        );

        using var fs = File.Create(dllPath);
        var result = compilation.Emit(fs);

        if (!result.Success)
        {
            var errors = result.Diagnostics
                .Where(d => d.Severity == DiagnosticSeverity.Error)
                .Select(d => d.ToString());

            return new BuildResult(false, string.Join("\n", errors));
        }

        return new BuildResult(true, null);
    }

    public BuildResult CompileInMemory(IReadOnlyList<ScriptSource> scripts, string projectId, out byte[] assemblyBytes)
    {
        var assemblyName = $"vsbd-nodes-{projectId}";
        assemblyBytes = Array.Empty<byte>();

        var parseOptions = new CSharpParseOptions(LanguageVersion.CSharp12);

        var trees = scripts.Select(s =>
            CSharpSyntaxTree.ParseText(
                s.Source,
                parseOptions,
                path: s.FileName
            )
        );

        var refs = ((string?)AppContext.GetData("TRUSTED_PLATFORM_ASSEMBLIES") ?? "")
            .Split(Path.PathSeparator)
            .Select(p => MetadataReference.CreateFromFile(p))
            .ToList();

        refs.Add(MetadataReference.CreateFromFile(typeof(NodeBase).Assembly.Location));

        var compilation = CSharpCompilation.Create(
            assemblyName,
            trees,
            refs,
            new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary)
        );

        using var ms = new MemoryStream();
        var result = compilation.Emit(ms);

        if (!result.Success)
        {
            var errors = result.Diagnostics
                .Where(d => d.Severity == DiagnosticSeverity.Error)
                .Select(d => d.ToString());

            return new BuildResult(false, string.Join('\n', errors));
        }

        assemblyBytes = ms.ToArray();
        return new BuildResult(true, null);
    }

    public async Task<NodeResult> GetNodes(
        byte[] assemblyBytes,
        IReadOnlyList<ScriptSource> scripts,
        bool attachSourceCode
    )
    {
        var coreAsm = typeof(NodeBase).Assembly;
        var alc = new AssemblyLoadContext("vsbd-nodes", isCollectible: true);

        alc.Resolving += (_, name) =>
            name.Name == coreAsm.GetName().Name ? coreAsm : null;

        using var ms = new MemoryStream(assemblyBytes);
        var asm = alc.LoadFromStream(ms);

        try
        {
            var nodes = new List<Node>();

            foreach (var type in asm.GetExportedTypes().Where(t => t.IsClass && !t.IsAbstract))
            {
                var inputs = type
                    .GetCustomAttributes<NodeInputAttribute>(false)
                    .Select(a => new NodeInput(a.Type.FullName!, a.Name))
                    .ToArray();

                var outputs = type
                    .GetCustomAttributes<NodeOutputAttribute>(false)
                    .Select(a => new NodeOutput(a.Type.FullName!, a.Name))
                    .ToArray();

                var properties = type
                    .GetProperties(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                    .Select(p => (Prop: p, Attr: p.GetCustomAttribute<NodePropertyAttribute>()))
                    .Where(x => x.Attr != null)
                    .Select(x => new NodeProperty(
                        x.Prop.PropertyType.FullName ?? "?",
                        x.Prop.Name,
                        x.Attr!.DefaultValue?.ToString()
                    ))
                    .ToArray();

                string? sourceCode = null;

                if (attachSourceCode)
                {
                    sourceCode = scripts
                        .FirstOrDefault(s => Path.GetFileNameWithoutExtension(s.FileName) == type.Name)
                        ?.Source;
                }

                nodes.Add(new Node
                {
                    Name = type.FullName!,
                    SourceCode = sourceCode,
                    Inputs = inputs,
                    Outputs = outputs,
                    Properties = properties
                });
            }

            return new NodeResult(true, null, nodes.ToArray());
        }
        catch (ReflectionTypeLoadException ex)
        {
            return new NodeResult(
                false,
                string.Join('\n', ex.LoaderExceptions.Select(e => e.Message))
            );
        }
    }
}
