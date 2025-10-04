
using vsbd_core;
using System;
using System.Threading.Tasks;
using NetCord.Gateway;
using System.Threading;

[NodeOutput("OnMessageCreate", typeof(MessageCreatedEvent))]
[NodeOutput("OnMessageDelete", typeof(MessageDeletedEvent))]
public sealed class DiscordEntry : NodeDiscordClient, IEventSourceNode
{
    private int _nodeId;

    public override ValueTask Execute(NodeContext ctx)
    {
        ctx.Inputs ??= new(StringComparer.OrdinalIgnoreCase);
        ctx.Outputs ??= new(StringComparer.OrdinalIgnoreCase);
        foreach (var kv in ctx.Inputs)
            ctx.Outputs[kv.Key] = kv.Value;
        return ValueTask.CompletedTask;
    }

    public override void OnNodeCreate(NodeContext context)
    {
        _nodeId = context.NodeId;

    }

    public async Task StartAsync(IEventDispatcher dispatcher, CancellationToken ct)
    {
        await base.Execute(new NodeContext() { CancellationToken = ct });

        Client.MessageCreate += async e =>
        {
            Task ReplyAsync(string reply, CancellationToken t) => e.ReplyAsync(reply, null, t);
            var messageCreatedEvent = new MessageCreatedEvent(e.Content, e.Author.Username, e.Author.IsBot, ReplyAsync);

            dispatcher.Emit(new Seed(_nodeId, new(StringComparer.OrdinalIgnoreCase)
            {
                ["OnMessageCreate"] = messageCreatedEvent
            }));
        };
    }
}
