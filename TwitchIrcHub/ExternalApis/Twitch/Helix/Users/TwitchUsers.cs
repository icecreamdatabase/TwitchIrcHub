using TwitchIrcHub.ExternalApis.Twitch.Helix.Users.DataTypes;
using TwitchIrcHub.Model;

namespace TwitchIrcHub.ExternalApis.Twitch.Helix.Users;

public static class TwitchUsers
{
    public static async Task<List<TwitchUsersResult>?> Users(List<string>? ids = null, List<string>? logins = null)
    {
        // We can't set defaults for none primitives
        ids ??= new List<string>();
        logins ??= new List<string>();

        if (ids.Count == 0 && logins.Count == 0)
            return new List<TwitchUsersResult>();
        
        string appAccessToken = await BotDataAccess.GetAppAccessToken();
        HelixDataHolder<TwitchUsersResult>? users = await TwitchUsersStatics.Users(BotDataAccess.ClientId, appAccessToken, ids, logins);
        return users?.Data;
    }
}
