using Microsoft.AspNetCore.Mvc;

namespace TwitchIrcHub.Controllers;

[ApiController]
[Route("[controller]")]
public class PingController : ControllerBase
{
    private readonly ILogger<PingController> _logger;

    public PingController(ILogger<PingController> logger)
    {
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult> Get()
    {
        return Ok("Pong");
    }
}
