using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;

namespace StackerBot;

public sealed class DiscordCommandsModule(IExternals externals, IRepository repository) : BaseCommandModule {
  [Command("add-yt")]
  [RequireRoles(RoleCheckMode.Any, Parameters.REQUIRED_ROLE)]
  public async Task AddYouTubeSubscription(CommandContext context, string channelName) {
    var existingResult = await repository.FindYouTubeSubscription(channelName, CancellationToken.None);
    if (existingResult.IsType(typeof(YouTubeSubscriptionModel))) {
      await context.RespondAsync($"Already subscribed to: {channelName}");
      return;
    }

    var channelId = await externals.GetYouTubeChannelId(channelName);

    if (channelId == null) {
      await context.RespondAsync($"Failed to find channel: {channelName}");
      return;
    }

    var success = await externals.SubscribeToYouTubeChannel(channelId);

    if (!success) {
      await context.RespondAsync($"Failed to subscribe to: {channelName}");
      return;
    }

    var subscription = new YouTubeSubscriptionModel {
      Id = Guid.NewGuid(), ChannelId = channelId, ChannelName = channelName, AddedAt = DateTime.UtcNow,
      AddedBy = context.User.Id, LastVideo = DateTime.UtcNow
    };

    await repository.AddYouTubeSubscription(subscription, CancellationToken.None);
    await context.RespondAsync($"Subscribed to: {channelName}");
  }

  [Command("remove-yt")]
  [RequireRoles(RoleCheckMode.Any, Parameters.REQUIRED_ROLE)]
  public async Task RemoveYouTubeSubscription(CommandContext context, string channelName) {
    var existingResult = await repository.FindYouTubeSubscription(channelName, CancellationToken.None);
    if (existingResult.IsType(typeof(NotFound))) {
      await context.RespondAsync($"Not subscribed to: {channelName}");
      return;
    }

    var channel = existingResult.GetT1;
    var success = await externals.UnSubscribeFromYouTubeChannel(channel.ChannelId);

    if (!success) {
      await context.RespondAsync($"Failed to unsubscribe from: {channelName}");
      return;
    }

    await repository.RemoveYouTubeSubscription(channel.Id, CancellationToken.None);
    await context.RespondAsync($"Unsubscribed from: {channelName}");
  }
}
