using Discord;
using Discord.Commands;
using Finbot.Core.Models;
using Finbot.Core.Portfolios;
using Finbot.Data.Models;
using System.Linq;
using System.Threading.Tasks;

namespace Finbot.Core.Modules
{
    public class PortfolioModule : ModuleBase<SocketCommandContext>
    {
        private readonly IPortfolioService portfolioManager;

        public PortfolioModule(IPortfolioService portfolioManager)
        {
            this.portfolioManager = portfolioManager;
        }

        [Command("portfolio")]
        [Summary("View your or someone else's portfolio")]
        public async Task ViewPortfolio(IUser user = null)
        {
            var pUser = user ?? Context.Message.Author;

            var userId = pUser.Id;

            var portfolio = await portfolioManager.GetPortfolioAsync(userId);

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

            var embed = new EmbedBuilder()
                .WithTitle($"{pUser.Username}'s Portfolio")
                .WithColor(Color.Gold)
                .WithFields(fields);

            await ReplyAsync(null, false, embed.Build());
        }

        [Command("buy")]
        [Summary("Buy stock E.g !buy TSLA 10")]
        public async Task StockBuy(string symbol, int quantity)
        {
            var trade = new Trade() 
            {
                Quantity = quantity,
                Symbol = symbol,
                SecurityType = SecurityType.Stock
            };
            
            var result = await portfolioManager.MarketBuy(Context.Message.Author.Id, trade);

            await ReplyAsync($":money_with_wings: Stock buy trade executed for *{quantity}* shares of *{result.Symbol}* at *{result.Price?.ToString("C")}*");
        }

        [Command("buycrypto")]
        [Summary("Buy crypto E.g !buycrypto BTCUSD 0.25")]
        public async Task CryptoBuy(string symbol, decimal quantity)
        {
            var trade = new Trade()
            {
                Quantity = quantity,
                Symbol = symbol,
                SecurityType = SecurityType.Crypto
            };

            var result = await portfolioManager.MarketBuy(Context.Message.Author.Id, trade);

            await ReplyAsync($":money_with_wings: Crypto buy trade executed for *{quantity}* of *{result.Symbol}* at *{result.Price?.ToString("C")}*");
        }

        [Command("sell")]
        [Summary("Sell stock. E.g. !sell TSLA 10")]
        public async Task StockSell(string symbol, int? quantity = null)
        {
            var trade = new Trade()
            {
                Quantity = quantity,
                Symbol = symbol,
                SecurityType = SecurityType.Stock
            };

            var result = await portfolioManager.MarketSell(Context.Message.Author.Id, trade);

            await ReplyAsync($":money_with_wings: Stock sell trade executed for *{quantity?.ToString() ?? "all"}* of *{result.Symbol}* at *{result.Price?.ToString("C")}*");
        }

        [Command("sellcrypto")]
        [Summary("Sell crypto. E.g. !sellcrypto BTCUSD 10")]
        public async Task CryptoSell(string symbol, decimal? quantity = null)
        {
            var trade = new Trade()
            {
                Quantity = quantity,
                Symbol = symbol,
                SecurityType = SecurityType.Crypto
            };

            var result = await portfolioManager.MarketSell(Context.Message.Author.Id, trade);

            await ReplyAsync($":money_with_wings: Crypto sell trade executed for *{quantity?.ToString() ?? "all"}* of *{result.Symbol}* at *{result.Price?.ToString("C")}*");
        }

        [Command("liquidate")]
        [Alias("sellall")]
        [Summary("Sells all your securites and closes all positions.")]
        public async Task Liquidate()
        {
            var mv = await portfolioManager.Liquidate(Context.Message.Author.Id);

            await ReplyAsync($"You sold everything you had for **{mv:C}**");
        }

        [Command("setbalance")]
        [Alias("sb")]
        [RequireOwner(ErrorMessage = "Only bot owner can set balance")]
        public async Task SetBalance(IUser user, decimal balance)
        {
            await portfolioManager.SetBalance(user.Id, balance);

            var embed = new EmbedBuilder()
                .WithTitle("Balance Update")
                .WithColor(Color.Blue)
                .WithDescription($":moneybag: Set {user.Username}'s cash balance to **{balance:C}**")
                .Build();

            await ReplyAsync("", false, embed);
        }
    }
}
