using NetCord;
using NetCord.Gateway;
using vsbd_core;

public class NodeDiscordClient : NodeBase
{
    public GatewayClient? Client;
    private Task? _runTask;

    public override ValueTask Execute(NodeExecutionContext execution)
    {
        Client = new GatewayClient(
            new BotToken(""),
            new GatewayClientConfiguration { });

        _runTask = Task.Run(async () =>
        {
            try
            {
                await Client.StartAsync().ConfigureAwait(false);

                await Task.Delay(Timeout.InfiniteTimeSpan, Context.CancellationToken)
                          .ConfigureAwait(false);
            }
            catch (Exception ex) { }
        });

        return ValueTask.CompletedTask;
    }
}