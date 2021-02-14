using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Finbot.Core.Models
{
    public class Portfolio
    {
        public ulong UserId { get; set; }

        public decimal CashBalance { get; set; }

        public IList<Position> Positions { get; } = new List<Position>();

        public decimal MarketValue => Positions.Sum(p => p.MarketValue) + CashBalance;

        public decimal UnrealizedPnL => MarketValue - Positions.Sum(p => p.AveragePrice * p.Quantity);

        public override string ToString()
        {
            var sb = new StringBuilder();
            foreach (var position in Positions)
            {
                sb.Append($"{position.Symbol} - {position.SecurityType} - Market Value: {position.MarketValue.ToString("C")} - Unrealized PnL:{position.UnrealizedPnl.ToString("C")}\r\n");
            }
            sb.Append($"Cash: { CashBalance.ToString("C") }\r\n");

            sb.Append($"**Total Market Value: {MarketValue.ToString("C")}**");

            return sb.ToString();
        }
    }
}
