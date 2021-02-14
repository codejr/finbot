namespace Finbot.Core.IEX.Models
{
    public class StockQuote : ISecurityPrice
    {
        public string Symbol { get; set; }

        public decimal? LatestPrice { get; set; }

        public decimal? Price => LatestPrice;
    }
}
