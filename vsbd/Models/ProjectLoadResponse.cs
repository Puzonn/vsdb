public class ProjectLoadResponse
{
    public required string Id { get; set; }
    public required DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public required IEnumerable<FlowNodeDto> FlowNodes { get; set; } = [];
    public required IEnumerable<FlowNodeEdgeDto> FlowEdges { get; set; } = [];
    public required IEnumerable<Node> Nodes { get; set; } = [];

}