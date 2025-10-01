public class NodeResult
{
    public string? Error { get; set; }
    public bool Success { get; set; }
    public Node[] Nodes { get; set; } = [];
    
    public NodeResult(bool success, string? error = null, Node?[]? nodes = null)
    {
        Error = error;
        Success = success;

        if (nodes != null)
        {
            Nodes = nodes!;
        }
    }
}