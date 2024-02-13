namespace StackerBot.Models;

[Table("youtube_subscriptions")]
public sealed class YouTubeSubscriptionModel {
  [Key, Column("id")]
  public required Guid Id { get; init; }
  [Column("channel_id"), MaxLength(255)]
  public required string ChannelId { get; init; }
  [Column("channel_name"), MaxLength(255)]
  public required string ChannelName { get; init; }
  [Column("added_by")]
  public required ulong AddedBy { get; init; }
  [Column("added_at")]
  public required DateTime AddedAt { get; init; }
  [Column("last_video")]
  public required DateTime LastVideo { get; set; }
}
