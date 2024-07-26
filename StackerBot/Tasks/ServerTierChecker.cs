using Coravel.Invocable;
using DSharpPlus;
using StackerBot.Services;

namespace StackerBot.Tasks;

public sealed class ServerTierChecker(ILogger<ServerTierChecker> logger, EventBus eventBus): IInvocable {
  public async Task Invoke() {
    try {
      await Handle();
    } catch (Exception error) {
      logger.LogError(error, "Exception occured checking server tier");
    }
  }

  private async Task Handle() {
    var tier = await eventBus.GetCurrentServerTier();

    if (tier != PremiumTier.Tier_3) {
      await eventBus.SendAlert($"SERVER NOT AT MAXIMUM BOOSTS!! SERVER AT TIER: {tier}");
    }
  }
}
