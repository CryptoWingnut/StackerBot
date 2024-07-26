namespace StackerBot.Models;

public sealed class HostedAuctionModel {
  public Guid Id { get; init; }
  public ulong Creator { get; init; }
  public ulong Host { get; set; }
  public DateTime StartTime { get; set; }
  public string Currency { get; set; }

  public List<AuctionItem> Items { get; set; } = [];
}
