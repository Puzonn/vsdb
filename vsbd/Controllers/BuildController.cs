using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("[controller]")]
public class BuildController : ControllerBase
{
    private readonly BuildService _buildService;
    private readonly ILogger<BuildController> _logger;

    public BuildController(BuildService buildService, ILogger<BuildController> logger)
    {
        _buildService = buildService;
        _logger = logger;
    }

    [HttpPost("/compile")]
    public async Task<ActionResult<BuildResult>> Compile()
    {
        var result = await _buildService.Compile();

        if (!result.Success)
        {
            return Problem(result.Error);
        }
        return Ok();
    }

    [HttpGet("/nodes")]
    public async Task<ActionResult<NodeResult>> GetNodes()
    {
        return Ok(await _buildService.GetNodes());
    }
}