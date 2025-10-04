using System.Threading.Channels;

public sealed class ChannelEventDispatcher : IEventDispatcher
{
    private readonly Channel<Seed> _queue;
    public ChannelEventDispatcher(Channel<Seed> queue) => _queue = queue;
    public void Emit(Seed seed) => _queue.Writer.TryWrite(seed);
}
