using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;
using TwitchIrcHub.ExternalApis.Discord;
using TwitchIrcHub.ExternalApis.Twitch.Helix.Auth;
using TwitchIrcHub.ExternalApis.Twitch.Helix.Auth.DataTypes;
using TwitchIrcHub.Helper;
using TwitchIrcHub.Model.Schema;

namespace TwitchIrcHub.Model;

public static class BotDataAccess
{
    public static IServiceProvider? ServiceProvider { get; set; }

    private static string? _clientId;

    public static string ClientId
    {
        get
        {
            if (string.IsNullOrEmpty(_clientId))
                _clientId = Get("clientId");
            return _clientId;
        }
    }

    private static string? _clientSecret;

    public static string ClientSecret
    {
        get
        {
            if (string.IsNullOrEmpty(_clientSecret))
                _clientSecret = Get("clientSecret");
            return _clientSecret;
        }
    }

    private static string? _hmacsha256Key;

    public static string Hmacsha256Key
    {
        get
        {
            if (string.IsNullOrEmpty(_hmacsha256Key))
                _hmacsha256Key = Get("hmacsha256Key");
            return _hmacsha256Key;
        }
    }

    public static string AppAccessToken
    {
        get => Get("appAccessToken");
        set => Set("appAccessToken", value);
    }

    private const int MinutesTokenIsAssumbedToBeValidAfterValidation = 60; // 1 hour in minutes
    private const int MinSecondsForRefreshingAppAccesstoken = 2 * 24 * 60 * 60; // 2 days in seconds
    private static DateTime _lastAppAccessValidateUtc = DateTime.MinValue;

    public static async Task<string> GetAppAccessToken(bool forceValidate = false)
    {
        // Have we checked recently
        if (!forceValidate &&
            (DateTime.UtcNow - _lastAppAccessValidateUtc).TotalMinutes < MinutesTokenIsAssumbedToBeValidAfterValidation
           )
            return AppAccessToken;

        string currentAppAccessToken = AppAccessToken;
        // Is the token still valid enough?
        TwitchValidateResult? validateResult = await TwitchAuthentication.Validate(currentAppAccessToken);
        if (validateResult is { ExpiresIn: > MinSecondsForRefreshingAppAccesstoken })
        {
            _lastAppAccessValidateUtc = DateTime.UtcNow;
            return currentAppAccessToken;
        }

        // Token is about to run out --> get a new one
        TwitchTokenResult? appAccessTokenResult = await TwitchAuthentication.GetAppAccessToken(ClientId, ClientSecret);
        if (appAccessTokenResult != null && !string.IsNullOrEmpty(appAccessTokenResult.AccessToken))
        {
            AppAccessToken = appAccessTokenResult.AccessToken;
            _lastAppAccessValidateUtc = DateTime.UtcNow;
            return appAccessTokenResult.AccessToken;
        }

        // Getting access token failed
        DiscordLogger.Log(
            LogLevel.Error,
            "AppAccessToken failed",
            JsonSerializer.Serialize(appAccessTokenResult, GlobalStatics.JsonIndentAndIgnoreNullValues)
        );
        throw new Exception("Validating AppAccessToken failed");
    }

    private static string Get(string key)
    {
        IrcHubDbContext db = GetFreshIrcHubDbContext();
        string? value = db.BotData.Where(data => data.Key == key).ToList().Select(data => data.Value).FirstOrDefault();
        if (string.IsNullOrEmpty(value))
            throw new Exception($"{nameof(BotDataAccess)}: value for {key} is missing!");
        return value;
    }

    private static void Set(string key, string value)
    {
        IrcHubDbContext db = GetFreshIrcHubDbContext();
        BotData? entry = db.BotData.Where(data => data.Key == key).ToList().FirstOrDefault();
        if (entry != null)
            entry.Value = value;
        else
            db.BotData.Add(new BotData { Key = key, Value = value });
        db.SaveChanges();
    }

    private static IrcHubDbContext GetFreshIrcHubDbContext()
    {
        if (ServiceProvider == null)
            throw new Exception("ServiceProvider is null");
        IrcHubDbContext? ircHubDbContext =
            ServiceProvider.CreateScope().ServiceProvider.GetService<IrcHubDbContext>();
        if (ircHubDbContext == null)
            throw new Exception("ircHubDbContext is null");
        return ircHubDbContext;
    }
}
