using TwitchIrcHub.IrcBot.Helper;
using TwitchIrcHub.IrcBot.Irc.IrcPoolManager;
using TwitchIrcHub.Model;

namespace TwitchIrcHub.IrcBot.Bot;

public class BotInstance : IBotInstance
{
    private readonly ILogger<BotInstance> _logger;
    private readonly IFactory<IIrcPoolManager> _ircPoolManagerFactor;
    private readonly IrcHubDbContext _ircHubDbContext;
    private IIrcPoolManager _ircPoolManager;

    public int BotUserId { get; private set; }

    public BotInstance(ILogger<BotInstance> logger, IFactory<IIrcPoolManager> ircPoolManagerFactor,
        IrcHubDbContext ircHubDbContext)
    {
        _logger = logger;
        _ircPoolManagerFactor = ircPoolManagerFactor;
        _ircHubDbContext = ircHubDbContext;
    }

    public void Init(int botUserId)
    {
        BotUserId = botUserId;
        Model.Schema.Bot? bot = _ircHubDbContext.Bots.Find(botUserId);
        _ircPoolManager = _ircPoolManagerFactor.Create();
        _ircPoolManager.Init("icdbot", "");
    }

    public void Update()
    {
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);
    }
}
