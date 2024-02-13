using Microsoft.AspNetCore.Mvc;
using StackerBot.Services;

namespace StackerBot;

[ApiController, Route("webhooks")]
public sealed class WebhooksController(EventBus eventBus, IRepository repository, ILogger<WebhooksController> logger) : ControllerBase {
  [HttpGet("yt_sub_notify")]
  public IActionResult VerifySubscription([FromQuery(Name = "hub.challenge")] string challenge) {
    return StatusCode(200, challenge);
  }

  [HttpPost("yt_sub_notify")]
  public async ValueTask<IActionResult> YouTubeSubscriptionNotify([FromBody] YouTubeFeed feed) {
    try {
      foreach (var entry in feed.YouTubeFeedEntries) {
        if (entry.VideoId is null || entry.ChannelId is null) {
          return StatusCode(200);
        }

        var videoId = entry.VideoId;
        var channelId = entry.ChannelId;
        var channelResult = await repository.FindYouTubeSubscriptionById(channelId, CancellationToken.None);

        if (channelResult.IsNotType(typeof(YouTubeSubscriptionModel))) {
          return StatusCode(200);
        }

        var channel = channelResult.GetT1;


        await eventBus.SendYouTubeChannelPost(channel.ChannelName, $"https://www.youtube.com/watch?v={videoId}");
      }
    } catch (Exception error) {
      logger.LogError(error, "Exception occured handling YouTube subscription notification");
    }

    return StatusCode(200);
  }
}
