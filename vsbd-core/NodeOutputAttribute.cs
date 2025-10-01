namespace vsbd_core
{
    [AttributeUsage(AttributeTargets.Class)]
    public class NodeOutputAttribute : Attribute
    {
        public Type[] Outputs;

        public NodeOutputAttribute(params Type[] outputs)
        {
            Outputs = outputs;
        }
    }
}
