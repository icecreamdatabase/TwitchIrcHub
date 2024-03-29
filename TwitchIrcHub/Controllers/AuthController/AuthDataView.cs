﻿using System.Diagnostics.CodeAnalysis;
using TwitchIrcHub.IrcBot.Bot;

namespace TwitchIrcHub.Controllers.AuthController;

[SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
[SuppressMessage("ReSharper", "UnusedAutoPropertyAccessor.Global")]
public class AuthDataView
{
    public int UserId { get; }
    public string UserName { get; }
    public string AccessToken { get; }
    public int? SupinicApiUser { get; }
    public string? SupinicApiKey { get; }

    public AuthDataView(IBotInstanceData bot)
    {
        UserId = bot.UserId;
        UserName = bot.UserName;
        AccessToken = bot.AccessToken;
        SupinicApiUser = bot.SupinicApiUser;
        SupinicApiKey = bot.SupinicApiKey;
    }
}
