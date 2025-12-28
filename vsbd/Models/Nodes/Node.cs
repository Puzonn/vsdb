public class Node
{
    public string Name { get; set; }
    public string SourceCode { get; set; } = "";
    public NodeInput[] Inputs { get; set; } = [];
    public NodeOutput[] Outputs { get; set; } = [];
    public NodeProperty[] Properties { get; set; } = [];
}