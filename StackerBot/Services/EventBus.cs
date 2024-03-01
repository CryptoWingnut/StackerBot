namespace StackerBot.Services;

public sealed class EventBus {
  public event SendYouTubeChannelPostMessage? OnSendYouTubeChannelPostMessage;
  public event SendMetalsPricePostMessage? OnSendMetalsPricePostMessage;
  public event SendCountdownPostMessage? OnSendCountdownPostMessage;

  public delegate ValueTask SendYouTubeChannelPostMessage(string channelName, string url);
  public delegate ValueTask SendMetalsPricePostMessage(string message);
  public delegate ValueTask SendCountdownPostMessage(string message);

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

  public async ValueTask SendCountdownPost(string message) {
    if (OnSendCountdownPostMessage is not null) {
      await OnSendCountdownPostMessage(message);
    }
  }
}
