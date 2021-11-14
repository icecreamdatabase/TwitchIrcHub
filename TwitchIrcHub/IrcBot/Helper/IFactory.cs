namespace TwitchIrcHub.IrcBot.Helper;

public interface IFactory<out T>
{
    public T Create();
}
