using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TwitchIrcHub.Authentication.Policies;

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
    [Authorize(Policy = Policies.IsRegisteredApp)]
    public async Task<ActionResult> Get()
    {
        return Ok();
    }
}