using System.Text;
using System.Text.Json;
using Coravel.Invocable;
using StackerBot.Services;

namespace StackerBot.Tasks;

public sealed class MetalsPricePoller(ILogger<MetalsPricePoller> logger, EventBus eventBus) : IInvocable {
  public async Task Invoke() {
    try {
      await Handle();
    } catch (Exception error) {
      logger.LogError(error, "Exception occured while polling metals prices");
    }
  }

  private async Task Handle() {
    var client = new HttpClient();

    var response = await client.GetAsync($"https://api.metals.dev/v1/latest?api_key={Environment.GetEnvironmentVariable("METALS_API_KEY")}&currency=USD&unit=toz");
    var text = await response.Content.ReadAsStringAsync();
    var json = JsonDocument.Parse(text);

    var goldUsd = json.RootElement.GetProperty("metals").GetProperty("gold").GetDecimal();
    var silverUsd = json.RootElement.GetProperty("metals").GetProperty("silver").GetDecimal();

    var cad = json.RootElement.GetProperty("currencies").GetProperty("CAD").GetDecimal();
    var gbp = json.RootElement.GetProperty("currencies").GetProperty("GBP").GetDecimal();
    var eur = json.RootElement.GetProperty("currencies").GetProperty("EUR").GetDecimal();

    var goldCad = goldUsd / cad;
    var silverCad = silverUsd / cad;

    var goldGbp = goldUsd / gbp;
    var silverGbp = silverUsd / gbp;

    var goldEur = goldUsd / eur;
    var silverEur = silverUsd / eur;

    var gsr = goldUsd / silverUsd;

    var message = new StringBuilder();

    message.AppendLine("SPOT PRICE UPDATE");
    message.AppendLine("");
    message.AppendLine("GOLD");
    message.AppendLine($"£{goldGbp.ToString("F2")} [GBP]");
    message.AppendLine($"${goldUsd.ToString("F2")} [USD]");
    message.AppendLine($"${goldCad.ToString("F2")} [CAD]");
    message.AppendLine($"€{goldEur.ToString("F2")} [EUR]");
    message.AppendLine("");
    message.AppendLine("SILVER");
    message.AppendLine($"£{silverGbp.ToString("F2")} [GBP]");
    message.AppendLine($"${silverUsd.ToString("F2")} [USD]");
    message.AppendLine($"${silverCad.ToString("F2")} [CAD]");
    message.AppendLine($"€{silverEur.ToString("F2")} [EUR]");
    message.AppendLine("");
    message.AppendLine($"GSR: {gsr.ToString("F1")}");

    await eventBus.SendMetalsPricePost(message.ToString());
  }
}
