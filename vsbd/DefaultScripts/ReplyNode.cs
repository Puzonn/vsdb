using vsbd_core;
using System;
using System.Threading.Tasks;

[NodeInput(name: "Message", typeof(MessageCreatedEvent))]
public class ReplyNode : NodeBase
{
    [NodeProperty("MessageTemplate", typeof(string), "Hello {0}")]
    public string Message { get; set; }

    public override async ValueTask Execute(NodeExecutionContext execution)
    {
        var e = execution.GetInput<MessageCreatedEvent>("Message");

        if (e.IsBot)
        {
            return;
        }

        string formated = string.Format(Message, e.Author);
        
        await e.ReplyAsync(formated, Context.CancellationToken);
    }
}