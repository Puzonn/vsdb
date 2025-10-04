using vsbd_core;
using System;
using System.Threading.Tasks;

[NodeInput(name: "Message", typeof(MessageCreatedEvent))]
public class ReplyNode : NodeBase
{
    [NodeProperty("MessageTemplate", typeof(string), "Hello {0}")]
    public string Message { get; }

    public override async ValueTask Execute(NodeContext context)
    {
        var e = context.GetInput<MessageCreatedEvent>("Message");

        if (e.IsBot)
        {
            return;
        }

        await e.ReplyAsync(Message, context.CancellationToken);
    }
}