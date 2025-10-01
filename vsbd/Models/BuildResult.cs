public class BuildResult
{
    public string? Error { get; set; }
    public bool Success { get; set; }

    public BuildResult(bool success, string? error)
    {
        Error = error;
        Success = success;
    }
}