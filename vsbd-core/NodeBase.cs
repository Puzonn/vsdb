namespace vsbd_core
{
    public abstract class NodeBase
    {
        public abstract ValueTask Execute(NodeContext context);
        public virtual void OnNodeCreate(NodeContext context){}
    }
}

