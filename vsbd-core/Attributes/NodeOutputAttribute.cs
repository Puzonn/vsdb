namespace vsbd_core
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    public class NodeOutputAttribute : Attribute
    {
        public string Name { get; }
        public Type Type { get; }

        public NodeOutputAttribute(string name, Type type)
        {
            Name = name;
            Type = type;
        }
    }
}
