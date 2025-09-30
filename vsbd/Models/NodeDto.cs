namespace vsbd.Models;

public class NodeDto
{
    public string Name { get; set; }
    public string[] Outputs { get; set; } = [];
    public string[] Inputs { get; set; } = [];
}