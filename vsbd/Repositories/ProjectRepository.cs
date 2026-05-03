using Microsoft.EntityFrameworkCore;

public class ProjectRepository
{
    private readonly AppDbContext _db;
    private readonly BuildService _buildService;

    public ProjectRepository(AppDbContext db, BuildService buildService)
    { 
        _db = db;
        _buildService = buildService;
    }

    public async Task<ProjectLoadResponse> GetLoadedProjectAsync(string projectId)
    {
        var projects = await _db.Projects.ToListAsync();
        var project = await _db.Projects
            .Include(x => x.FlowEdges)
            .Include(x => x.FlowNodes)
            .ThenInclude(x => x.Properties)
            .FirstOrDefaultAsync(x => x.Id == projectId);

        var build = await _buildService.FullCompileInMemory(projectId, true);

        return new ProjectLoadResponse
        {
            Nodes = build.Nodes,
            CreatedAt = project!.CreatedAt,
            Id = project.Id,

            FlowNodes = project.FlowNodes.Select(node => new FlowNodeDto
            {
                Id = node.Id,
                Name = node.Name,
                Type = node.Type,
                Position = new System.Numerics.Vector2
                {
                    X = node.PositionX,
                    Y = node.PositionY,
                },
                Properties = node.Properties.Select(property => new FlowNodePropertyDto
                {
                    Name = property.Name,
                    Type = property.Type,
                    Value = property.Value,
                }).ToArray(),
            }).ToArray(),

            FlowEdges = project.FlowEdges.Select(edge => new FlowNodeEdgeDto
            {
                Id = edge.Id,
                SourceId = edge.SourceId,
                TargetId = edge.TargetId,
                SourceHandleId = edge.SourceHandleId,
                TargetHandleId = edge.TargetHandleId,
            }).ToArray(),
        };
    }

    public async Task<Flow?> BuildFlowAsync(string projectId, CancellationToken ct = default)
    {
        var project = await _db.Projects
            .AsNoTracking()
            .Include(x => x.FlowNodes)
                .ThenInclude(x => x.Properties)
            .Include(x => x.FlowEdges)
            .FirstOrDefaultAsync(x => x.Id == projectId, ct);

        if (project is null)
        {
            return null;
        }

        var nodes = project.FlowNodes
            .Select(node => new FlowRuntimeNode
            {
                Id = node.Id,
                Name = node.Name,
                Type = node.Type,
                Properties = node.Properties.ToDictionary(
                    property => property.Name,
                    property => property.Value
                ),
            })
            .ToList();

        var edges = project.FlowEdges
            .Select(edge => new FlowRuntimeEdge
            {
                Id = edge.Id,
                SourceId = edge.SourceId,
                TargetId = edge.TargetId,
                SourceHandleId = edge.SourceHandleId,
                TargetHandleId = edge.TargetHandleId,
            })
            .ToList();

        return new Flow
        {
            ProjectId = project.Id,
            Nodes = nodes,
            Edges = edges,
        };
    }
}