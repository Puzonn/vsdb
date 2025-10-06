public class NodeExecutionContext
{
    public Dictionary<string, object> Outputs = new();
    public void SetOutput(string outputName, object value)
    {
        Outputs[outputName] = value;
    }

    public Dictionary<string, object> Inputs = new();
    public T? GetInput<T>(string inputName)
    {
        return Inputs.TryGetValue(inputName, out var v) ? (T?)v : default;
    }
}