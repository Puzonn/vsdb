using System.Security.Cryptography.X509Certificates;

public class FlowRunnerService
{
    private readonly BuildService _buildService;
    private readonly ILogger<FlowRunnerService> _logger;

    private Dictionary<string, object> _lastOutputs = new();

    public FlowRunnerService(BuildService buildService, ILogger<FlowRunnerService> logger)
    {
        _buildService = buildService;
        _logger = logger;
    }


    public Task RunFlow(Flow flow, Seed seed, CancellationToken ct = default)
        => RunFlow(flow, new[] { seed }, ct);

    public async Task RunFlow(Flow flow, IEnumerable<Seed> seeds, CancellationToken ct = default)
    {
        var nodesById = flow.Nodes.ToDictionary(n => n.Id);
        var outgoing = flow.Connections.GroupBy(c => c.From).ToDictionary(g => g.Key, g => g.ToList());
        var incoming = flow.Connections.GroupBy(c => c.To).ToDictionary(g => g.Key, g => g.ToList());

        var start = seeds.Select(s => s.NodeId).Distinct().ToList();
        if (start.Count == 0)
        {
            start = flow.Nodes
                .Where(n => !incoming.ContainsKey(n.Id))
                .Select(n => n.Id)
                .ToList();
        }

        var reachable = new HashSet<int>(start);
        var q = new Queue<int>(start);

        while (q.Count > 0)
        {
            var u = q.Dequeue();
            if (outgoing.TryGetValue(u, out var outs))
                foreach (var e in outs)
                    if (reachable.Add(e.To)) q.Enqueue(e.To);
        }

        var remainingParents = reachable.ToDictionary(
            id => id,
            id => incoming.TryGetValue(id, out var inc) ? inc.Count(c => reachable.Contains(c.From)) : 0);

        var inputsForNode = new Dictionary<int, Dictionary<string, object>>();
        Dictionary<string, object> NewBag() => new(StringComparer.OrdinalIgnoreCase);
        foreach (var s in seeds)
            inputsForNode[s.NodeId] = new Dictionary<string, object>(s.Inputs, StringComparer.OrdinalIgnoreCase);

        var ready = new Queue<int>(start.Where(id => remainingParents[id] == 0));

        int executedCount = 0;

        while (ready.Count > 0)
        {
            ct.ThrowIfCancellationRequested();
            var nodeId = ready.Dequeue();
            if (!reachable.Contains(nodeId)) continue;

            var node = nodesById[nodeId];

            inputsForNode.TryGetValue(nodeId, out var mergedInputs);
            var ctx = new NodeExecutionContext
            {
                Inputs = mergedInputs ?? new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase),
            };

            var compiled = _buildService.GetCompiledNode(node.Name, node.Id, _logger)
                          ?? throw new InvalidOperationException($"No implementation for node '{node.Name}'.");

            _buildService.SetProperties(compiled, node.Properties);

            if (compiled == null)
            {
                throw new Exception("Compiled node was null");
            }

            await compiled.Execute(ctx).ConfigureAwait(false);
            executedCount++;

            try
            {
                if (outgoing.TryGetValue(nodeId, out var edgesFromHere))
                {
                    foreach (var edge in edgesFromHere)
                    {
                        if (!reachable.Contains(edge.To)) continue;

                        if (!inputsForNode.TryGetValue(edge.To, out var bag))
                            inputsForNode[edge.To] = bag = NewBag();

                        var sourceOutputs = ctx.Outputs;
                        if (!string.IsNullOrWhiteSpace(edge.FromType) && !string.IsNullOrWhiteSpace(edge.ToType))
                        {
                            if (!sourceOutputs.TryGetValue(edge.FromType, out var val))
                                throw new InvalidOperationException(
                                    $"Output '{edge.FromType}' not found on node {nodeId} ('{node.Name}').");
                            bag[edge.ToType] = val;
                        }
                        else if (string.IsNullOrWhiteSpace(edge.FromType) && !string.IsNullOrWhiteSpace(edge.ToType))
                        {
                            if (sourceOutputs.Count != 1)
                                throw new InvalidOperationException(
                                    $"Ambiguous mapping: node {nodeId} produced {sourceOutputs.Count} outputs, but edge only specifies ToType='{edge.ToType}'. Add FromType.");
                            var only = sourceOutputs.First();
                            bag[edge.ToType] = only.Value;
                        }
                        else if (!string.IsNullOrWhiteSpace(edge.FromType) && string.IsNullOrWhiteSpace(edge.ToType))
                        {
                            if (!sourceOutputs.TryGetValue(edge.FromType, out var val))
                                throw new InvalidOperationException(
                                    $"Output '{edge.FromType}' not found on node {nodeId} ('{node.Name}').");
                            bag[edge.FromType] = val;
                        }
                        else
                        {
                            foreach (var kv in sourceOutputs)
                                bag[kv.Key] = kv.Value;
                        }

                        remainingParents[edge.To] -= 1;
                        if (remainingParents[edge.To] == 0)
                            ready.Enqueue(edge.To);
                        else if (remainingParents[edge.To] < 0)
                            throw new InvalidOperationException($"Negative indegree for node {edge.To}.");
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
            }
        }

        if (executedCount != reachable.Count)
        {
            var blocked = remainingParents.Where(kv => kv.Value > 0).Select(kv => kv.Key);
            throw new InvalidOperationException(
                "Run did not finish the reachable subgraph. Blocked nodes: " + string.Join(", ", blocked));
        }
    }

}
