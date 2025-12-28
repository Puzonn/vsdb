public class ProjectEdgeDb
{
    public Guid Id { get; set; }

    public Guid ProjectId { get; set; }
    public ProjectDb Project { get; set; } = null!;

    public Guid SourceNodeId { get; set; }
    public ProjectNodeDb SourceNode { get; set; } = null!;

    public Guid SourcePinId { get; set; }
    public ProjectPinDb SourcePin { get; set; } = null!;

    public Guid TargetNodeId { get; set; }
    public ProjectNodeDb TargetNode { get; set; } = null!;

    public Guid TargetPinId { get; set; }
    public ProjectPinDb TargetPin { get; set; } = null!;
}
