using System.Text;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using DSharpPlus.Interactivity.Enums;
using DSharpPlus.Interactivity.Extensions;
using DSharpPlus.SlashCommands;

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

    var slashCommands = _client.UseSlashCommands(new() { Services = services });
    slashCommands.RegisterCommands<AuctionCommandsModule>(Parameters.WINGNUTS_TEST_SERVER_ID);

    var commands = _client.UseCommandsNext(new() { StringPrefixes = new[] { "!" }, Services = services });
    commands.RegisterCommands<DiscordCommandsModule>();

    _client.UseInteractivity(new() { PollBehaviour = PollBehaviour.KeepEmojis, Timeout = TimeSpan.FromMinutes(5) });

    eventBus.OnSendYouTubeChannelPostMessage += SendYouTubeChannelPost;
    eventBus.OnSendMetalsPricePostMessage += SendMetalsPricePost;
    eventBus.OnSendCountdownPostMessage += SendCountdownPost;
    eventBus.OnSendBreakingNewsMessage += SendBreakingNews;
    eventBus.OnGetInvites += GetInvites;
    eventBus.OnGetMember += GetDiscordMember;
    eventBus.OnSendWeeklyLeaderboard += SendWeeklyLeaderboard;
    eventBus.OnGetServerTier += GetServerTier;
    eventBus.OnSendAdminAlert += SendAdminAlert;

    _client.GuildMemberAdded += OnUserJoinServer;
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

  private async Task OnUserJoinServer(DiscordClient _, GuildMemberAddEventArgs args) {
    if (args.Guild.Id != Parameters.STACKER_SOCIAL_SERVER_ID) {
      return;
    }

    var message = $"""
                   Hello {args.Member.Mention} & Welcome to Stackers Social!
                   Here we talk about Gold & Silver, as well as other investments.
                   Please feel free to take a look around and introduce yourself here: https://discord.com/channels/1197631472752939128/1198268080330129470
                   """;

    var channel = await _client.GetChannelAsync(Parameters.STACKER_SOCIAL_CHANNEL_ID);
    await channel.SendMessageAsync(message);
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

  private async ValueTask<PremiumTier> GetServerTier() {
    var server = await _client.GetGuildAsync(Parameters.STACKER_SOCIAL_SERVER_ID);
    return server.PremiumTier;
  }

  private async ValueTask SendAdminAlert(string message) {
    var channel = await _client.GetChannelAsync(Parameters.STAFF_ROOM_CHANNEL_ID);
    await channel.SendMessageAsync($"@here ALERT! {message}");
  }
}
