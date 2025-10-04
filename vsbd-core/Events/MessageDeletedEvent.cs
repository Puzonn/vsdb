public sealed record MessageDeletedEvent(
    string Message,
    string Author,
    Func<string, CancellationToken, Task> ReplyAsync
);