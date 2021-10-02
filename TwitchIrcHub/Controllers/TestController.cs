using Microsoft.AspNetCore.Mvc;

namespace TwitchIrcHub.Controllers;

[ApiController]
[Route("[controller]")]
public class TestController : ControllerBase
{
    private readonly ILogger<TestController> _logger;

    public TestController(ILogger<TestController> logger)
    {
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult> Get()
    {
        return Ok();
    }
}