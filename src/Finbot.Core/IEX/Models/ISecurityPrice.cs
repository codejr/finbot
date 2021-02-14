namespace Finbot.Core.IEX.Models
{
    public interface ISecurityPrice
    {
        string Symbol { get; }

        decimal? Price { get; }
    }
}