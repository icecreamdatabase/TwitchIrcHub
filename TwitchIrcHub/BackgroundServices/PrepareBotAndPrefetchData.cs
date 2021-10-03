using TwitchIrcHub.Model;

namespace TwitchIrcHub.BackgroundServices;

/// <summary>
/// https://stackoverflow.com/questions/50763577/where-to-put-code-to-run-after-startup-is-completed/50771330
/// </summary>
public class PrepareBotAndPrefetchData : IHostedService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IHostEnvironment _hostEnvironment;

    public PrepareBotAndPrefetchData(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
        _hostEnvironment = serviceProvider.GetRequiredService<IHostEnvironment>();
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        /* SCOPE */
        using IServiceScope serviceScope = _serviceProvider.CreateScope();
        IServiceProvider scopeServiceProvider = serviceScope.ServiceProvider;

        /* DATABASE */
        await using IrcHubDbContext db = scopeServiceProvider.GetRequiredService<IrcHubDbContext>();

        await db.Database.EnsureCreatedAsync(cancellationToken);
    }

    public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
}