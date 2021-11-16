namespace Finbot.Core.Models;
using Finbot.Data.Models;

public class Trade
{
    public string Symbol { get; set; }

    public SecurityType SecurityType { get; set; }

    public decimal? Quantity { get; set; }
}
