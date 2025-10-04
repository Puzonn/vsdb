public sealed record MessageCreatedEvent(
    string Message,
    string Author,
    bool IsBot,
    Func<string, CancellationToken, Task> ReplyAsync
);