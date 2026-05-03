public class SaveProjectDto
{
    public required ProjectFile[] Libraries { get; set; }
    public required FlowNode[] FlowNodes { get; set; }
    public required FlowEdge[] FlowEdges { get; set; }
}