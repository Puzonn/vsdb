public class PathService
{
    private readonly IHostEnvironment _env;

    public PathService(IHostEnvironment env)
    {
        _env = env;
    }

    public string GetScriptsRoot()
    {
        return Path.Combine(
            AppContext.BaseDirectory,
            "DefaultScripts"
        );
    }

    public string GetProjectScriptsRoot(string projectId)
    {
        return Path.Combine(
            _env.ContentRootPath,
            "wwwroot",
            "projects",
            projectId,
            "Scripts"
        );
    }

    public string GetProjectLibrariesRoot(string projectId)
    {
        return Path.Combine(
            _env.ContentRootPath,
            "wwwroot",
            "projects",
            projectId,
            "Libraries"
        );
    }

    public string GetProjectLibrary(string projectId)
    {
        return Path.Combine(
            _env.ContentRootPath,
            "wwwroot",
            "projects",
            projectId,
            "Libraries",
            $"vsbd-nodes-{projectId}"
        );
    }
}