public sealed class FlowNodePropertyDb
{
    public int Id { get; set; }

    public required string FlowNodeId { get; set; }
    public FlowNodeDb FlowNode { get; set; } = null!;

    public required string Name { get; set; }
    public required string Type { get; set; }

    public string? Value { get; set; }
}