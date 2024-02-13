using System.Xml.Serialization;
using Coravel.Invocable;
using StackerBot.Services;

namespace StackerBot.Tasks;

public sealed class YouTubeVideoPoller(IRepository repository, ILogger<YouTubeVideoPoller> logger, EventBus eventBus) : IInvocable {
  public async Task Invoke() {
    var client = new HttpClient();
    var channelsResults = await repository.GetYouTubeSubscriptions(CancellationToken.None);

    if (channelsResults.IsType(typeof(DatabaseError))) {
      logger.LogError("Failed to retrieve YouTube subscriptions");
      return;
    }

    foreach (var channel in channelsResults.GetT1) {
      var request = await client.GetAsync($"https://www.youtube.com/feeds/videos.xml?channel_id={channel.ChannelId}");
      var response = await request.Content.ReadAsStringAsync();
      var serializer = new XmlSerializer(typeof(YouTubeFeed));
      using var reader = new StringReader(response);
      var deserialized = serializer.Deserialize(reader);

      if (deserialized is not YouTubeFeed) {
        logger.LogError("Failed to deserialize YouTube feed for channel {Channel}", channel.ChannelId);
        return;
      }

      var feed = (YouTubeFeed) deserialized;

      foreach (var entry in feed.YouTubeFeedEntries) {
        if (string.IsNullOrEmpty(entry.Published)) {
          logger.LogError("Failed to parse published date for video {VideoId}", entry.VideoId);
          continue;
        }

        if (string.IsNullOrEmpty(entry.VideoId)) {
          logger.LogError("Failed to parse video ID for video {Channel}", channel.ChannelId);
          continue;
        }

        var published = DateTime.Parse(entry.Published).ToUniversalTime();

        if (published <= channel.LastVideo) {
          continue;
        }

        await eventBus.SendYouTubeChannelPost(channel.ChannelName.TrimStart('@'), $"https://www.youtube.com/watch?v={entry.VideoId}");
        channel.LastVideo = published;
        await repository.UpdateLastVideoTime(channel.Id, published, CancellationToken.None);
      }
    }
  }
}
