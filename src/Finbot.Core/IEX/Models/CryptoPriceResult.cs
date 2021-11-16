namespace Finbot.Core.IEX.Models;

public class CryptoPriceResult : ISecurityPrice
{
    public decimal? Price { get; set; }

    public string Symbol { get; set; }
}
