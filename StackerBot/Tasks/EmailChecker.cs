using Coravel.Invocable;
using MailKit;
using MailKit.Net.Imap;
using MailKit.Search;
using StackerBot.Services;

namespace StackerBot.Tasks;

public sealed class EmailChecker(ILogger<EmailChecker> logger, IRepository repository, EventBus eventBus) : IInvocable {
  public async Task Invoke() {
    try {
      await Handle();
    } catch (Exception error) {
      logger.LogError(error, "Exception occured while checking emails");
    }
  }

  private async Task Handle() {
    using var client = new ImapClient();
    await client.ConnectAsync("imap.gmail.com", 993, true, CancellationToken.None);
    await client.AuthenticateAsync("stackerbot0@gmail.com", Environment.GetEnvironmentVariable("EMAIL_PASSWORD"));

    var inbox = client.Inbox;
    await inbox.OpenAsync(FolderAccess.ReadWrite, CancellationToken.None);

    var unreadMessages = await inbox.SearchAsync(SearchQuery.NotSeen);

    foreach (var uid in unreadMessages) {
      var message = await inbox.GetMessageAsync(uid);
      var sender = message.From.Mailboxes.FirstOrDefault()?.Address;

      if (sender is null) {
        continue;
      }

      var whitelistedResult = await repository.IsEmailWhitelisted(sender, CancellationToken.None);

      if (whitelistedResult.IsNotType(typeof(bool))) {
        logger.LogError("Failed to check if email is whitelisted");
        continue;
      }

      var whitelisted = whitelistedResult.GetT1;

      if (!whitelisted) {
        continue;
      }

      logger.LogInformation("Sending breaking news from: {Sender}", sender);
      await eventBus.SendBreakingNews(sender, message.Subject, message.TextBody);
      await inbox.AddFlagsAsync(uid, MessageFlags.Seen, true, CancellationToken.None);
    }
  }
}
