namespace vsbd_core
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public class NodeInputAttribute : Attribute
    {
        public string Name { get; }
        public Type Type { get; }

        public NodeInputAttribute(string name, Type type)
        {
            Name = name;
            Type = type;
        }
    }
}
