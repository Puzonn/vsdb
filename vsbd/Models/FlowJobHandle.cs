using System.Threading.Channels;

public sealed class FlowJobHandle
{
    public required Flow Flow { get; init; }
    public required Channel<Seed> Queue { get; init; }
    public required CancellationTokenSource TokenSource { get; init; }
    public required Task Worker { get; init; }
}