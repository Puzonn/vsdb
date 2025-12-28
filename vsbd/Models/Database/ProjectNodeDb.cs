public class ProjectNodeDb
{
    public Guid Id { get; set; }
    public Guid ProjectId { get; set; }

    public string SourceCode { get; set; } = "";

    public float PositionX { get; set; }
    public float PositionY { get; set; }

    public ProjectDb Project { get; set; } = null!;
    public List<ProjectPinDb> Pins { get; set; } = [];
}
