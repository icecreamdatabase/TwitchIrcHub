using TwitchIrcHub.IrcBot.Helper;
using TwitchIrcHub.IrcBot.Irc.DataTypes.FromTwitch;
using TwitchIrcHub.IrcBot.Irc.DataTypes.ToTwitch;
using TwitchIrcHub.IrcBot.Irc.IrcPoolManager;
using TwitchIrcHub.IrcBot.PubSub.PubSubPoolManager;

namespace TwitchIrcHub.IrcBot.Bot;

public class BotInstance : IBotInstance
{
    private readonly ILogger<BotInstance> _logger;
    private readonly IFactory<IIrcPoolManager> _ircPoolManagerFactor;
    private readonly IFactory<IBotInstanceData> _botInstanceDataFactory;
    private readonly IFactory<IPubSubPoolManager> _pubSubPoolManagerFactor;
    private IIrcPoolManager _ircPoolManager = null!;
    private IPubSubPoolManager _pubSubPoolManager = null!;
    public IBotInstanceData BotInstanceData { get; private set; } = null!;

    public BotInstance(ILogger<BotInstance> logger,
        IFactory<IIrcPoolManager> ircPoolManagerFactor,
        IFactory<IBotInstanceData> botInstanceDataFactory,
        IFactory<IPubSubPoolManager> pubSubPoolManagerFactor
    )
    {
        _logger = logger;
        _ircPoolManagerFactor = ircPoolManagerFactor;
        _botInstanceDataFactory = botInstanceDataFactory;
        _pubSubPoolManagerFactor = pubSubPoolManagerFactor;
    }

    public async Task Init(int botUserId)
    {
        BotInstanceData = _botInstanceDataFactory.Create();
        await BotInstanceData.Init(botUserId);
        _ircPoolManager = _ircPoolManagerFactor.Create();
        await _ircPoolManager.Init(this);
        _pubSubPoolManager = _pubSubPoolManagerFactor.Create();
        await _pubSubPoolManager.Init(this);
    }

    public async Task IntervalPing()
    {
        await BotInstanceData.IntervalPing();
        await _ircPoolManager.IntervalPing();
        await _pubSubPoolManager.IntervalPing();
    }

    public void SendPrivMsg(PrivMsgToTwitch privMsg)
    {
        _ircPoolManager.SendMessage(privMsg);
    }

    public Task<List<IrcUserState>> GetUserStatesForChannels(List<int> roomIds)
    {
        return _ircPoolManager.UserStateCache.GetBasedOnRoomIds(roomIds);
    }

    public IrcGlobalUserState? GetGlobalUserState() => _ircPoolManager.GlobalUserStateCache.LastGlobalUserState;

    public void Dispose()
    {
        GC.SuppressFinalize(this);
        //TODO
    }
}
