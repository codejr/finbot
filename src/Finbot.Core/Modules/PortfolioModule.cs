using Discord;
using Discord.Commands;
using Finbot.Core.Models;
using Finbot.Core.Portfolios;
using Finbot.Data.Models;
using Microsoft.Extensions.Logging;
using System.Linq;
using System.Threading.Tasks;

namespace Finbot.Core.Modules
{
    public class PortfolioModule : ModuleBase<SocketCommandContext>
    {
        private readonly IPortfolioService portfolioManager;
        private readonly ILogger<PortfolioModule> logger;

        public PortfolioModule(IPortfolioService portfolioManager, ILogger<PortfolioModule> logger)
        {
            this.portfolioManager = portfolioManager;
            this.logger = logger;
        }

        [Command("portfolio")]
        [Alias("pf")]
        [Summary("View your or someone else's portfolio")]
        public async Task ViewPortfolio(IUser user = null)
        {
            user ??= Context.Message.Author;

            var portfolio = await portfolioManager.GetPortfolioAsync(user.Id);

            var fields = portfolio.Positions
                .Select(p => new EmbedFieldBuilder()
                        .WithName($"{p.Symbol.ToUpper()} - {p.SecurityType}")
                        .WithValue($"Market Value: {p.MarketValue:C} Unrealized PnL: {p.UnrealizedPnl:C}"))
                .ToList();

            fields.AddRange(new[]
            {
                new EmbedFieldBuilder().WithName("Cash").WithValue($"{portfolio.CashBalance:C}"),
                new EmbedFieldBuilder().WithName("Totals").WithValue($"**Market Value: {portfolio.MarketValue:C}**")
            });

            logger.LogInformation($"{Context.User.Username} - viewd {user?.Username ?? "self"}'s portfolio");

            var embed = new EmbedBuilder()
                .WithTitle($"{user.Username}'s Portfolio")
                .WithColor(Color.Gold)
                .WithFields(fields);

            await ReplyAsync(null, false, embed.Build());
        }

        [Command("buy")]
        [Alias("b")]
        [Summary("Buy stock E.g !buy TSLA 10")]
        public async Task StockBuy(string symbol, int quantity)
        {
            var trade = new Trade() 
            {
                Quantity = quantity,
                Symbol = symbol.ToUpper(),
                SecurityType = SecurityType.Stock
            };
            
            var result = await portfolioManager.MarketBuy(Context.Message.Author.Id, trade);

            var msg = $"Stock buy trade executed for *{quantity}* shares of *{result.Symbol}* at *{result.Price?.ToString("C")}*";

            logger.LogInformation($"{Context.User.Username} - {msg}");

            await ReplyAsync($":money_with_wings: {msg}");
        }

        [Command("buycrypto")]
        [Alias("bc")]
        [Summary("Buy crypto E.g !buycrypto BTCUSD 0.25")]
        public async Task CryptoBuy(string symbol, decimal quantity)
        {
            var trade = new Trade()
            {
                Quantity = quantity,
                Symbol = symbol.ToUpper(),
                SecurityType = SecurityType.Crypto
            };

            var result = await portfolioManager.MarketBuy(Context.Message.Author.Id, trade);

            var msg = $"Crypto buy trade executed for *{quantity}* of *{result.Symbol}* at *{result.Price?.ToString("C")}*";

            logger.LogInformation($"{Context.User.Username} - {msg}");

            await ReplyAsync($":money_with_wings: {msg}");
        }

        [Command("sell")]
        [Alias("sell")]
        [Summary("Sell stock. E.g. !sell TSLA 10")]
        public async Task StockSell(string symbol, int? quantity = null)
        {
            var trade = new Trade()
            {
                Quantity = quantity,
                Symbol = symbol.ToUpper(),
                SecurityType = SecurityType.Stock
            };

            var result = await portfolioManager.MarketSell(Context.Message.Author.Id, trade);

            var msg = $"Stock sell trade executed for *{quantity?.ToString() ?? "all"}* of *{result.Symbol}* at *{result.Price?.ToString("C")}*";

            logger.LogInformation($"{Context.User.Username} - {msg}");

            await ReplyAsync($":money_with_wings: {msg}");
        }

        [Command("sellcrypto")]
        [Alias("sc")]
        [Summary("Sell crypto. E.g. !sellcrypto BTCUSD 10")]
        public async Task CryptoSell(string symbol, decimal? quantity = null)
        {
            var trade = new Trade()
            {
                Quantity = quantity,
                Symbol = symbol.ToUpper(),
                SecurityType = SecurityType.Crypto
            };

            var result = await portfolioManager.MarketSell(Context.Message.Author.Id, trade);

            var msg = $"Crypto sell trade executed for *{quantity?.ToString() ?? "all"}* of *{result.Symbol}* at *{result.Price?.ToString("C")}*";

            logger.LogInformation($"{Context.User.Username} - {msg}");

            await ReplyAsync($":money_with_wings: {msg}");
        }

        [Command("liquidate")]
        [Alias("sellall")]
        [Summary("Sells all your securites and closes all positions.")]
        public async Task Liquidate()
        {
            var mv = await portfolioManager.Liquidate(Context.Message.Author.Id);

            logger.LogInformation($"{Context.User.Username} liquidated own portfolio for {mv}");

            await ReplyAsync($"You sold everything you had for **{mv:C}**");
        }

        [Command("liquidate")]
        [Alias("sellall")]
        [Summary("Sells all a user's securites and closes all positions.")]
        [RequireOwner(ErrorMessage = "Only bot owner can force liquidate")]
        public async Task Liquidate(IUser user)
        {
            var mv = await portfolioManager.Liquidate(user.Id);

            logger.LogInformation($"{Context.User.Username} liquidated {user.Username}'s portfolio for {mv}");

            await ReplyAsync($"{user.Username} was forced to sell everything for **{mv:C}**");
        }

        [Command("setbalance")]
        [Alias("sb")]
        [RequireOwner(ErrorMessage = "Only bot owner can set balance")]
        public async Task SetBalance(IUser user, decimal balance)
        {
            await portfolioManager.SetBalance(user.Id, balance);

            var msg = $"Set {user.Username}'s cash balance to **{balance:C}**";

            logger.LogInformation($"{Context.User.Username} - {msg}");

            var embed = new EmbedBuilder()
                .WithTitle("Balance Update")
                .WithColor(Color.Blue)
                .WithDescription($":moneybag: {msg}")
                .Build();

            await ReplyAsync("", false, embed);
        }
    }
}
