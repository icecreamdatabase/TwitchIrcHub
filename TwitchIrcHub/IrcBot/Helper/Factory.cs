namespace TwitchIrcHub.IrcBot.Helper;
//public abstract class Factory<T> : IFactory<T>
//{
//    private readonly IServiceProvider _serviceProvider;
//    protected Factory(IServiceProvider serviceProvider) => _serviceProvider = serviceProvider;
//    public T Create() => _serviceProvider.GetRequiredService<T>();
//    
//}

public class Factory<T> : IFactory<T>
{
    private readonly Func<T> _initFunc;

    public Factory(Func<T> initFunc)
    {
        _initFunc = initFunc;
    }

    public T Create()
    {
        return _initFunc();
    }
}