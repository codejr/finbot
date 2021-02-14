using Discord;
using Discord.Commands;
using Finbot.Core.Models;
using Finbot.Core.Portfolios;
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
            var userId = user?.Id ?? Context.Message.Author.Id;

            var portfolio = await portfolioManager.GetPortfolioAsync(userId);

            await ReplyAsync(portfolio.ToString());
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

            await ReplyAsync($":money_with_wings: Trade executed for *{quantity}* shares of *{result.Symbol}* at *{result.Price?.ToString("C")}*");
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

            await ReplyAsync($":money_with_wings: Crypto trade executed for *{quantity}* of *{result.Symbol}* at *{result.Price?.ToString("C")}*");
        }
    }
}
