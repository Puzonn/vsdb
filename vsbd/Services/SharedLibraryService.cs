using System.Diagnostics;
using Microsoft.Extensions.Options;

public sealed class SharedLibraryService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<SharedLibraryService> _logger;
    private readonly PathService _path;

    public SharedLibraryService(
        IConfiguration configuration,
        ILogger<SharedLibraryService> logger, PathService path)
    {
        _configuration = configuration;
        _logger = logger;
        _path = path;
    }

    public async Task<string> BuildSharedLibrary(
        CancellationToken ct, string projectId)
    {
        string libraryDirectoryPath = _configuration.GetValue<string>("Paths:SharedLibraryProjectPath")!;

        var psi = new ProcessStartInfo
        {
            FileName = "dotnet",
            Arguments = "build -c Release",
            WorkingDirectory = libraryDirectoryPath,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
        };

        using var process = Process.Start(psi);

        if (process is null)
        {
            throw new InvalidOperationException("Could not start dotnet build.");
        }

        var stdoutTask = process.StandardOutput.ReadToEndAsync(ct);
        var stderrTask = process.StandardError.ReadToEndAsync(ct);

        await process.WaitForExitAsync(ct);

        var stdout = await stdoutTask;
        var stderr = await stderrTask;

        if (process.ExitCode != 0)
        {
            throw new InvalidOperationException(
                $"dotnet build failed.{Environment.NewLine}{stdout}{Environment.NewLine}{stderr}");
        }

        var sourceDllPath = Path.Combine(
            libraryDirectoryPath,
            "bin",
            "Release",
            "net9.0",
            "vsbd-core.dll"
        );

        var outputDir = _path.GetProjectLibrariesRoot(projectId);

        if (!File.Exists(sourceDllPath))
            throw new FileNotFoundException("Built DLL was not found.", sourceDllPath);

        var targetDllPath = Path.Combine(
            outputDir,
            "vsbd-core.dll"
        );

        File.Copy(sourceDllPath, targetDllPath, overwrite: true);

        return stdout;
    }
}