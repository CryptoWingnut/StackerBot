using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.Exceptions;
using DSharpPlus.SlashCommands;

namespace StackerBot;

public sealed class AuctionCommandsModule : ApplicationCommandModule {
  [SlashCommand("new-auction", "Create a new auction")]
  public async Task CreateBotAuction(InteractionContext context) {
    try {
      var modal = new DiscordInteractionResponseBuilder()
        .WithTitle("Create New Auction")
        .WithCustomId("create-bot-auction")
        .AddComponents(
          new TextInputComponent("Title", "title", "Please enter your auction title", style: TextInputStyle.Short)
        )
        .AddComponents(
          new TextInputComponent("Description", "description", "Please enter your item description", style: TextInputStyle.Paragraph)
        )
        .AddComponents(
          new TextInputComponent("Start Price", "start-price", "Please enter the starting price for the item", style: TextInputStyle.Short)
        );

      await context.Interaction.CreateResponseAsync(InteractionResponseType.Modal, modal);
    } catch (BadRequestException exception) {
      Console.WriteLine("BAD REQUEST: " + exception);
      Console.WriteLine("ERRORS: " + exception.Errors);
      Console.WriteLine("JSON: " + exception.JsonMessage);
    }
    catch (Exception error) {
      Console.WriteLine("ERROR: " + error);
    }
  }
}
