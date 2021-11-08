using System.Net.Http.Headers;
using System.Text.Json;
using Microsoft.AspNetCore.Http.Extensions;
using TwitchIrcHub.ExternalApis.Twitch.Helix.Users.DataTypes;

namespace TwitchIrcHub.ExternalApis.Twitch.Helix.Users;

public static class TwitchUsersStatics
{
    private static readonly HttpClient Client = new();
    private const string BaseUrlUsers = @"https://api.twitch.tv/helix/users";

    public static async Task<HelixDataHolder<TwitchUsersResult>?> Users(string clientId, string appAccessToken,
        IEnumerable<string> ids, IEnumerable<string> logins)
    {
        string queryString = BaseUrlUsers + new QueryBuilder { { "id", ids }, { "login", logins } };

        using HttpRequestMessage requestMessage = new();
        requestMessage.Method = HttpMethod.Get;
        requestMessage.RequestUri = new Uri(queryString, UriKind.Absolute);
        requestMessage.Headers.Add("client-id", clientId);
        requestMessage.Headers.Authorization = new AuthenticationHeaderValue("Bearer", appAccessToken);
        HttpResponseMessage response = await Client.SendAsync(requestMessage);
        string responseFromServer = await response.Content.ReadAsStringAsync();

        return JsonSerializer.Deserialize<HelixDataHolder<TwitchUsersResult>>(responseFromServer);
    }
}
