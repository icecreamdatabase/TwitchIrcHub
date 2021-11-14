namespace TwitchIrcHub.IrcBot.Helper;

public class Limits
{
    public static readonly Limits NormalBot = new()
    {
        SendConnections = 2,
        MaxChannelsPerIrcClient = 50,
        IrcAuthBucketLimit = 20,
        IrcJoinBucketLimit = 20,
        IrcChannelMessageBucketLimitUser = 20,
        IrcChannelMessageBucketLimitMod = 100,
        IrcGlobalMessageBucketLimitUser = 20,
        IrcGlobalMessageBucketLimitMod = 100,
    };


    public static readonly Limits KnownBot = new()
    {
        SendConnections = 2,
        MaxChannelsPerIrcClient = 50,
        IrcAuthBucketLimit = 20,
        IrcJoinBucketLimit = 20,
        IrcChannelMessageBucketLimitUser = 20,
        IrcChannelMessageBucketLimitMod = 100,
        IrcGlobalMessageBucketLimitUser = 50,
        IrcGlobalMessageBucketLimitMod = 100,
    };

    public static readonly Limits VerifiedBot = new()
    {
        SendConnections = 5,
        MaxChannelsPerIrcClient = 500,
        IrcAuthBucketLimit = 200,
        IrcJoinBucketLimit = 2000,
        IrcChannelMessageBucketLimitUser = 20,
        IrcChannelMessageBucketLimitMod = 100,
        IrcGlobalMessageBucketLimitUser = 7500,
        IrcGlobalMessageBucketLimitMod = 7500,
    };

    public int SendConnections { get; private init; }
    public int MaxChannelsPerIrcClient { get; private init; }
        
    /* IRC connect bucket */
    public int IrcAuthBucketLimit { get; private init; }
    public const int IrcAuthBucketPerXSeconds = 10;
    public int IrcJoinBucketLimit { get; private init; }
    public const int IrcJoinBucketPerXSeconds = 10;
        
    /* IRC message bucket */
    public int IrcChannelMessageBucketLimitUser { get; private init; }
    public int IrcChannelMessageBucketLimitMod { get; private init; }
    public int IrcGlobalMessageBucketLimitUser { get; private init; }
    public int IrcGlobalMessageBucketLimitMod { get; private init; }
    public const int IrcMessageBucketPerXSeconds = 30;
}