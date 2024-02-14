﻿using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;

namespace StackerBot;

public sealed class DiscordCommandsModule(IExternals externals, IRepository repository, ILogger<DiscordCommandsModule> logger) : BaseCommandModule {
  [Command("add-yt")]
  [RequireRoles(RoleCheckMode.Any, Parameters.REQUIRED_ROLE)]
  public async Task AddYouTubeSubscription(CommandContext context, string channelName) {
    try {
      var channelNameActual = $"@{channelName}";

      if (channelNameActual == "@" && context.Message.MentionedUsers.Count > 0) {
        channelNameActual = $"@{context.Message.MentionedUsers[0].Username}";
      }

      var existingResult = await repository.FindYouTubeSubscription(channelNameActual, CancellationToken.None);
      if (existingResult.IsType(typeof(YouTubeSubscriptionModel))) {
        await context.RespondAsync($"Already subscribed to: {channelNameActual}");
        return;
      }

      var channelId = await externals.GetYouTubeChannelId(channelNameActual);

      if (channelId == null) {
        await context.RespondAsync($"Failed to find channel: {channelNameActual}");
        return;
      }

      var success = await externals.SubscribeToYouTubeChannel(channelId);

      if (!success) {
        await context.RespondAsync($"Failed to subscribe to: {channelNameActual}");
        return;
      }

      var subscription = new YouTubeSubscriptionModel {
        Id = Guid.NewGuid(), ChannelId = channelId, ChannelName = channelNameActual, AddedAt = DateTime.UtcNow,
        AddedBy = context.User.Id, LastVideo = DateTime.UtcNow
      };

      await repository.AddYouTubeSubscription(subscription, CancellationToken.None);
      await context.RespondAsync($"Subscribed to: {channelNameActual}");
    } catch (Exception error) {
      logger.LogError(error, "Exception occured while adding YouTube subscription");
      await context.RespondAsync("Error! Please notify Wingnut!");
    }
  }

  [Command("remove-yt")]
  [RequireRoles(RoleCheckMode.Any, Parameters.REQUIRED_ROLE)]
  public async Task RemoveYouTubeSubscription(CommandContext context, string channelName) {
    try {
      var channelNameActual = $"@{channelName}";

      if (channelNameActual == "@" && context.Message.MentionedUsers.Count > 0) {
        channelNameActual = $"@{context.Message.MentionedUsers[0].Username}";
      }

      var existingResult = await repository.FindYouTubeSubscription(channelNameActual, CancellationToken.None);
      if (existingResult.IsType(typeof(NotFound))) {
        await context.RespondAsync($"Not subscribed to: {channelNameActual}");
        return;
      }

      var channel = existingResult.GetT1;
      var success = await externals.UnSubscribeFromYouTubeChannel(channel.ChannelId);

      if (!success) {
        await context.RespondAsync($"Failed to unsubscribe from: {channelNameActual}");
        return;
      }

      await repository.RemoveYouTubeSubscription(channel.Id, CancellationToken.None);
      await context.RespondAsync($"Unsubscribed from: {channelNameActual}");
    } catch (Exception error) {
      logger.LogError(error, "Exception occured while removing YouTube subscription");
      await context.RespondAsync("Error! Please notify Wingnut!");
    }
  }
}
