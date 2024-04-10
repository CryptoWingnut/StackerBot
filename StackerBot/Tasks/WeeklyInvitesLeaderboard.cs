using System.Text;
using Coravel.Invocable;
using StackerBot.Services;

namespace StackerBot.Tasks;

public sealed class WeeklyInvitesLeaderboard(ILogger<WeeklyInvitesLeaderboard> logger, EventBus eventBus) : IInvocable {
  public async Task Invoke() {
    try {
      await Handle();
    } catch (Exception error) {
      logger.LogError(error, "Exception occured while handling weekly invites leaderboard");
    }
  }

  private async ValueTask Handle() {
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

    await eventBus.SendInvitesLeaderboard(response.ToString());
  }
}
