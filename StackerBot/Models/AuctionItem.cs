namespace StackerBot.Models;

public sealed class AuctionItem {
  public Guid Id { get; init; }
  public Guid? HostedAuctionId { get; init; }
  public Guid? BotAuctionId { get; init; }

  public string Title { get; set; }
  public string Description { get; set; }
  public decimal StartPrice { get; set; }
  public uint Lot { get; set; }
}
