using Finbot.Core.IEX.Models;
using Finbot.Core.Models;
using System.Threading.Tasks;

namespace Finbot.Core.Portfolios
{
    public interface IPortfolioService
    {
        Task<Portfolio> GetPortfolioAsync(ulong userId);
        Task<ISecurityPrice> MarketBuy(ulong userId, Trade trade);
    }
}