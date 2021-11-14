using TwitchIrcHub.ExternalApis.Twitch.Helix.Users.DataTypes;
using TwitchIrcHub.Model;

namespace TwitchIrcHub.ExternalApis.Twitch.Helix.Users;

public static class TwitchUsers
{
    private const int MaxTwitchUsersChunkSize = 50;
    private static readonly Dictionary<string, string> IdToLoginCache = new();
    private static readonly Dictionary<string, string> LoginToIdCache = new();

    public static async Task<List<TwitchUsersResult>?> Users(List<string>? ids = null, List<string>? logins = null)
    {
        // We can't set defaults for none primitives
        ids ??= new List<string>();
        logins ??= new List<string>();

        if (ids.Count == 0 && logins.Count == 0)
            return new List<TwitchUsersResult>();

        if (ids.Count + logins.Count <= MaxTwitchUsersChunkSize)
        {
            return await UsersRaw(ids, logins);
        }

        List<TwitchUsersResult> returnUsers = new();

        foreach (string[] chunkedIds in ids.Chunk(MaxTwitchUsersChunkSize))
        {
            returnUsers.AddRange(await UsersRaw(chunkedIds.ToList(), new List<string>()));
        }
        foreach (string[] chunkedLogins in ids.Chunk(MaxTwitchUsersChunkSize))
        {
            returnUsers.AddRange(await UsersRaw(new List<string>(), chunkedLogins.ToList()));
        }

        return returnUsers;
    }

    private static async Task<List<TwitchUsersResult>> UsersRaw(List<string> ids, List<string> logins)
    {
        if (ids.Count + logins.Count > MaxTwitchUsersChunkSize)
            throw new ArgumentException(
                $"Combined count of {nameof(ids)} and {nameof(logins)} can't be higher than 50.");

        string appAccessToken = await BotDataAccess.GetAppAccessToken();
        HelixDataHolder<TwitchUsersResult>? users =
            await TwitchUsersStatics.Users(BotDataAccess.ClientId, appAccessToken, ids, logins);
        return users?.Data ?? new List<TwitchUsersResult>();
    }

    public static async Task<Dictionary<string, string>> IdsToLoginsWithCache(List<string> ids)
    {
        List<string> idsNotInCache = ids.Except(IdToLoginCache.Keys).ToList();

        // Fetch ids not in cache and add them to the dictionary
        List<TwitchUsersResult>? users = await Users(ids: idsNotInCache);
        users?.ForEach(user => IdToLoginCache[user.Id] = user.Login);

        return new Dictionary<string, string>(IdToLoginCache.Where(pair => ids.Contains(pair.Key)));
    }

    public static async Task<Dictionary<string, string>> LoginsToIdsWithCache(List<string> logins)
    {
        List<string> loginsNotInCache = logins.Except(LoginToIdCache.Keys).ToList();

        // Fetch logins not in cache and add them to the dictionary
        List<TwitchUsersResult>? users = await Users(logins: loginsNotInCache);
        users?.ForEach(user => LoginToIdCache[user.Login] = user.Id);

        return new Dictionary<string, string>(LoginToIdCache.Where(pair => logins.Contains(pair.Key)));
    }
}
