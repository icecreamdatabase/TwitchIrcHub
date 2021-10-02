using TwitchIrcHub.IrcBot.Helper;
using TwitchIrcHub.IrcBot.Irc.DataTypes;
using TwitchIrcHub.IrcBot.Irc.IrcClient;

namespace TwitchIrcHub.IrcBot.Irc.IrcPoolManager;

public class IrcPoolManager : IIrcPoolManager
{
    private readonly ILogger<IrcPoolManager> _logger;
    private readonly IFactory<IIrcClient> _ircClientFactory;

    private readonly List<IIrcClient> _ircSendClients = new();
    private readonly List<IIrcClient> _ircReceiveClients = new();
    private const int MaxChannelsPerIrcClient = 200;

    public BasicBucket AuthenticateBucket { get; } = new(20, 10);
    public BasicBucket JoinBucket { get; } = new(20, 10);

    protected internal int BotUserId => 0;
    public string BotUsername => _username;
    public string BotOauth => _oauth;

    public IrcPoolManager(ILogger<IrcPoolManager> logger, IFactory<IIrcClient> ircClientFactory)
    {
        _logger = logger;
        _ircClientFactory = ircClientFactory;
    }

    private string _username;
    private string _oauth;

    public async Task Init(string username, string oauth)
    {
        _username = username;
        _oauth = oauth;
        int sendConnectionCount = Limits.NormalBot.SendConnections;
        for (int i = 0; i < sendConnectionCount; i++)
        {
            IIrcClient ircClient = _ircClientFactory.Create();
            ircClient.Init(this);
            _ircSendClients.Add(ircClient);
        }

        SetChannel("icdb", "pajlada", "theonemanny", "amy_magic1", "divinecarly", "channelpoints_tts",
            //"supibot", "cineafx", "icecreamdatabase", "icdbot", "weneedmoredankbots", "sshsierra", "omgmochi",
            //"forsen", "nymn", "swushwoi", "nani", "ceejaey", "supinic", "sunred_", "teischente", "tranred",
            //"vesp3r", "empyrione", "chickybro", "fabzeef", "romydank", "robbaz", "tene__", "nuuls", "hugo_one",
            "griphthefrog", "x0r6zt", "zahra", "teyn", "zelbina");

        await Task.Delay(10000);
            
        Join("nuuls");
    }

    public void SetChannel(params string[] channelNames)
    {
        Part(_ircReceiveClients.SelectMany(client => client.Channels).Except(channelNames).ToArray());
        Join(channelNames.Except(_ircReceiveClients.SelectMany(client => client.Channels)).ToArray());
    }

    public void Join(params string[] channelNames)
    {
        List<string> channels = channelNames.ToList();
        foreach (IIrcClient ircClient in _ircReceiveClients
            .Where(ircClient => ircClient.Channels.Count >= MaxChannelsPerIrcClient)
        )
        {
            int freeSlots = MaxChannelsPerIrcClient - ircClient.Channels.Count;
            List<string> newChannels = channels.Take(freeSlots).ToList();
            channels.RemoveRange(0, freeSlots);
            ircClient.Channels.AddRange(newChannels);
        }

        // Need new IrcClient
        while (channels.Count > 0)
        {
            IIrcClient ircClient = _ircClientFactory.Create();
            ircClient.Init(this);
            _ircReceiveClients.Add(ircClient);

            List<string> newChannels = channels.Take(Math.Min(channels.Count, MaxChannelsPerIrcClient)).ToList();
            channels.RemoveRange(0, Math.Min(channels.Count, MaxChannelsPerIrcClient));
            ircClient.Channels.AddRange(newChannels);
        }
    }

    public void Part(params string[] channelNames)
    {
        foreach (string channelName in channelNames)
        {
            IIrcClient ircClient = GetIrcClientOfChannel(channelName);
            if (ircClient == null) continue;

            ircClient.Channels.Remove(channelName);
            if (ircClient.Channels.Count > 0) continue;

            //ircClient.Shutdown();
            //_ircReceiveClients.Remove(ircClient);
        }
    }

    public void SendMessage(string channel, string message)
    {
    }

    private IIrcClient GetIrcClientOfChannel(string channel)
    {
        return _ircReceiveClients.FirstOrDefault(client => client.Channels.Contains(channel));
    }

    public Task RefreshAuth()
    {
        return null;
    }

    public async Task NewPrivMsg(IrcMessage ircMessage)
    {
        _logger.LogInformation("PRIVMSG: {Channel}: {Msg}",
            ircMessage.IrcParameters[0], ircMessage.IrcParameters[1]);
    }
}