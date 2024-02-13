using Newtonsoft.Json;

namespace StackerBot.Services;

public sealed class Externals : IExternals {
  private static readonly HttpClient httpClient = new();

  public async ValueTask<bool> SubscribeToYouTubeChannel(string channelId) {
    var feed = $"https://www.youtube.com/feeds/videos.xml?channel_id={channelId}";
    var content = new FormUrlEncodedContent(new[] {
      new KeyValuePair<string, string>("hub.mode", "subscribe"),
      new KeyValuePair<string, string>("hub.topic", feed),
      new KeyValuePair<string, string>("hub.callback", Environment.GetEnvironmentVariable("YOUTUBE_CALLBACK_URL") ?? "")
    });

    var response = await httpClient.PostAsync("https://pubsubhubbub.appspot.com/", content);

    return response.IsSuccessStatusCode;
  }

  public async ValueTask<bool> UnSubscribeFromYouTubeChannel(string channelId) {
    var feed = $"https://www.youtube.com/feeds/videos.xml?channel_id={channelId}";
    var content = new FormUrlEncodedContent(new[] {
      new KeyValuePair<string, string>("hub.mode", "unsubscribe"),
      new KeyValuePair<string, string>("hub.topic", feed),
      new KeyValuePair<string, string>("hub.callback", Environment.GetEnvironmentVariable("YOUTUBE_CALLBACK_URL") ?? "")
    });

    var response = await httpClient.PostAsync("https://pubsubhubbub.appspot.com/", content);

    return response.IsSuccessStatusCode;
  }

  public async ValueTask<string?> GetYouTubeChannelId(string channelName) {
    var apiKey = Environment.GetEnvironmentVariable("GOOGLE_API_KEY");
    var requestUrl = $"https://www.googleapis.com/youtube/v3/search?part=id&type=channel&q={channelName}&key={apiKey}";

    var response = await httpClient.GetAsync(requestUrl);

    if (!response.IsSuccessStatusCode) {
      return null;
    }

    var content = await response.Content.ReadAsStringAsync();
    var data = JsonConvert.DeserializeObject<dynamic>(content);
    return data?.items[0].id.channelId.ToString();
  }
}
