public class ProjectDb
{
    public Guid Id { get; set; }
    public List<ProjectNodeDb> Nodes { get; set; } = [];
    public List<ProjectEdgeDb> Edges { get; set; } = [];
}
