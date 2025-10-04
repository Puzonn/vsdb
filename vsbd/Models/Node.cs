public class Node
{
    public string Name { get; set; }
    public NodeInput[] Inputs { get; set; } = [];
    public NodeOutput[] Outputs { get; set; } = [];

    public Node(string name)
    {
        Name = name;
    }
}