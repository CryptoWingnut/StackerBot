namespace StackerBot.Interfaces;

public interface IExternals {
  ValueTask<bool> SubscribeToYouTubeChannel(string channelId);
  ValueTask<bool> UnSubscribeFromYouTubeChannel(string channelId);
  ValueTask<string?> GetYouTubeChannelId(string channelName);
}
