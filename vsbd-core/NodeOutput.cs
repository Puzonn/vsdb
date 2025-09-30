namespace vsbd.Attributes;

[AttributeUsage(AttributeTargets.Class)]
public class NodeOutput : Attribute
{
    public NodeOutput(params Type[] outputs)
    {
        Outputs = outputs;
    }

    public Type[] Outputs { get; set; }
}