﻿using System.Text;
using System.Text.Json;
using CoinGecko.Clients;
using CoinGecko.Interfaces;
using Coravel.Invocable;
using StackerBot.Services;

namespace StackerBot.Tasks;

public sealed class MetalsPricePoller(ILogger<MetalsPricePoller> logger, EventBus eventBus) : IInvocable {
  private readonly ICoinGeckoClient _coinGecko = CoinGeckoClient.Instance;

  public async Task Invoke() {
    try {
      await HandleMetals();
    } catch (Exception error) {
      logger.LogError(error, "Exception occured while polling metals prices");
    }
  }

  private async Task HandleMetals() {
    var client = new HttpClient();

    var response = await client.GetAsync($"https://metals-api.com/api/latest?access_key={Environment.GetEnvironmentVariable("METALS_API_KEY")}&base=USD&symbols=XAU,XAG,XPT,EUR,GBP,CAD");
    var text = await response.Content.ReadAsStringAsync();
    var json = JsonDocument.Parse(text);

    var goldUsd = json.RootElement.GetProperty("rates").GetProperty("USDXAU").GetDecimal();
    var silverUsd = json.RootElement.GetProperty("rates").GetProperty("USDXAG").GetDecimal();
    var platinumUsd = json.RootElement.GetProperty("rates").GetProperty("USDXPT").GetDecimal();

    var cad = json.RootElement.GetProperty("rates").GetProperty("USDCAD").GetDecimal();
    var gbp = json.RootElement.GetProperty("rates").GetProperty("USDGBP").GetDecimal();
    var eur = json.RootElement.GetProperty("rates").GetProperty("USDEUR").GetDecimal();

    var goldCad = goldUsd / cad;
    var silverCad = silverUsd / cad;
    var platinumCad = platinumUsd / cad;

    var goldGbp = goldUsd / gbp;
    var silverGbp = silverUsd / gbp;
    var platinumGbp = platinumUsd / gbp;

    var goldEur = goldUsd / eur;
    var silverEur = silverUsd / eur;
    var platinumEur = platinumUsd / eur;

    var gsr = goldUsd / silverUsd;

    var message = new StringBuilder();

    message.AppendLine("SPOT PRICE UPDATE");
    message.AppendLine("");
    message.AppendLine("GOLD");
    message.AppendLine($"£{goldGbp.ToString("N2")} [GBP]");
    message.AppendLine($"${goldUsd.ToString("N2")} [USD]");
    message.AppendLine($"${goldCad.ToString("N2")} [CAD]");
    message.AppendLine($"€{goldEur.ToString("N2")} [EUR]");
    message.AppendLine("");
    message.AppendLine("SILVER");
    message.AppendLine($"£{silverGbp.ToString("N2")} [GBP]");
    message.AppendLine($"${silverUsd.ToString("N2")} [USD]");
    message.AppendLine($"${silverCad.ToString("N2")} [CAD]");
    message.AppendLine($"€{silverEur.ToString("N2")} [EUR]");
    message.AppendLine("");
    message.AppendLine("PLATINUM");
    message.AppendLine($"£{platinumGbp.ToString("N2")} [GBP]");
    message.AppendLine($"${platinumUsd.ToString("N2")} [USD]");
    message.AppendLine($"${platinumCad.ToString("N2")} [CAD]");
    message.AppendLine($"€{platinumEur.ToString("N2")} [EUR]");
    message.AppendLine("");
    message.AppendLine($"GSR: {gsr.ToString("F1")}");

    await eventBus.SendMetalsPricePost(message.ToString());

    var cryptoResponse = await _coinGecko.SimpleClient.GetSimplePrice(["bitcoin", "ethereum"], ["usd"]);

    var bitcoinUsd = cryptoResponse["bitcoin"]["usd"] ?? 0;
    var ethereumUsd = cryptoResponse["ethereum"]["usd"] ?? 0;

    var bitcoinCad = bitcoinUsd / cad;
    var ethereumCad = ethereumUsd / cad;

    var bitcoinGbp = bitcoinUsd / gbp;
    var ethereumGbp = ethereumUsd / gbp;

    var bitcoinEur = bitcoinUsd / eur;
    var ethereumEur = ethereumUsd / eur;

    var crypto = new StringBuilder();

    crypto.AppendLine("CRYPTO PRICE UPDATE");
    crypto.AppendLine("");
    crypto.AppendLine("BITCOIN");
    crypto.AppendLine($"£{bitcoinGbp.ToString("N2")} [GBP]");
    crypto.AppendLine($"${bitcoinUsd.ToString("N2")} [USD]");
    crypto.AppendLine($"${bitcoinCad.ToString("N2")} [CAD]");
    crypto.AppendLine($"€{bitcoinEur.ToString("N2")} [EUR]");
    crypto.AppendLine("");
    crypto.AppendLine("ETHEREUM");
    crypto.AppendLine($"£{ethereumGbp.ToString("N2")} [GBP]");
    crypto.AppendLine($"${ethereumUsd.ToString("N2")} [USD]");
    crypto.AppendLine($"${ethereumCad.ToString("N2")} [CAD]");
    crypto.AppendLine($"€{ethereumEur.ToString("N2")} [EUR]");

    await eventBus.SendCryptoPricePost(crypto.ToString());
  }
}
