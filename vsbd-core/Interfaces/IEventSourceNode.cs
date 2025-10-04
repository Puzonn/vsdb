public interface IEventSourceNode
{
    Task StartAsync(IEventDispatcher dispatcher, CancellationToken ct);
}