public class NodeLogger : INodeLogger
{
    private readonly ILogger _logger;

    public NodeLogger(ILogger logger)
    {
        _logger = logger;
    }

    public void LogTrace(string message) => _logger.LogInformation(message);
}