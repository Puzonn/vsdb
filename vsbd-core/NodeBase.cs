namespace vsbd_core
{
    public abstract class NodeBase
    {
        public NodeContext Context { get; set; }

        public abstract ValueTask Execute(NodeExecutionContext execution);
        
        public virtual void OnNodeCreate()
        {

        }
    }
}

