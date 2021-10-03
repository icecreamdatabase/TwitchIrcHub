using Microsoft.EntityFrameworkCore;
using TwitchIrcHub.Model.Schema;

namespace TwitchIrcHub.Model
{
    public static class BotDataAccess
    {
        public static IServiceProvider? ServiceProvider { get; set; }

        private static string? _clientId;
        public static string ClientId
        {
            get
            {
                if (string.IsNullOrEmpty(_clientId))
                    _clientId = Get(GetFreshBotData(), "clientId");
                return _clientId;
            }
        }
        
        private static string? _clientSecret;
        public static string ClientSecret
        {
            get
            {
                if (string.IsNullOrEmpty(_clientSecret))
                    _clientSecret = Get(GetFreshBotData(), "clientSecret");
                return _clientSecret;
            }
        }
        
        private static string? _hmacsha256Key;
        public static string Hmacsha256Key
        {
            get
            {
                if (string.IsNullOrEmpty(_hmacsha256Key))
                    _hmacsha256Key = Get(GetFreshBotData(), "hmacsha256Key");
                return _hmacsha256Key;
            }
        }

        public static string AppAccessToken
        {
            get => Get(GetFreshBotData(), "appAccessToken");
            set => Set(GetFreshBotData(), "appAccessToken", value);
        }

        private static string Get(IQueryable<BotData> botData, string key)
        {
            string? value = botData.Where(data => data.Key == key).ToList().Select(data => data.Value).FirstOrDefault();
            if (string.IsNullOrEmpty(value))
                throw new Exception($"{nameof(BotDataAccess)}: value for {key} is missing!");
            return value;
        }

        private static void Set(DbSet<BotData> botData, string key, string value)
        {
            BotData? entry = botData.Where(data => data.Key == key).ToList().FirstOrDefault();
            if (entry != null)
                entry.Value = value;
            else
                botData.Add(new BotData { Key = key, Value = value });
        }

        private static DbSet<BotData> GetFreshBotData()
        {
            if (ServiceProvider == null)
                throw new Exception("ServiceProvider is null");
            IrcHubDbContext? ircHubDbContext =
                ServiceProvider.CreateScope().ServiceProvider.GetService<IrcHubDbContext>();
            if (ircHubDbContext == null)
                throw new Exception("ircHubDbContext is null");
            DbSet<BotData> botData = ircHubDbContext.BotData;
            return botData;
        }
    }
}
