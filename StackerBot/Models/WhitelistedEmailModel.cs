namespace StackerBot.Models;

public sealed class WhitelistedEmailModel {
  [Key, Column("id")]
  public required Guid Id { get; init; }
  [Column("email"), MaxLength(255)]
  public required string Address { get; init; }
}
