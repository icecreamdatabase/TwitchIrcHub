using TwitchIrcHub.Model;
using TwitchIrcHub.ExternalApis.Twitch.Helix.Auth;
using TwitchIrcHub.ExternalApis.Twitch.Helix.Auth.DataTypes;

namespace TwitchIrcHub.IrcBot.Bot;

public class BotInstanceData : IBotInstanceData
{
    private readonly ILogger<BotInstanceData> _logger;
    private readonly IrcHubDbContext _ircHubDbContext;

    private const int PreemptiveTokenRefreshSeconds = 600;
    private const int CacheValidTime = 300;
    private DateTime _lastValidation = DateTime.MinValue;
    private bool UpdateCachedAccessToken => (DateTime.Now - _lastValidation).TotalSeconds > CacheValidTime;
    private bool _currentlyValidatingOrRefreshing;

    public int UserId { get; private set; }
    public string UserName { get; private set; } = null!;
    public string AccessToken { get; private set; } = null!;
    public string RefreshToken { get; private set; } = null!;
    public int? SupinicApiUser { get; private set; }
    public string? SupinicApiKey { get; private set; }

    public BotInstanceData(ILogger<BotInstanceData> logger, IServiceProvider serviceProvider)
    {
        _logger = logger;
        _ircHubDbContext = serviceProvider.CreateScope().ServiceProvider.GetService<IrcHubDbContext>()!;
    }

    public async Task Init(int botUserId)
    {
        UserId = botUserId;
        Model.Schema.Bot? bot = _ircHubDbContext.Bots.Find(botUserId);
        if (bot == null)
            throw new Exception($"No DB data for {botUserId}");

        UserName = bot.UserName;
        AccessToken = bot.AccessToken;
        RefreshToken = bot.RefreshToken;
        SupinicApiUser = bot.SupinicApiUser;
        SupinicApiKey = bot.SupinicApiKey;

        await ValidateAccessToken();
        _lastValidation = DateTime.Now;
    }

    public void IntervalPing()
    {
        if (UpdateCachedAccessToken && !_currentlyValidatingOrRefreshing)
            ValidateAccessToken().ContinueWith(_ => _lastValidation = DateTime.Now);
    }

    private const int MaxRefreshRetryCountBeforeException = 10;
    private int _refreshRetryCount;

    private async Task ValidateAccessToken()
    {
        _currentlyValidatingOrRefreshing = true;
        try
        {
            TwitchValidateResult? validateResult = await TwitchAuthentication.Validate(AccessToken);
            if (validateResult == null)
                throw new Exception("ValidateResult is null. Validate has failed!");

            if (validateResult.ExpiresIn < PreemptiveTokenRefreshSeconds)
                await RefreshAccessToken();
        }
        catch
        {
            _refreshRetryCount++;
            if (_refreshRetryCount >= MaxRefreshRetryCountBeforeException)
            {
                //TODO: shut down this bot and alert in discord
                throw;
            }
        }
        finally
        {
            _currentlyValidatingOrRefreshing = false;
        }
    }

    private async Task RefreshAccessToken()
    {
        TwitchTokenResult? tokenResult = await TwitchAuthentication.Refresh(RefreshToken);
        if (tokenResult == null)
            throw new Exception("TokenResult is null. Refresh has failed!");

        Model.Schema.Bot? bot = await _ircHubDbContext.Bots.FindAsync(UserId);
        if (bot == null)
            throw new Exception($"No DB data for {UserId}");

        AccessToken = tokenResult.AccessToken;
        bot.AccessToken = AccessToken;
        RefreshToken = tokenResult.RefreshToken;
        bot.RefreshToken = RefreshToken;
        await _ircHubDbContext.SaveChangesAsync();

        _lastValidation = DateTime.Now;
        _logger.LogInformation("Refreshed AccessToken for {UserName} ({UserId})", UserName, UserId);
    }
}
