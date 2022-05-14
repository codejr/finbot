namespace Finbot.Core.Portfolios;
using System.Threading.Tasks;
using Finbot.Core.IEX.Models;
using Finbot.Core.Models;
using Finbot.Data.Models;

public interface IPortfolioService
{
    Task<Portfolio> GetPortfolioAsync(ulong userId);

    Task<ISecurityPrice> MarketBuy(ulong userId, Trade trade);

    Task<ISecurityPrice> MarketSell(ulong userId, Trade trade);

    Task<decimal> Liquidate(ulong userId);

    Task SetBalance(ulong userId, decimal newBalance);
    Task AddFunds(ulong userId, decimal amount);
}
