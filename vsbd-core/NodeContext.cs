public class NodeContext
{
    public Dictionary<string, object> Outputs = new();
    public Dictionary<string, object> Inputs = new();
    public CancellationToken CancellationToken;
    public INodeLogger Logger;
    public int NodeId;
    
    public void SetOutput(string outputName, object value)
    {
        Outputs[outputName] = value;
    }

    public T? GetInput<T>(string inputName)
    {
        return Inputs.TryGetValue(inputName, out var v) ? (T?)v : default;
    }
}