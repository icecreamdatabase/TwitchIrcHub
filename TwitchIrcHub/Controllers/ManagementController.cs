using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TwitchIrcHub.Authentication.Policies;
using TwitchIrcHub.Model;

namespace TwitchIrcHub.Controllers;

[ApiController]
[Route("[controller]")]
[Authorize(Policy = Policies.IsRegisteredApp)]
public class ManagementController: ControllerBase
{
    private readonly ILogger<ManagementController> _logger;

    public ManagementController(ILogger<ManagementController> logger)
    {
        _logger = logger;
    }

    [HttpGet("GetDb")]
    public ActionResult GetDb([FromServices] IrcHubDbContext ircHubDbContext)
    {
        ircHubDbContext.Database.EnsureCreated();
        return Ok(ircHubDbContext.Database.GenerateCreateScript());
    }
}