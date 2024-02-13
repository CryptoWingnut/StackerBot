using Microsoft.AspNetCore.Mvc;
using System.Xml.Serialization;
using StackerBot.Services;

namespace StackerBot;

[ApiController, Route("webhooks")]
public sealed class WebhooksController(EventBus eventBus, IRepository repository, ILogger<WebhooksController> logger) : ControllerBase {
  [HttpGet("yt_sub_notify")]
  public IActionResult VerifySubscription([FromQuery(Name = "hub.challenge")] string challenge) {
    return StatusCode(200, challenge);
  }

  [HttpPost("yt_sub_notify")]
  public async ValueTask<IActionResult> YouTubeSubscriptionNotify([FromBody] string data) {
    try {
      var serializer = new XmlSerializer(typeof(YouTubeFeed));
      using var reader = new StringReader(data);
      var deserialized = serializer.Deserialize(reader);

      if (deserialized is null) {
        logger.LogError("Failed to deserialize YouTube subscription notification data: {Data}", data);
        return StatusCode(200);
      }

      var feed = (YouTubeFeed) deserialized;

      if (feed.YouTubeFeedEntry?.VideoId is null || feed.YouTubeFeedEntry.ChannelId is null) {
        logger.LogError("Failed to deserialize YouTube subscription notification data: {Data}", data);
        return StatusCode(200);
      }

      var videoId = feed.YouTubeFeedEntry.VideoId;
      var channelId = feed.YouTubeFeedEntry.ChannelId;

      var channelResult = await repository.FindYouTubeSubscription(channelId, CancellationToken.None);

      if (channelResult.IsNotType(typeof(YouTubeSubscriptionModel))) {
        return StatusCode(200);
      }

      var channel = channelResult.GetT1;

      await eventBus.SendYouTubeChannelPost(channel.ChannelName, $"https://www.youtube.com/watch?v={videoId}");
    } catch (Exception error) {
      logger.LogError(error, "Exception occured handling YouTube subscription notification");
    }

    return StatusCode(200);
  }
}
