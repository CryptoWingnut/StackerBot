﻿using System.Text;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.Entities;

namespace StackerBot.Services;

public sealed class DiscordBot : IHostedService, IDisposable {
  private readonly DiscordClient _client;

  public DiscordBot(ILoggerFactory logger, IServiceProvider services, EventBus eventBus) {
    _client = new(
      new() {
        Token = Environment.GetEnvironmentVariable("STACKERBOT_API_KEY"),
        TokenType = TokenType.Bot,
        Intents = DiscordIntents.All,
        LoggerFactory = logger
      }
    );

    var commands = _client.UseCommandsNext(new() { StringPrefixes = new[] { "!" }, Services = services });
    commands.RegisterCommands<DiscordCommandsModule>();

    eventBus.OnSendYouTubeChannelPostMessage += SendYouTubeChannelPost;
    eventBus.OnSendMetalsPricePostMessage += SendMetalsPricePost;
    eventBus.OnSendCountdownPostMessage += SendCountdownPost;
    eventBus.OnSendBreakingNewsMessage += SendBreakingNews;
    eventBus.OnGetInvites += GetInvites;
    eventBus.OnGetMember += GetDiscordMember;
    eventBus.OnSendWeeklyLeaderboard += SendWeeklyLeaderboard;
  }

  public async Task StartAsync(CancellationToken cancellationToken) {
    await _client.ConnectAsync();
  }

  public async Task StopAsync(CancellationToken cancellationToken) {
    await _client.DisconnectAsync();
  }

  public void Dispose() {
    _client.DisconnectAsync();
    _client.Dispose();
  }

  private async ValueTask<DiscordMember?> GetDiscordMember(ulong id) {
    var server = await _client.GetGuildAsync(Parameters.STACKER_SOCIAL_SERVER_ID);
    return await server.GetMemberAsync(id);
  }

  private async ValueTask<IReadOnlyList<DiscordInvite>> GetInvites() {
    var server = await _client.GetGuildAsync(Parameters.STACKER_SOCIAL_SERVER_ID);
    return await server.GetInvitesAsync();
  }

  private async ValueTask SendYouTubeChannelPost(string channelName, string url) {
    var channel = await _client.GetChannelAsync(Parameters.YOUTUBE_CHANNEL_ID);
    await channel.SendMessageAsync($"New video from {channelName}: {url}");
  }

  private async ValueTask SendMetalsPricePost(string message) {
    var channel = await _client.GetChannelAsync(Parameters.METALS_PRICE_CHANNEL_ID);
    await channel.SendMessageAsync(message);

    var stackerSocialChannel = await _client.GetChannelAsync(Parameters.STACKER_SOCIAL_CHANNEL_ID);
    await stackerSocialChannel.SendMessageAsync(message);
  }

  private async ValueTask SendCountdownPost(string message) {
    var channel = await _client.GetChannelAsync(Parameters.COUNTDOWN_CHANNEL_ID);
    await channel.SendMessageAsync(message);
  }

  private async ValueTask SendBreakingNews(string from, string body) {
    var channel = await _client.GetChannelAsync(Parameters.BREAKING_NEWS_CHANNEL_ID);
    var message = new StringBuilder();
    message.AppendLine($"NEWS FROM : {from}");
    message.AppendLine(body);

    if (message.Length > 2000) {
      message.Length = 2000;
    }

    await channel.SendMessageAsync(message.ToString());
  }

  private async ValueTask SendWeeklyLeaderboard(string leaderboard) {
    var channel = await _client.GetChannelAsync(Parameters.WEEKLY_LEADERBOARD_CHANNEL_ID);
    await channel.SendMessageAsync(leaderboard);

    var stackerSocialChannel = await _client.GetChannelAsync(Parameters.STACKER_SOCIAL_CHANNEL_ID);
    await stackerSocialChannel.SendMessageAsync(leaderboard);
  }
}
