using Finbot.Core.IEX.Models;
using System.Threading.Tasks;

namespace Finbot.Core.IEX
{
    public interface IFinDataClient
    {
        Task<ISecurityPrice> GetCryptoPriceAsync(string symbol);
        Task<ISecurityPrice> GetPriceAsync(string symbol);
    }
}