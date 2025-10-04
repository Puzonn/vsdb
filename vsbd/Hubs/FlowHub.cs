using Microsoft.AspNetCore.SignalR;

public class FlowHub : Hub
{
    private readonly IFlowJobManager _jobManager;

    public FlowHub(IFlowJobManager jobManager)
    {
        _jobManager = jobManager;
    }

    public Task<bool> StartFlow(Flow flow)
    {
        var ok = _jobManager.StartClient(Context.ConnectionId, flow, out _);
        return Task.FromResult(ok);
    }

    public Task<bool> StopFlow()
       => _jobManager.StopClient(Context.ConnectionId);
}