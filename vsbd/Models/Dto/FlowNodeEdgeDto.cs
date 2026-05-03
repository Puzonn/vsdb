public sealed class FlowNodeEdgeDto
{
    public required string Id { get; set; }
    public required string SourceId { get; set; }
    public required string TargetId { get; set; }
    public string? SourceHandleId { get; set; }
    public string? TargetHandleId { get; set; }
}