using vsbd_core;

public class NodeFactory
{
    private const string AssemblyName = "vsbd-nodes";

    private readonly PathService _path;

    private bool IsInitialized = false;
    private string ProjectId;
    private readonly ILogger<NodeFactory> _logger;

    public NodeFactory(PathService path, ILogger<NodeFactory> logger)
    {
        _logger = logger;
        _path = path;
    }

    public async Task<NodeBase?> GetCompiledNode(string name, string id)
    {
        if (!IsInitialized)
        {
            throw new Exception("Factory is not initialized");
        }
        
        var alc = new System.Runtime.Loader.AssemblyLoadContext(AssemblyName, isCollectible: true);
        var asm = alc.LoadFromAssemblyPath(_path.GetProjectLibrary(ProjectId));

        var exportedTypes = asm.GetExportedTypes().Where(t => t.IsClass && !t.IsAbstract);
        var type = exportedTypes.Where(x => x.FullName == name).FirstOrDefault();

        if (type == null)
        {
            return null;
        }

        var instance = Activator.CreateInstance(type) as NodeBase;

        if (instance is null)
        {
            throw new Exception($"Could not create instance of {name}");
        }

        INodeLogger nodeLogger = new NodeLogger(_logger);

        instance.Context = new NodeContext()
        {
            Logger = nodeLogger,
            NodeId = id,
        };

        instance!.OnNodeCreate();

        return instance;
    }

    public void InitializeFactory(string projectId)
    {
        IsInitialized = true;
        ProjectId = projectId;
    }
}