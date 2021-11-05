using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using TwitchIrcHub.ExternalApis.Discord;
using TwitchIrcHub.Hubs.IrcHub;
using TwitchIrcHub.IrcBot.Bot;
using TwitchIrcHub.IrcBot.Helper;
using TwitchIrcHub.IrcBot.Irc.DataTypes;
using TwitchIrcHub.IrcBot.Irc.DataTypes.Parsed;
using TwitchIrcHub.IrcBot.Irc.IrcClient;
using TwitchIrcHub.Model;

namespace TwitchIrcHub.IrcBot.Irc.IrcPoolManager;

public class IrcPoolManager : IIrcPoolManager
{
    private readonly ILogger<IrcPoolManager> _logger;
    private readonly IFactory<IIrcClient> _ircClientFactory;
    private readonly IServiceProvider _serviceProvider;

    private IrcHubDbContext IrcHubDbContext =>
        _serviceProvider.CreateScope().ServiceProvider.GetService<IrcHubDbContext>() ??
        throw new InvalidOperationException();

    private IHubContext<IrcHub, IIrcHub> IrcHubContext =>
        _serviceProvider.GetService<IHubContext<IrcHub, IIrcHub>>() ??
        throw new InvalidOperationException();

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
        _serviceProvider = serviceProvider;
        _ircClientFactory = ircClientFactory;
    }

    public Task Init(BotInstance botInstance)
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
        return Task.CompletedTask;
    }

    public Task IntervalPing()
    {
        UpdateChannels();
        return Task.CompletedTask;
    }

    public void UpdateChannels()
    {
        string[] channels = IrcHubDbContext.Connections
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

    //@client-nonce=xxx;reply-parent-msg-id=xxx PRIVMSG #channel :xxxxxx `
    public void SendMessage(string channel, string message, string? clientNonce = null, string? replyParentMsgId = null)
    {
    }

    private IIrcClient? GetIrcClientOfChannel(string channel)
    {
        return _ircReceiveClients.FirstOrDefault(client => client.Channels.Contains(channel));
    }

    public Task ForceCheckAuth()
    {
        return Task.CompletedTask;
    }

    public async Task NewIrcMessage(IrcMessage ircMessage)
    {
        switch (ircMessage.IrcCommand)
        {
            case IrcCommands.ClearChat:
            {
                IrcClearChat ircClearChat = new IrcClearChat(ircMessage);
                List<int> appIds = GetAppIdsFromConnections(ircClearChat.RoomId);
                break;
            }
            case IrcCommands.ClearMsg:
            {
                IrcClearMsg ircClearMsg = new IrcClearMsg(ircMessage);
                List<int> appIds = GetAppIdsFromConnections(ircClearMsg.RoomId);
                break;
            }
            case IrcCommands.GlobalUserstate:
            {
                IrcGlobalUserState ircGlobalUserState = new IrcGlobalUserState(ircMessage);
                List<int> appIds = GetAppIdsFromConnections();
                break;
            }
            case IrcCommands.HostTarget:
            {
                IrcHostTarget ircHostTarget = new IrcHostTarget(ircMessage);
                List<int> appIds = GetAppIdsFromConnections(ircHostTarget.RoomName);
                break;
            }
            case IrcCommands.Notice:
            {
                IrcNotice ircNotice = new IrcNotice(ircMessage);
                List<int> appIds = GetAppIdsFromConnections(ircNotice.RoomName);
                break;
            }
            case IrcCommands.PrivMsg:
            {
                _logger.LogInformation("{Raw}", ircMessage.RawSource);
                IrcPrivMsg ircPrivMsg = new IrcPrivMsg(ircMessage);
                _logger.LogInformation("{Command}: {Channel}: {Msg}",
                    ircMessage.IrcCommand, ircPrivMsg.RoomName, ircPrivMsg.Message);

                List<int> appIds = GetAppIdsFromConnections(ircPrivMsg.RoomId);
                await IrcHub.NewIrcPrivMsg(IrcHubContext, ircPrivMsg, appIds);

                //DiscordLogger.Log(LogLevel.Information, JsonSerializer.Serialize(ircPrivMsg, new JsonSerializerOptions { WriteIndented = true, DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull}));
                break;
            }
            case IrcCommands.RoomState:
            {
                IrcRoomState ircRoomState = new IrcRoomState(ircMessage);
                List<int> appIds = GetAppIdsFromConnections(ircRoomState.RoomId);
                break;
            }
            case IrcCommands.UserNotice:
            {
                IrcUserNotice ircUserNotice = new IrcUserNotice(ircMessage);
                List<int> appIds = GetAppIdsFromConnections(ircUserNotice.RoomId);
                break;
            }
            case IrcCommands.UserState:
            {
                IrcUserState ircUserState = new IrcUserState(ircMessage);
                List<int> appIds = GetAppIdsFromConnections(ircUserState.RoomName);
                break;
            }
            default:
            {
                _logger.LogInformation("{Command}: {Raw}", ircMessage.IrcCommand, ircMessage.RawSource);
                break;
            }
        }
    }

    private List<int> GetAppIdsFromConnections()
    {
        return IrcHubDbContext.Connections
            .Where(connection => connection.BotUserId == _botInstance.BotInstanceData.UserId)
            .Select(connection => connection.RegisteredAppId)
            .Distinct()
            .ToList();
    }

    private List<int> GetAppIdsFromConnections(string roomName)
    {
        return new List<int>();
        int roomId = 1;
        return GetAppIdsFromConnections(roomId);
    }

    private List<int> GetAppIdsFromConnections(int roomId)
    {
        return IrcHubDbContext.Connections
            .Where(connection => connection.BotUserId == _botInstance.BotInstanceData.UserId &&
                                 connection.RoomId == roomId)
            .Select(connection => connection.RegisteredAppId)
            .Distinct()
            .ToList();
    }

    public void RemoveReceiveClient(IIrcClient ircClient)
    {
        _ircReceiveClients.Remove(ircClient);
    }
}
