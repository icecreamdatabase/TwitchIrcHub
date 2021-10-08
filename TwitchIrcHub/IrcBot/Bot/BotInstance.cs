using TwitchIrcHub.IrcBot.Helper;
using TwitchIrcHub.IrcBot.Irc.IrcPoolManager;

namespace TwitchIrcHub.IrcBot.Bot;

public class BotInstance : IBotInstance
{
    private readonly ILogger<BotInstance> _logger;
    private readonly IFactory<IIrcPoolManager> _ircPoolManagerFactor;
    private readonly IFactory<IBotInstanceData> _botInstanceDataFactory;
    private IIrcPoolManager _ircPoolManager = null!;
    public IBotInstanceData BotInstanceData { get; private set; } = null!;

    public BotInstance(ILogger<BotInstance> logger, IFactory<IIrcPoolManager> ircPoolManagerFactor,
        IFactory<IBotInstanceData> botInstanceDataFactory)
    {
        _logger = logger;
        _ircPoolManagerFactor = ircPoolManagerFactor;
        _botInstanceDataFactory = botInstanceDataFactory;
    }

    public void Init(int botUserId)
    {
        BotInstanceData = _botInstanceDataFactory.Create();
        BotInstanceData.Init(botUserId);
        _ircPoolManager = _ircPoolManagerFactor.Create();
        _ircPoolManager.Init(this);
    }

    public void IntervalPing()
    {
        BotInstanceData.IntervalPing();
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);
    }
}
