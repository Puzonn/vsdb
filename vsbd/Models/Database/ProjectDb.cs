public sealed class ProjectDb
{
    public required string Id { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public List<FlowNodeDb> FlowNodes { get; set; } = [];
    public List<FlowNodeEdgeDb> FlowEdges { get; set; } = [];
}