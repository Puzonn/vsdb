using System.Collections.Concurrent;
using System.Threading.Channels;

public sealed class FlowJobManager : IFlowJobManager
{
    private readonly IServiceScopeFactory _scopes;
    private readonly ConcurrentDictionary<string, FlowJobHandle> _jobs = new();
    private readonly ILogger<FlowJobManager> _logger;

    public FlowJobManager(IServiceScopeFactory scopes, ILogger<FlowJobManager> logger)
    {
        _scopes = scopes;
        _logger = logger;
    }

    public bool StartClient(string clientId, Flow flow, out string error)
    {
        error = default!;
        if (_jobs.ContainsKey(clientId))
        {
            error = "Already started.";
            return false;
        }

        var cts = new CancellationTokenSource();
        var queue = Channel.CreateUnbounded<Seed>(new UnboundedChannelOptions
        {
            SingleReader = true,
            SingleWriter = false
        });

        var dispatcher = new ChannelEventDispatcher(queue);
        using var scope = _scopes.CreateScope();
        var nodeFactory = scope.ServiceProvider.GetRequiredService<BuildService>();
        var logger = scope.ServiceProvider.GetRequiredService<ILogger<FlowRunnerService>>();

        foreach (var n in flow.Nodes)
        {
            var compiled = nodeFactory.GetCompiledNode(n.Name, n.Id, logger);
            if (compiled is IEventSourceNode src)
            {
                _logger.LogInformation($"Created entry point {compiled}");
                _ = src.StartAsync(dispatcher, cts.Token);
            }
        }

        var worker = Task.Run(async () =>
        {
            using var scope = _scopes.CreateScope();
            var runner = scope.ServiceProvider.GetRequiredService<FlowRunnerService>();
            var ct = cts.Token;

            try
            {
                while (await queue.Reader.WaitToReadAsync(ct).ConfigureAwait(false))
                {
                    while (queue.Reader.TryRead(out var seed))
                        await runner.RunFlow(flow, seed, ct).ConfigureAwait(false);
                }
            }
            catch (OperationCanceledException) { }
        });

        _jobs[clientId] = new FlowJobHandle
        {
            Flow = flow,
            Queue = queue,
            TokenSource = cts,
            Worker = worker
        };
        return true;
    }

    public bool Enqueue(string clientId, Seed seed, out string error)
    {
        error = default!;
        if (!_jobs.TryGetValue(clientId, out var h))
        {
            error = "Client not started.";
            return false;
        }
        return h.Queue.Writer.TryWrite(seed);
    }

    public async Task<bool> StopClient(string clientId)
    {
        if (!_jobs.TryRemove(clientId, out var h)) return false;
        h.Queue.Writer.TryComplete();
        h.TokenSource.Cancel();
        try { await h.Worker.ConfigureAwait(false); } catch { }
        h.TokenSource.Dispose();
        return true;
    }
}