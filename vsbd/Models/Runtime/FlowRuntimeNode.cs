public sealed class FlowRuntimeNode
{
    public required string Id { get; init; }
    public required string Name { get; init; }
    public required string Type { get; init; }
    public required IReadOnlyDictionary<string, string?> Properties { get; init; }
}
