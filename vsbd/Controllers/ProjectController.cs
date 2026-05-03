using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

[ApiController]
[Route("[controller]")]
public class ProjectController : ControllerBase
{
    private readonly BuildService _buildService;
    private readonly ProjectRepository _projectRepository;
    private readonly AppDbContext _db;
    private readonly ILogger<ProjectController> _logger;
    private readonly IWebHostEnvironment _env;
    private readonly PathService _path;

    public ProjectController(ILogger<ProjectController> logger, IWebHostEnvironment env,
     BuildService buildService, AppDbContext db, PathService path, ProjectRepository projectRepository)
    {
        _projectRepository = projectRepository;
        _path = path;
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

    [HttpGet("{projectId}")]
    public async Task<ActionResult<ProjectLoadResponse>> GetProject(string projectId)
    {
        return Ok(await _projectRepository.GetLoadedProjectAsync(projectId));
    }

    [HttpPost("create")]
    public async Task<ActionResult<ProjectCreationResponse>> CreateProject()
    {
        var projectId = Guid.NewGuid().ToString("N");

        Directory.CreateDirectory(_path.GetProjectLibrariesRoot(projectId));
        Directory.CreateDirectory(_path.GetProjectScriptsRoot(projectId));

        var build = await _buildService.FullCompileInMemory(projectId, true);

        if (!build.Result.Success)
            return Ok(new ProjectRunResult(false, build.Result.Error, null));

        return Ok(new ProjectCreationResponse(
            ProjectId: projectId,
            Nodes: build.Nodes!
        ));
    }

    [HttpPost("save/{projectId}")]
    public async Task<ActionResult> SaveProject(string projectId, [FromBody] SaveProjectDto request)
    {
        var project = await _db.Projects
            .Include(x => x.FlowNodes)
                .ThenInclude(x => x.Properties)
            .Include(x => x.FlowEdges)
            .FirstOrDefaultAsync(x => x.Id == projectId);

        if (project is null)
        {
            project = new ProjectDb
            {
                Id = projectId,
                CreatedAt = DateTime.UtcNow,
            };

            _db.Projects.Add(project);
        }

        var root = _path.GetProjectScriptsRoot(projectId);

        if (!Directory.Exists(root))
        {
            Directory.CreateDirectory(root);
        }

        foreach (var file in request.Libraries)
        {
            if (string.IsNullOrWhiteSpace(file.Name))
                continue;

            var safeName = Path.GetFileName(file.Name);

            var filePath = Path.Combine(root, safeName + ".cs");

            await System.IO.File.WriteAllTextAsync(
                filePath,
                file.SourceCode
            );
        }

        _db.FlowNodeProperties.RemoveRange(
            project.FlowNodes.SelectMany(x => x.Properties)
        );

        _db.FlowNodes.RemoveRange(project.FlowNodes);
        _db.FlowEdges.RemoveRange(project.FlowEdges);

        project.FlowNodes = request.FlowNodes
            .Select(node => new FlowNodeDb
            {
                Id = node.Id,
                ProjectId = project.Id,
                Name = node.Name,
                Type = node.Type,
                PositionX = node.Position.X,
                PositionY = node.Position.Y,

                Properties = node.Properties
                    .Select(property => new FlowNodePropertyDb
                    {
                        FlowNodeId = node.Id,
                        Name = property.Name,
                        Type = property.Type,
                        Value = property.Value,
                    })
                    .ToList(),
            })
            .ToList();

        project.FlowEdges = request.FlowEdges
            .Select(edge => new FlowNodeEdgeDb
            {
                Id = edge.Id,
                ProjectId = project.Id,
                SourceId = edge.SourceId,
                TargetId = edge.TargetId,
                SourceHandleId = edge.SourceHandleId,
                TargetHandleId = edge.TargetHandleId,
            })
            .ToList();

        await _db.SaveChangesAsync();

        return Ok();
    }

    [HttpPost("compile/{projectId}")]
    public async Task<ActionResult<ProjectRunResult>> CompileProject(string projectId)
    {
        var scripts = _path.GetProjectScriptsRoot(projectId);
        var libraries = _path.GetProjectLibrariesRoot(projectId);

        if (!Directory.Exists(libraries))
        {
            Directory.CreateDirectory(libraries);
        }

        if (!Directory.Exists(scripts))
        {
            return NotFound("Project not found");
        }

        var build = await _buildService.FullCompileInMemory(projectId, true);

        if (!build.Result.Success)
        {
            return Ok(new ProjectRunResult(false, build.Result.Error, null));
        }

        return Ok(new ProjectRunResult(true, null, build.Nodes));
    }
}