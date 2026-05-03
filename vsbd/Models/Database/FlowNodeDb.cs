public class FlowNodeDb
{
     public required string Id { get; set; }

    public required string ProjectId { get; set; }
    public ProjectDb Project { get; set; } = null!;

    public required string Name { get; set; }
    public required string Type { get; set; }

    public float PositionX { get; set; }
    public float PositionY { get; set; }

    public List<FlowNodePropertyDb> Properties { get; set; } = [];
}
