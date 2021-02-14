using Finbot.Core.Models;
using System.Threading.Tasks;

namespace Finbot.Core
{
    public interface IPortfolioManager
    {
        Task<Portfolio> GetPortfolioAsync(ulong userId);
        Task MarketBuy(ulong userId, Trade trade);
    }
}