using System.Text;
using TwitchIrcHub.ExternalApis.Discord.WebhookObjects;

namespace TwitchIrcHub.ExternalApis.Discord;

public static class DiscordWebhook
{
    private static readonly HttpClient Client = new();
    private static readonly Dictionary<LogChannel, string> WebhookLinks = new();

    public static void SetWebhooks(IConfigurationSection configurationSection)
    {
        Dictionary<string, string> webhookLinks = configurationSection.GetChildren()
            .ToDictionary(section => section.Key, section => section.Value);

        foreach ((string key, string value) in webhookLinks)
        {
            if (Enum.IsDefined(typeof(LogChannel), key))
                WebhookLinks.Add(Enum.Parse<LogChannel>(key), value);
        }
    }

    internal static async void SendFilesWebhook(LogChannel logChannel, string username,
        Dictionary<string, string> files,
        string payloadJson = "")
    {
        if (!WebhookLinks.ContainsKey(logChannel))
            return;
        string idToken = WebhookLinks[logChannel];

        using MultipartFormDataContent content = new()
        {
            {new StringContent(username), "username"},
            {new StringContent(payloadJson, Encoding.UTF8, "application/json"), "payload_json"},
        };
        foreach ((string key, string value) in files)
        {
            content.Add(new StringContent(value), "file", $"{key}.txt");
        }

        HttpResponseMessage response =
            await Client.PostAsync($@"https://discord.com/api/webhooks/{idToken}", content);
        string res = await response.Content.ReadAsStringAsync();
    }

    internal static async void SendEmbedsWebhook(LogChannel logChannel, WebhookPostContent content)
    {
        if (!WebhookLinks.ContainsKey(logChannel))
            return;
        string idToken = WebhookLinks[logChannel];

        using HttpRequestMessage requestMessage = new()
        {
            Method = HttpMethod.Post,
            RequestUri = new Uri(@$"https://discord.com/api/webhooks/{idToken}"),
            Content = JsonContent.Create(content, mediaType: null)
        };
        HttpResponseMessage response = await Client.SendAsync(requestMessage);
        string res = await response.Content.ReadAsStringAsync();
    }
}