namespace StackerBot.Services;

public sealed class EventBus {
  public event SendYouTubeChannelPostMessage? OnSendYouTubeChannelPostMessage;

  public delegate ValueTask SendYouTubeChannelPostMessage(string channelName, string url);

  public async ValueTask SendYouTubeChannelPost(string channelName, string url) {
    if (OnSendYouTubeChannelPostMessage is not null) {
      await OnSendYouTubeChannelPostMessage(channelName, url);
    }
  }
}
