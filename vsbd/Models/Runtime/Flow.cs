public class Flow
{
    public required string ProjectId;
    public required IReadOnlyCollection<FlowRuntimeNode> Nodes;
    public required IReadOnlyCollection<FlowRuntimeEdge> Edges { get; init; }
}