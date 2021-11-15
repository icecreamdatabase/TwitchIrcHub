﻿using System.Collections.Concurrent;
using System.Net;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using TwitchIrcHub.ExternalApis.Twitch.Helix.Users;
using TwitchIrcHub.Hubs.IrcHub;
using TwitchIrcHub.IrcBot.Bot;
using TwitchIrcHub.IrcBot.Helper;
using TwitchIrcHub.IrcBot.Irc.DataCache;
using TwitchIrcHub.IrcBot.Irc.DataTypes;
using TwitchIrcHub.IrcBot.Irc.DataTypes.FromTwitch;
using TwitchIrcHub.IrcBot.Irc.DataTypes.ToTwitch;
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

    private int _ircLastUsedSendClientIndex;
    private readonly List<IIrcClient> _ircSendClients = new();
    private readonly List<IIrcClient> _ircReceiveClients = new();
    private const int MaxChannelsPerIrcClient = 200;

    private readonly ConcurrentDictionary<string, string> _previousMessageInChannel = new();
    private readonly ConcurrentDictionary<string, DateTime> _previousMessageDateTimeInChannel = new();

    public string BotUsername => _botInstance.BotInstanceData.UserName;
    public string BotOauth => _botInstance.BotInstanceData.AccessToken;

    private BotInstance _botInstance = null!;
    private IrcSendQueue _ircSendQueue = null!;

    public IrcBuckets IrcBuckets { get; private set; } = null!;
    public UserStateCache UserStateCache { get; } = new();
    public GlobalUserStateCache GlobalUserStateCache { get; } = new();

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
        IrcBuckets = new IrcBuckets(_botInstance.BotInstanceData.Limits);
        _ircSendQueue = new IrcSendQueue(this);
        for (int i = 0; i < _botInstance.BotInstanceData.Limits.SendConnections; i++)
        {
            IIrcClient ircClient = _ircClientFactory.Create();
            ircClient.Init(this, true);
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

    public void SendMessage(PrivMsgToTwitch privMsg)
    {
        _ircSendQueue.Enqueue(privMsg);
        //foreach (PrivMsgToTwitch msg in morePrivMsgsInBatch)
        //{
        //    msg.UseSameSendConnectionAsPreviousMsg = true;
        //    _ircSendQueue.Enqueue(msg);
        //}
    }

    //@client-nonce=xxx;reply-parent-msg-id=xxx PRIVMSG #channel :xxxxxx `
    public async Task SendMessageNoQueue(PrivMsgToTwitch privMsgToTwitch)
    {
        bool useModRateLimit = UserStateCache.IsModInChannel(privMsgToTwitch.RoomName);

        /* ---------- Ratelimit ---------- */
        await IrcBuckets.WaitForMessageTicket(useModRateLimit);
        await HandleGlobalCooldown(privMsgToTwitch, useModRateLimit);
        
        /* ---------- Message adjustment ---------- */ 
        HandleMessageCleanup(privMsgToTwitch);
        HandleDuplicateMessage(privMsgToTwitch);

        /* ---------- Advance send client index if needed ---------- */
        if (!privMsgToTwitch.UseSameSendConnectionAsPreviousMsg)
            _ircLastUsedSendClientIndex = (_ircLastUsedSendClientIndex + 1) % _ircSendClients.Count;

        _logger.LogInformation("{Line}", privMsgToTwitch.ToString());

        /* ---------- Send message ---------- */
        await _ircSendClients[_ircLastUsedSendClientIndex].SendLine(privMsgToTwitch.ToString());
    }

    private async Task HandleGlobalCooldown(PrivMsgToTwitch privMsgToTwitch, bool useModRateLimit)
    {
        if (useModRateLimit) return;

        if (_previousMessageDateTimeInChannel.ContainsKey(privMsgToTwitch.RoomName))
        {
            double msSinceLastMessageInSameChannel =
                (DateTime.UtcNow - _previousMessageDateTimeInChannel[privMsgToTwitch.RoomName]).TotalMilliseconds;

            double additionalWaitRequired = 1100 - msSinceLastMessageInSameChannel;

            if (additionalWaitRequired > 0)
            {
                _logger.LogDebug("Sending messages too fast. Waiting for {Ms} ms", (int)additionalWaitRequired);
                await Task.Delay((int)additionalWaitRequired, CancellationToken.None);
            }
        }

        _previousMessageDateTimeInChannel[privMsgToTwitch.RoomName] = DateTime.UtcNow;
    }

    private static void HandleMessageCleanup(PrivMsgToTwitch privMsgToTwitch)
    {
        privMsgToTwitch.Message = privMsgToTwitch.Message.Trim();
    }

    private void HandleDuplicateMessage(PrivMsgToTwitch privMsgToTwitch)
    {
        // Is the message identical
        if (_previousMessageInChannel.ContainsKey(privMsgToTwitch.RoomName) &&
            privMsgToTwitch.Message == _previousMessageInChannel[privMsgToTwitch.RoomName]
           )
        {
            // If the message is an ACTION we want to ignore the first space after the .me /me
            // for the startIndex we technically need to check for index out of bound.
            // But we always trim beforehand and therefore the space can never be the last character in the string.
            int spaceIndex = privMsgToTwitch.Message.StartsWith(".me ") || privMsgToTwitch.Message.StartsWith("/me ")
                ? privMsgToTwitch.Message.IndexOf(' ', privMsgToTwitch.Message.IndexOf(' ') + 1)
                : privMsgToTwitch.Message.IndexOf(' ');
            if (spaceIndex == -1)
                // No space found, fall back to the old magic character.
                privMsgToTwitch.Message += "\U000E0000";
            else
                // Insert a second space at the position of the first space.
                privMsgToTwitch.Message = privMsgToTwitch.Message.Insert(spaceIndex, " ");
        }

        _previousMessageInChannel[privMsgToTwitch.RoomName] = privMsgToTwitch.Message;
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
                await IrcHubToClients.NewIrcClearChat(IrcHubContext, ircClearChat, appIds);
                break;
            }
            case IrcCommands.ClearMsg:
            {
                IrcClearMsg ircClearMsg = new IrcClearMsg(ircMessage);
                List<int> appIds = GetAppIdsFromConnections(ircClearMsg.RoomId);
                await IrcHubToClients.NewIrcClearMsg(IrcHubContext, ircClearMsg, appIds);
                break;
            }
            case IrcCommands.GlobalUserState:
            {
                IrcGlobalUserState ircGlobalUserState = new IrcGlobalUserState(ircMessage);
                GlobalUserStateCache.LastGlobalUserState = ircGlobalUserState;
                List<int> appIds = GetAppIdsFromConnections();
                await IrcHubToClients.NewIrcGlobalUserState(IrcHubContext, ircGlobalUserState, appIds);
                break;
            }
            case IrcCommands.HostTarget:
            {
                IrcHostTarget ircHostTarget = new IrcHostTarget(ircMessage);
                List<int> appIds = await GetAppIdsFromConnections(ircHostTarget.RoomName);
                await IrcHubToClients.NewIrcHostTarget(IrcHubContext, ircHostTarget, appIds);
                break;
            }
            case IrcCommands.Notice:
            {
                IrcNotice ircNotice = new IrcNotice(ircMessage);
                List<int> appIds = await GetAppIdsFromConnections(ircNotice.RoomName);
                await IrcHubToClients.NewIrcNotice(IrcHubContext, ircNotice, appIds);
                break;
            }
            case IrcCommands.PrivMsg:
            {
                _logger.LogInformation("{Raw}", ircMessage.RawSource);
                IrcPrivMsg ircPrivMsg = new IrcPrivMsg(ircMessage);
                //_logger.LogInformation("{Command}: {Channel}: {Msg}", ircMessage.IrcCommand, ircPrivMsg.RoomName, ircPrivMsg.Message);

                List<int> appIds = GetAppIdsFromConnections(ircPrivMsg.RoomId);
                await IrcHubToClients.NewIrcPrivMsg(IrcHubContext, ircPrivMsg, appIds);

                //DiscordLogger.Log(LogLevel.Information, JsonSerializer.Serialize(ircPrivMsg, new JsonSerializerOptions { WriteIndented = true, DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull}));
                break;
            }
            case IrcCommands.RoomState:
            {
                IrcRoomState ircRoomState = new IrcRoomState(ircMessage);
                List<int> appIds = GetAppIdsFromConnections(ircRoomState.RoomId);
                await IrcHubToClients.NewIrcRoomState(IrcHubContext, ircRoomState, appIds);
                break;
            }
            case IrcCommands.UserNotice:
            {
                IrcUserNotice ircUserNotice = new IrcUserNotice(ircMessage);
                List<int> appIds = GetAppIdsFromConnections(ircUserNotice.RoomId);
                await IrcHubToClients.NewIrcUserNotice(IrcHubContext, ircUserNotice, appIds);
                break;
            }
            case IrcCommands.UserState:
            {
                IrcUserState ircUserState = new IrcUserState(ircMessage);
                UserStateCache.AddUserState(ircUserState);
                List<int> appIds = await GetAppIdsFromConnections(ircUserState.RoomName);
                await IrcHubToClients.NewIrcUserState(IrcHubContext, ircUserState, appIds);
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

    private async Task<List<int>> GetAppIdsFromConnections(string roomName)
    {
        Dictionary<string, string> dict = await TwitchUsers.LoginsToIdsWithCache(new List<string> { roomName });
        return dict.ContainsKey(roomName)
            ? GetAppIdsFromConnections(int.Parse(dict[roomName]))
            : new List<int>();
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
