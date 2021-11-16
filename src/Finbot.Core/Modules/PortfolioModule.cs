namespace Finbot.Core.Modules;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.Commands;
using Finbot.Core.Models;
using Finbot.Core.Portfolios;
using Finbot.Data.Models;
using Microsoft.Extensions.Logging;

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
        user ??= this.Context.Message.Author;

        var portfolio = await this.portfolioManager.GetPortfolioAsync(user.Id);

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

        this.logger.LogInformation($"{this.Context.User.Username} - viewd {user?.Username ?? "self"}'s portfolio");

        var embed = new EmbedBuilder()
            .WithTitle($"{user.Username}'s Portfolio")
            .WithColor(Color.Gold)
            .WithFields(fields);

        await this.ReplyAsync(null, false, embed.Build());
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

        var result = await this.portfolioManager.MarketBuy(this.Context.Message.Author.Id, trade);

        var msg = $"Stock buy trade executed for *{quantity}* shares of *{result.Symbol}* at *{result.Price?.ToString("C")}*";

        this.logger.LogInformation($"{this.Context.User.Username} - {msg}");

        await this.ReplyAsync($":money_with_wings: {msg}");
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

        var result = await this.portfolioManager.MarketBuy(this.Context.Message.Author.Id, trade);

        var msg = $"Crypto buy trade executed for *{quantity}* of *{result.Symbol}* at *{result.Price?.ToString("C")}*";

        this.logger.LogInformation($"{this.Context.User.Username} - {msg}");

        await this.ReplyAsync($":money_with_wings: {msg}");
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

        var result = await this.portfolioManager.MarketSell(this.Context.Message.Author.Id, trade);

        var msg = $"Stock sell trade executed for *{quantity?.ToString() ?? "all"}* of *{result.Symbol}* at *{result.Price?.ToString("C")}*";

        this.logger.LogInformation($"{this.Context.User.Username} - {msg}");

        await this.ReplyAsync($":money_with_wings: {msg}");
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

        var result = await this.portfolioManager.MarketSell(this.Context.Message.Author.Id, trade);

        var msg = $"Crypto sell trade executed for *{quantity?.ToString() ?? "all"}* of *{result.Symbol}* at *{result.Price?.ToString("C")}*";

        this.logger.LogInformation($"{this.Context.User.Username} - {msg}");

        await this.ReplyAsync($":money_with_wings: {msg}");
    }

    [Command("liquidate")]
    [Alias("sellall")]
    [Summary("Sells all your securites and closes all positions.")]
    public async Task Liquidate()
    {
        var mv = await this.portfolioManager.Liquidate(this.Context.Message.Author.Id);

        this.logger.LogInformation($"{this.Context.User.Username} liquidated own portfolio for {mv}");

        await this.ReplyAsync($"You sold everything you had for **{mv:C}**");
    }

    [Command("liquidate")]
    [Alias("sellall")]
    [Summary("Sells all a user's securites and closes all positions.")]
    [RequireOwner(ErrorMessage = "Only bot owner can force liquidate")]
    public async Task Liquidate(IUser user)
    {
        var mv = await this.portfolioManager.Liquidate(user.Id);

        this.logger.LogInformation($"{this.Context.User.Username} liquidated {user.Username}'s portfolio for {mv}");

        await this.ReplyAsync($"{user.Username} was forced to sell everything for **{mv:C}**");
    }

    [Command("setbalance")]
    [Alias("sb")]
    [RequireOwner(ErrorMessage = "Only bot owner can set balance")]
    public async Task SetBalance(IUser user, decimal balance)
    {
        await this.portfolioManager.SetBalance(user.Id, balance);

        var msg = $"Set {user.Username}'s cash balance to **{balance:C}**";

        this.logger.LogInformation($"{this.Context.User.Username} - {msg}");

        var embed = new EmbedBuilder()
            .WithTitle("Balance Update")
            .WithColor(Color.Blue)
            .WithDescription($":moneybag: {msg}")
            .Build();

        await this.ReplyAsync("", false, embed);
    }
}
