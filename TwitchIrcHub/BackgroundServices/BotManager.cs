using TwitchIrcHub.IrcBot.Bot;
using TwitchIrcHub.IrcBot.Helper;
using TwitchIrcHub.Model;

namespace TwitchIrcHub.BackgroundServices;

public class BotManager : TimedHostedService
{
    private readonly ILogger<BotManager> _logger;

    private static readonly List<IBotInstance> BotInstances = new();

    protected override TimeSpan Interval { get; } = TimeSpan.FromSeconds(30.0);
    protected override TimeSpan FirstRunAfter { get; } = TimeSpan.FromSeconds(0.1f);

    public BotManager(IServiceProvider services) : base(services)
    {
        _logger = services.GetService<ILogger<BotManager>>()!;
    }

    protected override async Task RunJobAsync(IServiceProvider serviceProvider, CancellationToken stoppingToken)
    {
        while (!PrepareBotAndPrefetchData.IsPrepared) 
            await Task.Delay(100, CancellationToken.None);

        IrcHubDbContext? db = serviceProvider.CreateScope().ServiceProvider.GetService<IrcHubDbContext>();
        if (db == null)
            return;

        _logger.LogInformation("Checking bots ...");

        List<int> activeBotIds = BotInstances.Select(bot => bot.BotInstanceData.UserId).ToList();
        List<int> requiredBotIds = db.Bots.Where(bot => bot.Enabled).Select(bot => bot.UserId).ToList();

        RemoveBots(activeBotIds.Except(requiredBotIds).ToArray());
        await CreateBots(serviceProvider, requiredBotIds.Except(activeBotIds).ToArray());

        Task[] intervalPingTasks = BotInstances.Select(bot => bot.IntervalPing()).ToArray();
        await Task.WhenAll(intervalPingTasks);
    }

    private static async Task CreateBots(IServiceProvider serviceProvider, params int[] botUserIds)
    {
        if (botUserIds.Length == 0)
            return;

        foreach (int botUserId in botUserIds)
        {
            IFactory<IBotInstance>? botInstanceFactory = serviceProvider.GetService<IFactory<IBotInstance>>();
            if (botInstanceFactory == null)
                return;

            IBotInstance botInstance = botInstanceFactory.Create();
            await botInstance.Init(botUserId);
            BotInstances.Add(botInstance);
        }
    }

    private static void RemoveBots(params int[] botUserIds)
    {
        if (botUserIds.Length == 0)
            return;

        foreach (IBotInstance botInstance in BotInstances.Where(bot => botUserIds.Contains(bot.BotInstanceData.UserId)))
        {
            BotInstances.Remove(botInstance);
            botInstance.Dispose();
        }
    }

    public static IBotInstance? GetBotInstance(int botUserId)
    {
       return BotInstances.FirstOrDefault(iBotInstance => iBotInstance.BotInstanceData.UserId == botUserId);
    }
}
