namespace StackerBot;

public sealed class DatabaseContext : DbContext {
  public DbSet<WhitelistedEmailModel> WhitelistedEmails => Set<WhitelistedEmailModel>();
  public DbSet<YouTubeSubscriptionModel> YouTubeSubscriptions => Set<YouTubeSubscriptionModel>();

  public DatabaseContext() {}
  public DatabaseContext(DbContextOptions options) : base(options) {}

  protected override void OnConfiguring(DbContextOptionsBuilder builder) {
    builder.UseNpgsql(Tools.DatabaseConnectString());
  }
}
