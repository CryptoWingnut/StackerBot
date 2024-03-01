using Coravel.Invocable;
using StackerBot.Services;

namespace StackerBot.Tasks;

public sealed class GiveawayCountdown(ILogger<GiveawayCountdown> logger, EventBus eventBus) : IInvocable {
  private readonly DateTime _targetTime = new(2024, 3, 2, 19, 0, 0, DateTimeKind.Utc);

  public async Task Invoke() {
    try {
      await Handle();
    } catch (Exception error) {
      logger.LogError(error, "Exception occured while handling giveaway countdown");
    }
  }

  private async Task Handle() {
    var remaining = _targetTime - DateTime.UtcNow;

    if (remaining.Hours < 0) {
      return;
    }

    var message = remaining.Hours == 0
      ? "IT'S TIME! Head on over to: <https://www.youtube.com/@thestackcollector> for the the live stream and massive giveaway!"
      : $"{remaining.Hours} HOURS UNTIL THE MASSIVE GIVEAWAY{Environment.NewLine}Be sure to check out the live stream at: <https://www.youtube.com/@thestackcollector>";

    await eventBus.SendCountdownPost(message);
  }
}
