using System.Text;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using StackerBot.Services;

namespace StackerBot;

public sealed class DiscordCommandsModule(IExternals externals, IRepository repository, ILogger<DiscordCommandsModule> logger, EventBus eventBus) : BaseCommandModule {
  private static DateTime _inviteLeaderboardAntispam = DateTime.MinValue;

  [Command("member-list")]
  [RequireRoles(RoleCheckMode.Any, Parameters.REQUIRED_ROLE)]
  public async Task GetMemberList(CommandContext context, string email) {
    try {
      var members = await context.Guild.GetAllMembersAsync();
      var list = new StringBuilder();

      foreach (var member in members) {
        if (member.IsBot) {
          continue;
        }

        list.AppendLine(member.Nickname ?? member.DisplayName ?? member.Username);
      }

      var message = new MimeMessage();
      message.From.Add(new MailboxAddress("SSBot", "stackerbot0@gmail.com"));
      message.To.Add(new MailboxAddress("", email));
      message.Subject = "Stacker Social Member List";

      message.Body = new TextPart("plain") {
        Text = list.ToString()
      };

      using var client = new SmtpClient();
      await client.ConnectAsync("smtp.gmail.com", 587, SecureSocketOptions.StartTls);
      await client.AuthenticateAsync("stackerbot0@gmail.com", Environment.GetEnvironmentVariable("EMAIL_PASSWORD"));
      await client.SendAsync(message);
      await client.DisconnectAsync(true);

      await context.RespondAsync("Member list has been emailed");
    } catch (Exception error) {
      logger.LogError(error, "Exception occured while generating member list");
      await context.RespondAsync("Error! Please notify Wingnut!");
    }
  }

  [Command("invites")]
  public async Task GetInviteLeaderboard(CommandContext context) {
    try {
      if (context.Channel.Id != Parameters.STACKER_SOCIAL_CHANNEL_ID) {
        return;
      }

      if (DateTime.UtcNow < _inviteLeaderboardAntispam.AddHours(2)) {
        return;
      }

      var invites = await eventBus.GetServerInvites();
      var leaderboard = new Dictionary<ulong, int>();

      foreach (var invite in invites) {
        if (invite.Uses == 0) {
          continue;
        }

        if (leaderboard.ContainsKey(invite.Inviter.Id)) {
          leaderboard[invite.Inviter.Id] += invite.Uses;
        } else {
          leaderboard[invite.Inviter.Id] = invite.Uses;
        }
      }

      var sortedLeaderboard = leaderboard.OrderByDescending(x => x.Value).ToList();

      var response = new StringBuilder();
      response.AppendLine("SERVER INVITE LEADERBOARD");
      response.AppendLine("-------------------------");

      var total = sortedLeaderboard.Count > 10 ? 10 : sortedLeaderboard.Count;

      for (var i = 0; i < total; i++) {
        var user = await eventBus.GetMember(sortedLeaderboard[i].Key);
        if (user is not null) {
          response.AppendLine($"{i + 1} :: {user.Username} - {sortedLeaderboard[i].Value} INVITED");
        }
      }

      await context.RespondAsync(response.ToString());
      _inviteLeaderboardAntispam = DateTime.UtcNow;
    } catch (Exception error) {
      logger.LogError(error, "Exception occured while processing invite leaderboard");
      await context.RespondAsync("Error! Please notify Wingnut!");
    }
  }

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
