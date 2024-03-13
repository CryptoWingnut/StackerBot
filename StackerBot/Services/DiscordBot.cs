using System.Text;
using DSharpPlus;
using DSharpPlus.CommandsNext;

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
}
