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
            new BotToken("MTI1OTg2ODk2OTA2NjgyMzc1MA.GN0YRp.LWA_7GPhWiF1tuvbwbZk5ZfW_V8ycx2Dsk35LU"),
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