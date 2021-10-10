using Microsoft.EntityFrameworkCore;
using TwitchIrcHub.IrcBot.Bot;
using TwitchIrcHub.IrcBot.Helper;
using TwitchIrcHub.IrcBot.Irc.DataTypes;
using TwitchIrcHub.IrcBot.Irc.IrcClient;
using TwitchIrcHub.Model;

namespace TwitchIrcHub.IrcBot.Irc.IrcPoolManager;

public class IrcPoolManager : IIrcPoolManager
{
    private readonly ILogger<IrcPoolManager> _logger;
    private readonly IFactory<IIrcClient> _ircClientFactory;
    private readonly IrcHubDbContext _ircHubDbContext;

    private readonly List<IIrcClient> _ircSendClients = new();
    private readonly List<IIrcClient> _ircReceiveClients = new();
    private const int MaxChannelsPerIrcClient = 200;

    public BasicBucket AuthenticateBucket { get; } = new(20, 10);
    public BasicBucket JoinBucket { get; } = new(20, 10);

    public string BotUsername => _botInstance.BotInstanceData.UserName;
    public string BotOauth => _botInstance.BotInstanceData.AccessToken;

    private BotInstance _botInstance = null!;

    public IrcPoolManager(ILogger<IrcPoolManager> logger, IServiceProvider serviceProvider,
        IFactory<IIrcClient> ircClientFactory)
    {
        _logger = logger;
        _ircHubDbContext = serviceProvider.CreateScope().ServiceProvider.GetService<IrcHubDbContext>()!;
        _ircClientFactory = ircClientFactory;
    }

    public async Task Init(BotInstance botInstance)
    {
        _botInstance = botInstance;
        int sendConnectionCount = Limits.NormalBot.SendConnections;
        for (int i = 0; i < sendConnectionCount; i++)
        {
            IIrcClient ircClient = _ircClientFactory.Create();
            ircClient.Init(this);
            _ircSendClients.Add(ircClient);
        }

        UpdateChannels();
    }

    public void IntervalPing()
    {
        UpdateChannels();
    }

    public void UpdateChannels()
    {
        string[] channels = _ircHubDbContext.Connections
            .Include(channel => channel.Channel)
            .Where(connection => connection.BotUserId == _botInstance.BotInstanceData.UserId)
            .Select(connection => connection.Channel.ChannelName)
            .Distinct()
            .ToArray();

        SetChannel(channels);
    }

    private void SetChannel(params string[] channelNames)
    {
        Part(_ircReceiveClients.SelectMany(client => client.Channels).Except(channelNames).ToArray());
        Join(channelNames.Except(_ircReceiveClients.SelectMany(client => client.Channels)).ToArray());
    }

    private void Join(params string[] channelNames)
    {
        if (channelNames.Length == 0) return;
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

    private void Part(params string[] channelNames)
    {
        if (channelNames.Length == 0) return;

        foreach (string channelName in channelNames)
            GetIrcClientOfChannel(channelName)?.Channels.Remove(channelName);
    }

    public void SendMessage(string channel, string message)
    {
    }

    private IIrcClient? GetIrcClientOfChannel(string channel)
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

    public void RemoveReceiveClient(IIrcClient ircClient)
    {
        _ircReceiveClients.Remove(ircClient);
    }
}
