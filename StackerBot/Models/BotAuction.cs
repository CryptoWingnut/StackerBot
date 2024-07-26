namespace StackerBot.Models;

public sealed class BotAuction {
  public Guid Id { get; init; }
  public ulong Creator { get; init; }
  public DateTime StartTime { get; set; }
  public DateTime EndTime { get; set; }
  public string Currency { get; set; }

  public AuctionItem Item { get; set; }
}
