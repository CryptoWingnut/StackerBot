using System.Xml.Serialization;
using Coravel.Invocable;
using StackerBot.Services;

namespace StackerBot.Tasks;

public sealed class YouTubeVideoPoller(IRepository repository, ILogger<YouTubeVideoPoller> logger, EventBus eventBus) : IInvocable {
  public async Task Invoke() {
    try {
      await Handle();
    } catch (Exception error) {
      logger.LogError(error, "Exception occured while polling YouTube videos");
    }
  }

  private async Task Handle() {
    var client = new HttpClient();
    var channelsResults = await repository.GetYouTubeSubscriptions(CancellationToken.None);

    if (channelsResults.IsType(typeof(DatabaseError))) {
      logger.LogError("Failed to retrieve YouTube subscriptions");
      return;
    }

    foreach (var channel in channelsResults.GetT1) {
      try {
        await PollChannel(client, channel);
      } catch (Exception error) {
        logger.LogError(error, "An exception occurred with YouTube channel {ChannelId}", channel.ChannelId);
      }
    }
  }

  private async Task PollChannel(HttpClient client, YouTubeSubscriptionModel channel) {
    var request = await client.GetAsync($"https://www.youtube.com/feeds/videos.xml?channel_id={channel.ChannelId}");
    var response = await request.Content.ReadAsStringAsync();
    var serializer = new XmlSerializer(typeof(YouTubeFeed));
    using var reader = new StringReader(response);
    var deserialized = serializer.Deserialize(reader);

    if (deserialized is not YouTubeFeed feed) {
      logger.LogError("Failed to deserialize YouTube feed for channel {Channel}", channel.ChannelId);
      return;
    }

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
      await repository.UpdateLastVideoTime(channel.Id, published, CancellationToken.None);
    }
  }
}
