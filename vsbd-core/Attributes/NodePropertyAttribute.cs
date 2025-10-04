[AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
public class NodePropertyAttribute : Attribute
{
    public string Name { get; }
    public object DefaultValue { get; }
    public Type Type { get; }

    public NodePropertyAttribute(string name, Type type, object defaultValue)
    {
        Name = name;
        DefaultValue = defaultValue;
        Type = type;
    }
}