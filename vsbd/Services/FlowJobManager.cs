using System.Collections.Concurrent;
using System.Threading.Channels;

public sealed class FlowJobManager : IFlowJobManager
{
    private readonly IServiceScopeFactory _scopes;
    private readonly ConcurrentDictionary<string, FlowJobHandle> _jobs = new();
    private readonly ILogger<FlowJobManager> _logger;
    private readonly NodeFactory _nodeFactory;
    private readonly ProjectRepository _projectRepository;

    public FlowJobManager(
        IServiceScopeFactory scopes,
        ILogger<FlowJobManager> logger,
        NodeFactory nodeFactory, ProjectRepository projectRepository)
    {
        _projectRepository = projectRepository;
        _scopes = scopes;
        _logger = logger;
        _nodeFactory = nodeFactory;
    }

    public bool StartClient(string clientId, string projectId, out string error)
    {
        error = default!;

        if (_jobs.ContainsKey(clientId))
        {
            error = "Already started.";
            return false;
        }

        using var scope = _scopes.CreateScope();

        var flow = _projectRepository.BuildFlowAsync(projectId)
            .GetAwaiter()
            .GetResult();

        if (flow is null)
        {
            error = "Project not found.";
            return false;
        }

        _nodeFactory.InitializeFactory(projectId);

        var cts = new CancellationTokenSource();

        var queue = Channel.CreateUnbounded<Seed>(new UnboundedChannelOptions
        {
            SingleReader = true,
            SingleWriter = false,
        });

        var dispatcher = new ChannelEventDispatcher(queue);

        foreach (var node in flow.Nodes)
        {
            var compiled = _nodeFactory.GetCompiledNode(node.Name, node.Id);

            if (compiled is IEventSourceNode eventSource)
            {
                _logger.LogInformation(
                    "Created entry point {NodeName} {NodeId}",
                    node.Name,
                    node.Id
                );

                _ = eventSource.StartAsync(dispatcher, cts.Token);
            }
        }

        var worker = Task.Run(async () =>
        {
            using var workerScope = _scopes.CreateScope();

            var runner = workerScope.ServiceProvider
                .GetRequiredService<FlowRunnerService>();

            var ct = cts.Token;

            try
            {
                while (await queue.Reader.WaitToReadAsync(ct).ConfigureAwait(false))
                {
                    while (queue.Reader.TryRead(out var seed))
                    {
                        await runner.RunFlow(flow, seed, ct)
                            .ConfigureAwait(false);
                    }
                }
            }
            catch (OperationCanceledException)
            {
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Flow worker crashed for client {ClientId}", clientId);
            }
        });

        _jobs[clientId] = new FlowJobHandle
        {
            Flow = flow,
            Queue = queue,
            TokenSource = cts,
            Worker = worker,
        };

        return true;
    }

    public bool Enqueue(string clientId, Seed seed, out string error)
    {
        error = default!;

        if (!_jobs.TryGetValue(clientId, out var handle))
        {
            error = "Client not started.";
            return false;
        }

        return handle.Queue.Writer.TryWrite(seed);
    }

    public async Task<bool> StopClient(string clientId)
    {
        if (!_jobs.TryRemove(clientId, out var handle))
        {
            return false;
        }

        handle.Queue.Writer.TryComplete();
        handle.TokenSource.Cancel();

        try
        {
            await handle.Worker.ConfigureAwait(false);
        }
        catch
        {
        }

        handle.TokenSource.Dispose();

        return true;
    }
}