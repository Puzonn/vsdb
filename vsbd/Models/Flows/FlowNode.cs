using System.Numerics;

public sealed class FlowNode
{
    public required string Id { get; set; }
    public required string Name { get; set; }
    public required string Type { get; set; }
    public required Vector2 Position { get; set; }
    public required NodeProperty[] Properties { get; set; }
}