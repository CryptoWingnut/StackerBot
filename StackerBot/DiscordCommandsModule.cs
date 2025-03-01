using System.Text;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;

namespace StackerBot;

public sealed class DiscordCommandsModule(IExternals externals, IRepository repository, ILogger<DiscordCommandsModule> logger) : BaseCommandModule {
  [Command("join-stats")]
  [RequireRoles(RoleCheckMode.Any, Parameters.REQUIRED_ROLE, Parameters.MODERATOR_ROLE)]
  public async Task GenerateJoinStatistic(CommandContext context) {
    try {
      var statistics = new Dictionary<int, Dictionary<int, int>>();
      var members = await context.Guild.GetAllMembersAsync();

      foreach (var member in members) {
        if (member.IsBot) {
          continue;
        }

        var year = member.JoinedAt.Year;
        var month = member.JoinedAt.Month;

        if (!statistics.TryGetValue(year, out var yearValue)) {
          yearValue = new Dictionary<int, int>();
          statistics[year] = yearValue;
        }

        var yearStats = yearValue;

        if (!yearValue.TryGetValue(month, out var monthValue)) {
          monthValue = 0;
          yearStats[month] = monthValue;
        }

        yearStats[month]++;
      }

      var output = new StringBuilder();

      var yearsPresent = statistics.Keys.ToList();
      yearsPresent.Sort();

      foreach (var yearPresent in yearsPresent) {
        var yearStats = statistics[yearPresent];

        if (yearStats.TryGetValue(1, out var jan)) {
          output.AppendLine($"01/{yearPresent},{jan}");
        }

        if (yearStats.TryGetValue(2, out var feb)) {
          output.AppendLine($"02/{yearPresent},{feb}");
        }

        if (yearStats.TryGetValue(3, out var mar)) {
          output.AppendLine($"03/{yearPresent},{mar}");
        }

        if (yearStats.TryGetValue(4, out var apr)) {
          output.AppendLine($"04/{yearPresent},{apr}");
        }

        if (yearStats.TryGetValue(5, out var may)) {
          output.AppendLine($"05/{yearPresent},{may}");
        }

        if (yearStats.TryGetValue(6, out var jun)) {
          output.AppendLine($"06/{yearPresent},{jun}");
        }

        if (yearStats.TryGetValue(7, out var jul)) {
          output.AppendLine($"07/{yearPresent},{jul}");
        }

        if (yearStats.TryGetValue(8, out var aug)) {
          output.AppendLine($"08/{yearPresent},{aug}");
        }

        if (yearStats.TryGetValue(9, out var sep)) {
          output.AppendLine($"09/{yearPresent},{sep}");
        }

        if (yearStats.TryGetValue(10, out var oct)) {
          output.AppendLine($"10/{yearPresent},{oct}");
        }

        if (yearStats.TryGetValue(11, out var nov)) {
          output.AppendLine($"11/{yearPresent},{nov}");
        }

        if (yearStats.TryGetValue(12, out var dec)) {
          output.AppendLine($"12/{yearPresent},{dec}");
        }
      }

      var bytes = Encoding.UTF8.GetBytes(output.ToString());
      using var stream = new MemoryStream(bytes);

      var builder = new DiscordMessageBuilder()
        .WithContent("Here is the join statistics!")
        .AddFile("join-stats.csv", stream);

      await context.RespondAsync(builder);
    } catch (Exception error) {
      logger.LogError(error, "Exception occured while generating join statistics");
      await context.RespondAsync("Error! Please notify Wingnut!");
    }
  }

  [Command("member-list")]
  [RequireRoles(RoleCheckMode.Any, Parameters.REQUIRED_ROLE)]
  public async Task GetMemberList(CommandContext context) {
    try {
      var members = await context.Guild.GetAllMembersAsync();
      var list = new StringBuilder();

      foreach (var member in members) {
        if (member.IsBot) {
          continue;
        }

        list.AppendLine(member.Nickname ?? member.DisplayName ?? member.Username);
      }

      var bytes = Encoding.UTF8.GetBytes(list.ToString());
      using var stream = new MemoryStream(bytes);

      var builder = new DiscordMessageBuilder()
        .WithContent("Here is the member list!")
        .AddFile("member-list.txt", stream);

      await context.RespondAsync(builder);
    } catch (Exception error) {
      logger.LogError(error, "Exception occured while generating member list");
      await context.RespondAsync("Error! Please notify Wingnut!");
    }
  }

  [Command("add-yt")]
  [RequireRoles(RoleCheckMode.Any, Parameters.REQUIRED_ROLE, Parameters.MODERATOR_ROLE)]
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
  [RequireRoles(RoleCheckMode.Any, Parameters.REQUIRED_ROLE, Parameters.MODERATOR_ROLE)]
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

  [Command("add-email")]
  [RequireRoles(RoleCheckMode.Any, Parameters.REQUIRED_ROLE)]
  public async Task AddWhitelistedEmail(CommandContext context, string email) {
    try {
      var existingResult = await repository.IsEmailWhitelisted(email, CancellationToken.None);
      if (existingResult.IsType(typeof(bool))) {
        var existing = existingResult.GetT1;
        if (existing) {
          await context.RespondAsync($"Already whitelisted: {email}");
          return;
        }
      }

      var model = new WhitelistedEmailModel { Id = Guid.NewGuid(), Address = email };
      var addedResponse = await repository.AddWhitelistedEmail(model, CancellationToken.None);

      if (addedResponse.IsType(typeof(DatabaseError))) {
        await context.RespondAsync("Error! Please notify Wingnut!");
        return;
      }

      await context.RespondAsync($"Whitelisted: {email}");
    } catch (Exception error) {
      logger.LogError(error, "Exception occured while adding whitelisted email");
      await context.RespondAsync("Error! Please notify Wingnut!");
    }
  }

  [Command("remove-email")]
  [RequireRoles(RoleCheckMode.Any, Parameters.REQUIRED_ROLE)]
  public async Task RemoveWhitelistedEmail(CommandContext context, string email) {
    try {
      var existingResult = await repository.IsEmailWhitelisted(email, CancellationToken.None);
      if (existingResult.IsType(typeof(bool))) {
        var existing = existingResult.GetT1;
        if (!existing) {
          await context.RespondAsync($"Address not whitelisted: {email}");
          return;
        }
      }

      var removeResponse = await repository.RemoveWhitelistedEmail(email, CancellationToken.None);

      if (removeResponse.IsType(typeof(DatabaseError))) {
        await context.RespondAsync("Error! Please notify Wingnut!");
        return;
      }

      await context.RespondAsync($"Removed whitelisted email: {email}");
    } catch (Exception error) {
      logger.LogError(error, "Exception occured while removing whitelisted email");
      await context.RespondAsync("Error! Please notify Wingnut!");
    }
  }
}
