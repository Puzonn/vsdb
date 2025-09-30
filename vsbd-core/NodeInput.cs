namespace vsbd_core;

[AttributeUsage(AttributeTargets.Class)]
public class NodeInput : Attribute
{
    public NodeInput(params Type[] inputs)
    {
        Inputs = inputs;
    }

    public Type[] Inputs { get; set; }
}