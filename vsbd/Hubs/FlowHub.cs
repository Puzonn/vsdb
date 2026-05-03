using Microsoft.AspNetCore.SignalR;

public class FlowHub : Hub
{
    private readonly IFlowJobManager _jobManager;
    private readonly SharedLibraryService _library;

    public FlowHub(IFlowJobManager jobManager, SharedLibraryService library)
    {
        _library = library;
        _jobManager = jobManager;
    }

    public async Task<string> StartFlow(string projectId)
    {
        CancellationToken e = CancellationToken.None;
        var build = await _library.BuildSharedLibrary(e, projectId);
        var ok = _jobManager.StartClient(Context.ConnectionId, projectId, out _);
        
        return build;
    }

    public Task<bool> StopFlow() => _jobManager.StopClient(Context.ConnectionId);
}