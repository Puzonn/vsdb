public sealed class FlowRuntimeEdge
{
    public required string Id { get; init; }

    public required string SourceId { get; init; }
    public required string TargetId { get; init; }

    public string? SourceHandleId { get; init; }
    public string? TargetHandleId { get; init; }
}