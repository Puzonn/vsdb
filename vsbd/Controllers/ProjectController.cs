using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

[ApiController]
[Route("[controller]")]
public class ProjectController : ControllerBase
{
    private readonly BuildService _buildService;
    private readonly AppDbContext _db;
    private readonly ILogger<ProjectController> _logger;
    private readonly IWebHostEnvironment _env;

    public ProjectController(ILogger<ProjectController> logger, IWebHostEnvironment env, BuildService buildService, AppDbContext db)
    {
        _db = db;
        _buildService = buildService;
        _env = env;
        _logger = logger;
    }

    [HttpGet("all")]
    public async Task<ActionResult<Project[]>> GetProjects()
    {
        var res = await _db.Projects.ToListAsync();

        return Ok(res);
    }

    [HttpPost("create")]
    public async Task<ActionResult<ProjectCreationResponse>> CreateProject()
    {
        var projectId = Guid.NewGuid().ToString("N");

        var defaultScriptsRoot = Path.Combine(
            AppContext.BaseDirectory,
            "DefaultScripts"
        );

        var scripts = await Task.WhenAll(
    Directory.EnumerateFiles(defaultScriptsRoot, "*.cs", SearchOption.AllDirectories)
        .Select(async file => new ScriptSource(
            FileName: Path.GetFileName(file),
            Source: await System.IO.File.ReadAllTextAsync(file)
        ))
);
        var build = _buildService.CompileInMemory(scripts, projectId, out var assemblyBytes);



        if (!build.Success)
            return Ok(new ProjectRunResult(false, build.Error, null));

        var nodesResult = await _buildService.GetNodes(
    assemblyBytes,
    scripts,
    attachSourceCode: true
);


        return Ok(new ProjectCreationResponse(
            ProjectId: projectId,
            Nodes: nodesResult.Nodes!
        ));
    }

    private static void CopyDirectory(string sourceDir, string targetDir)
    {
        Directory.CreateDirectory(targetDir);

        foreach (var file in Directory.GetFiles(sourceDir))
        {
            var dest = Path.Combine(targetDir, Path.GetFileName(file));
            System.IO.File.Copy(file, dest, overwrite: false);
        }

        foreach (var dir in Directory.GetDirectories(sourceDir))
        {
            var dest = Path.Combine(targetDir, Path.GetFileName(dir));
            CopyDirectory(dir, dest);
        }
    }


    [HttpPost("send/{projectId}")]
    public async Task<ActionResult> SendProject(string projectId, [FromBody] ProjectFile[] libraries)
    {
        var root = Path.Combine(
            _env.ContentRootPath,
            "wwwroot",
            "projects",
            projectId,
            "Scripts"
        );

        if (!Directory.Exists(root))
            return NotFound("Project not found");

        foreach (var file in libraries)
        {
            if (string.IsNullOrWhiteSpace(file.Name))
                continue;

            var safeName = Path.GetFileName(file.Name);

            var filePath = Path.Combine(root, safeName);

            await System.IO.File.WriteAllTextAsync(
                filePath,
                file.SourceCode
            );
        }

        return Ok();
    }

    [HttpPost("compile/{projectId}")]
    public async Task<ActionResult<ProjectRunResult>> CompileProject(string projectId)
    {
        return Ok();
        // var projectScriptsDir = Path.Combine(
        //     _env.ContentRootPath,
        //     "wwwroot",
        //     "projects",
        //     projectId,
        //     "Scripts"
        // );

        // if (!Directory.Exists(projectScriptsDir))
        //     return NotFound("Project not found");

        // var build = await _buildService.Compile(projectScriptsDir, projectId);

        // if (!build.Success)
        //     return Ok(new ProjectRunResult(false, build.Error, null));

        // var nodesResult = await _buildService.GetNodes(projectId, true);

        // if (!nodesResult.Success)
        // {
        //     return Ok(new ProjectRunResult(false, nodesResult.Error, null));
        // }

        // return Ok(new ProjectRunResult(true, null, nodesResult.Nodes));
    }
}