using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("[controller]")]
public class BuildController : ControllerBase
{
    private readonly BuildService _buildService;

    public BuildController(BuildService buildService)
    {
        _buildService = buildService;
    }

    [HttpPost("/compile")]
    public async Task<BuildResult> Compile()
    {
        return await _buildService.Compile();
    }
    
    [HttpGet("/nodes")]
    public async Task<ActionResult<NodeResult>> GetNodes()
    {
        return Ok(await _buildService.GetNodes());
    }
}