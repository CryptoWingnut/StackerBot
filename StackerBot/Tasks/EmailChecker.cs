using System.Text;
using System.Text.RegularExpressions;

using Coravel.Invocable;

using HtmlAgilityPack;

using MailKit;
using MailKit.Net.Imap;
using MailKit.Search;
using StackerBot.Services;

namespace StackerBot.Tasks;

public sealed partial class EmailChecker(ILogger<EmailChecker> logger, IRepository repository, EventBus eventBus) : IInvocable {
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

      var htmlDoc = new HtmlDocument();
      htmlDoc.LoadHtml(message.HtmlBody ?? "");
      var formattedText = ConvertHtmlToPlainText(htmlDoc.DocumentNode);

      await eventBus.SendBreakingNews(sender, formattedText);
      await inbox.AddFlagsAsync(uid, MessageFlags.Seen, true, CancellationToken.None);
    }
  }

  private static int _counter;

  private static string ConvertHtmlToPlainText(HtmlNode node) {
    var sb = new StringBuilder();

    foreach (var childNode in node.ChildNodes) {
      switch (childNode.NodeType) {
        case HtmlNodeType.Element:
          switch (childNode.Name) {
            case "meta":
            case "style":
            case "link":
              break;

            case "html":
            case "head":
            case "title":
            case "body":
            case "center":
            case "table":
            case "tbody":
            case "tr":
            case "td":
            case "div":
            case "p":
            case "span":
            case "strong":
            case "colgroup":
            case "col":
            case "h1":
            case "br":
            case "img":
              sb.Append(ConvertHtmlToPlainText(childNode));
              break;

            case "a":
              if (childNode.Attributes["href"] != null) {
                sb.AppendLine($"<{childNode.Attributes["href"].Value}>");
              }
              break;

            default:
              Console.WriteLine("UNKNOWN NODE: " + childNode.Name);
              Console.WriteLine("CHILDREN: " + childNode.ChildNodes.Count);
              Console.WriteLine(childNode.InnerText);
              Console.WriteLine("====");
              sb.Append(ConvertHtmlToPlainText(childNode));
              break;
          }
          break;
        case HtmlNodeType.Text:
          sb.AppendLine(childNode.InnerText);
          break;
      }
    }

    var result = sb.ToString();
    result = RemoveExtraLineBreaks().Replace(result, "\n\n");
    return result;
  }

  [GeneratedRegex(@"\n{3,}")]
  private static partial Regex RemoveExtraLineBreaks();
}
