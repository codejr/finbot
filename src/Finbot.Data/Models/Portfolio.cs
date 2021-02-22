using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Finbot.Data.Models
{
    public class Portfolio
    {
        public int PortfolioId { get; set; }

        public string DiscordUserId { get; set; }

        public decimal CashBalance { get; set; }

        public List<Position> Positions { get; } = new List<Position>();

        public decimal MarketValue => Positions.Sum(p => p.MarketValue) + CashBalance;

        public decimal UnrealizedPnL => MarketValue - Positions.Sum(p => p.AveragePrice * p.Quantity);

        public override string ToString()
        {
            var sb = new StringBuilder();
            foreach (var position in Positions)
            {
                sb.Append($"{position.Symbol} - {position.SecurityType} - Market Value: {position.MarketValue:C} - Unrealized PnL:{position.UnrealizedPnl:C}\r\n");
            }
            sb.Append($"Cash: { CashBalance:C}\r\n");

            sb.Append($"**Total Market Value: {MarketValue:C}**");

            return sb.ToString();
        }
    }
}
