namespace Finbot.Core.IEX;
using System.Threading.Tasks;
using Finbot.Core.IEX.Models;

public interface IFinDataClient
{
    Task<ISecurityPrice> GetCryptoPriceAsync(string symbol);
    Task<ISecurityPrice> GetPriceAsync(string symbol);
}
