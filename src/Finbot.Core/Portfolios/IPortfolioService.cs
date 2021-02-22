using Finbot.Core.IEX.Models;
using Finbot.Core.Models;
using Finbot.Data.Models;
using System.Threading.Tasks;

namespace Finbot.Core.Portfolios
{
    public interface IPortfolioService
    {
        Task<Portfolio> GetPortfolioAsync(ulong userId);

        Task<ISecurityPrice> MarketBuy(ulong userId, Trade trade);

        Task<ISecurityPrice> MarketSell(ulong userId, Trade trade);

        Task<decimal> Liquidate(ulong userId);

        Task SetBalance(ulong userId, decimal newBalance);
    }
}