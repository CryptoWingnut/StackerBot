using DSharpPlus.Entities;

namespace StackerBot.Services;

public sealed class EventBus {
  public event SendYouTubeChannelPostMessage? OnSendYouTubeChannelPostMessage;
  public event SendMetalsPricePostMessage? OnSendMetalsPricePostMessage;
  public event SendCountdownPostMessage? OnSendCountdownPostMessage;
  public event SendBreakingNewsMessage? OnSendBreakingNewsMessage;
  public event GetInvites? OnGetInvites;

  public delegate ValueTask SendYouTubeChannelPostMessage(string channelName, string url);
  public delegate ValueTask SendMetalsPricePostMessage(string message);
  public delegate ValueTask SendCountdownPostMessage(string message);
  public delegate ValueTask SendBreakingNewsMessage(string from, string body);
  public delegate ValueTask<IReadOnlyList<DiscordInvite>> GetInvites();

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

  public async ValueTask SendBreakingNews(string from, string body) {
    if (OnSendBreakingNewsMessage is not null) {
      await OnSendBreakingNewsMessage(from, body);
    }
  }

  public async ValueTask<IReadOnlyList<DiscordInvite>> GetServerInvites() {
    if (OnGetInvites is not null) {
      return await OnGetInvites();
    }

    return [];
  }
}
