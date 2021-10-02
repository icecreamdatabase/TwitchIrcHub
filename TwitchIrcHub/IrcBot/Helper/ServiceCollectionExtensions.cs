namespace TwitchIrcHub.IrcBot.Helper;

/// <summary>
/// Taken from here: https://espressocoder.com/2018/10/08/injecting-a-factory-service-in-asp-net-core/
/// </summary>
public static class ServiceCollectionExtensions
{
    public static void AddFactory<TService, TImplementation>(this IServiceCollection services) 
        where TService : class
        where TImplementation : class, TService
    {
        services.AddTransient<TService, TImplementation>();
        services.AddSingleton<Func<TService>>(x => x.GetService<TService>);
        services.AddSingleton<IFactory<TService>, Factory<TService>>();
    }
}