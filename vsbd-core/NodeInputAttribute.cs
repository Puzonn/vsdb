namespace vsbd_core
{
    [AttributeUsage(AttributeTargets.Class)]
    public class NodeInputAttribute : Attribute
    {
        public Type[] Inputs;

        public NodeInputAttribute(params Type[] inputs)
        {
            Inputs = inputs;
        }
    }
}
