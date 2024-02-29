namespace StackerBot.Services;

public sealed class EventBus {
  public event SendYouTubeChannelPostMessage? OnSendYouTubeChannelPostMessage;
  public event SendMetalsPricePostMessage? OnSendMetalsPricePostMessage;

  public delegate ValueTask SendYouTubeChannelPostMessage(string channelName, string url);
  public delegate ValueTask SendMetalsPricePostMessage(string message);

  public async ValueTask SendYouTubeChannelPost(string channelName, string url) {
    if (OnSendYouTubeChannelPostMessage is not null) {
      await OnSendYouTubeChannelPostMessage(channelName, url);
    }
  }

  public async ValueTask SendMetalsPricePost(string message) {
    if (OnSendMetalsPricePostMessage is not null) {
      await OnSendMetalsPricePostMessage(message);
    }
  }
}
