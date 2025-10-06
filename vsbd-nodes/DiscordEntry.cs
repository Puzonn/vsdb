
using vsbd_core;
using System;
using System.Threading.Tasks;
using NetCord.Gateway;
using System.Threading;

[NodeOutput("OnMessageCreate", typeof(MessageCreatedEvent))]
[NodeOutput("OnMessageDelete", typeof(MessageDeletedEvent))]
public sealed class DiscordEntry : NodeDiscordClient, IEventSourceNode
{
    public override ValueTask Execute(NodeExecutionContext execution)
    {
        execution.Inputs ??= new(StringComparer.OrdinalIgnoreCase);
        execution.Outputs ??= new(StringComparer.OrdinalIgnoreCase);

        foreach (var kv in execution.Inputs)
            execution.Outputs[kv.Key] = kv.Value;
        return ValueTask.CompletedTask;
    }

    public async Task StartAsync(IEventDispatcher dispatcher, CancellationToken ct)
    {
        await base.Execute(null);

        Client.MessageCreate += async e =>
        {    
            Task ReplyAsync(string reply, CancellationToken t) => e.ReplyAsync(reply, null, t);
            var messageCreatedEvent = new MessageCreatedEvent(e.Content, e.Author.Username, e.Author.IsBot, ReplyAsync);

            dispatcher.Emit(new Seed(Context.NodeId, new(StringComparer.OrdinalIgnoreCase)
            {
                ["OnMessageCreate"] = messageCreatedEvent
            }));
        };
    }
}
