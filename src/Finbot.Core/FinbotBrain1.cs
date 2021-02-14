using Discord;
using Discord.WebSocket;
using Finbot.Core.IEX;
using Finbot.Core.Models;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Finbot.Core
{
    public class FinbotBrain : IFinbotBrain
    {
        private readonly DiscordSocketClient client;

        private readonly IFinDataClient finDataClient;

        private readonly ILogger<FinbotBrain> logger;

        private readonly IPortfolioManager portfolioManager;

        private readonly string token;

        public FinbotBrain(string token,
            IFinDataClient finDataClient,
            ILogger<FinbotBrain> logger,
            IPortfolioManager portfolioManager)
        {
            this.token = token;
            this.logger = logger;
            this.portfolioManager = portfolioManager;
            this.finDataClient = finDataClient;
            client = new DiscordSocketClient();
            client.Log += LogAsync;
            client.Ready += ReadyAsync;
            client.MessageReceived += MessageReceivedAsync;
        }

        public async Task RunAsync(CancellationToken cancellationToken)
        {
            await client.LoginAsync(TokenType.Bot, token);
            await client.StartAsync();

            await Task.Delay(Timeout.Infinite, cancellationToken);
        }

        private Task LogAsync(LogMessage log)
        {
            switch (log.Severity)
            {
                case LogSeverity.Critical:
                    logger.LogCritical(log.ToString());
                    break;

                case LogSeverity.Error:
                    logger.LogError(log.ToString());
                    break;

                case LogSeverity.Warning:
                    logger.LogWarning(log.ToString());
                    break;

                case LogSeverity.Info:
                    logger.LogInformation(log.ToString());
                    break;

                default:
                    logger.LogDebug(log.ToString());
                    break;
            }

            return Task.CompletedTask;
        }

        private Task ReadyAsync()
        {
            logger.LogInformation($"{client.CurrentUser} is connected!");

            return Task.CompletedTask;
        }

        // TODO: Clean this up with proper command system
        private async Task MessageReceivedAsync(SocketMessage message)
        {
            logger.LogDebug($"Received Message: {message.Content}");

            if (message.Author.Id == client.CurrentUser.Id)
                return;

            if (message.Content == "!ping")
                await message.Channel.SendMessageAsync("pong!");

            if (message.Content.StartsWith("!pricecrypto"))
            {
                var symbol = message.Content.Split(' ')[1];

                var priceResponce = await finDataClient.GetCryptoPriceAsync(symbol);

                await message.Channel.SendMessageAsync($":moneybag: Price for **{priceResponce.Symbol}** at **{priceResponce.Price?.ToString("C")}**");
            }

            if (message.Content.StartsWith("!price"))
            {
                var symbol = message.Content.Split(' ')[1];

                var priceResponce = await finDataClient.GetPriceAsync(symbol);

                await message.Channel.SendMessageAsync($":moneybag: Price for **{priceResponce.Symbol}** at **{priceResponce.Price?.ToString("C")}**");
            }

            if (message.Content == "!portfolio")
            {
                var portfolio = await portfolioManager.GetPortfolioAsync(message.Author.Id);

                await message.Channel.SendMessageAsync(portfolio.ToString());
            }

            if (message.Content.StartsWith("!buy"))
            {
                var split = message.Content.Split(' ');
                var symbol = split[1];
                var quantity = decimal.Parse(split[2]);
                var securityType = message.Content.StartsWith("!buycrypto") ? SecurityType.Crypto : SecurityType.Stock;

                var trade = new Trade() { Quantity = quantity, Symbol = symbol, SecurityType = securityType };
                try
                {
                    await portfolioManager.MarketBuy(message.Author.Id, trade);
                    await message.Channel.SendMessageAsync($"Trade executed.");
                }
                catch (Exception e)
                {
                    await message.Channel.SendMessageAsync($"Unknown Error: {e.Message}");
                }
            }
        }
    }
}
