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
}
