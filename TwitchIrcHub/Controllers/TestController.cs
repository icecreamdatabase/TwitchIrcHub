using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TwitchIrcHub.Authentication.Policies;
using TwitchIrcHub.BackgroundServices;
using TwitchIrcHub.IrcBot.Bot;
using TwitchIrcHub.IrcBot.Irc.DataTypes.ToTwitch;

namespace TwitchIrcHub.Controllers;

[ApiController]
[Route("[controller]")]
[Authorize(Policy = Policies.IsRegisteredApp)]
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
        const int botUserId = 122425204;

        IBotInstance? bot = BotManager.GetBotInstance(botUserId);
        if (bot == null)
            return NoContent();

        List<PrivMsgToTwitch> messages = new();

        for (int i = 0; i < 1; i++)
        {
            messages.Add(new PrivMsgToTwitch(
                    botUserId,
                    "icdb",
                    $"test{i:00}"
                )
            );
        }

        messages.ForEach(message => bot.SendPrivMsg(message));

        return Ok();
    }
}
