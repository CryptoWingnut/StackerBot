using DSharpPlus;
using DSharpPlus.Entities;

namespace StackerBot.Services;

public sealed class EventBus {
  public event SendYouTubeChannelPostMessage? OnSendYouTubeChannelPostMessage;
  public event SendMetalsPricePostMessage? OnSendMetalsPricePostMessage;
  public event SendCryptoPricePostMessage? OnSendCryptoPricePostMessage;
  public event SendCountdownPostMessage? OnSendCountdownPostMessage;
  public event SendBreakingNewsMessage? OnSendBreakingNewsMessage;
  public event GetInvites? OnGetInvites;
  public event GetDiscordMember? OnGetMember;
  public event SendWeeklyLeaderboard? OnSendWeeklyLeaderboard;
  public event GetServerTier? OnGetServerTier;
  public event SendAdminAlert? OnSendAdminAlert;

  public delegate ValueTask SendYouTubeChannelPostMessage(string channelName, string url);
  public delegate ValueTask SendMetalsPricePostMessage(string message);
  public delegate ValueTask SendCryptoPricePostMessage(string message);
  public delegate ValueTask SendCountdownPostMessage(string message);
  public delegate ValueTask SendBreakingNewsMessage(string from, string body);
  public delegate ValueTask<IReadOnlyList<DiscordInvite>> GetInvites();
  public delegate ValueTask<DiscordMember?> GetDiscordMember(ulong id);
  public delegate ValueTask SendWeeklyLeaderboard(string leaderboard);
  public delegate ValueTask<PremiumTier> GetServerTier();
  public delegate ValueTask SendAdminAlert(string message);

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

  public async ValueTask SendCryptoPricePost(string message) {
    if (OnSendCryptoPricePostMessage is not null) {
      await OnSendCryptoPricePostMessage(message);
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

  public async ValueTask<DiscordMember?> GetMember(ulong id) {
    if (OnGetMember is not null) {
      return await OnGetMember(id);
    }

    return null;
  }

  public async ValueTask SendInvitesLeaderboard(string leaderboard) {
    if (OnSendWeeklyLeaderboard is not null) {
      await OnSendWeeklyLeaderboard(leaderboard);
    }
  }

  public async ValueTask<PremiumTier> GetCurrentServerTier() {
    if (OnGetServerTier is not null) {
      return await OnGetServerTier();
    }

    return PremiumTier.None;
  }

  public async ValueTask SendAlert(string message) {
    if (OnSendAdminAlert is not null) {
      await OnSendAdminAlert(message);
    }
  }
}
