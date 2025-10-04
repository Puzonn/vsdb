using NetCord;
using NetCord.Gateway;
using vsbd_core;

public class NodeDiscordClient : NodeBase
{
    public GatewayClient? Client;
    private Task? _runTask;

    public override ValueTask Execute(NodeContext context)
    {
        Client = new GatewayClient(
            new BotToken("MTI1OTg2ODk2OTA2NjgyMzc1MA.G2lD_9.2LX-F8g0F6MkGQktPYoCVPBSlWgEtKE_sZvHUc"),
            new GatewayClientConfiguration { });

        _runTask = Task.Run(async () =>
        {
            try
            {
                await Client.StartAsync().ConfigureAwait(false);

                await Task.Delay(Timeout.InfiniteTimeSpan, context.CancellationToken)
                          .ConfigureAwait(false);
            }
            catch (Exception ex) { }
        });

        return ValueTask.CompletedTask;
    }
}