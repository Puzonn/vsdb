public class Node
{
    public string Name { get; set; }
    public string[] Inputs { get; set; } = [];
    public string[] Outputs { get; set; } = [];

    public Node(string name)
    {
        Name = name;
    }
}