namespace Finbot.Data.Models;
using System.Collections.Generic;
using System.Linq;
using System.Text;

public class Portfolio
{
    public int PortfolioId { get; set; }

    public string DiscordUserId { get; set; }

    public decimal CashBalance { get; set; }

    public List<Position> Positions { get; } = new List<Position>();

    public decimal MarketValue => this.Positions.Sum(p => p.MarketValue) + this.CashBalance;

    public decimal UnrealizedPnL => this.MarketValue - this.Positions.Sum(p => p.AveragePrice * p.Quantity);

    public override string ToString()
    {
        var sb = new StringBuilder();
        foreach (var position in this.Positions)
        {
            sb.Append($"{position.Symbol} - {position.SecurityType} - Market Value: {position.MarketValue:C} - Unrealized PnL:{position.UnrealizedPnl:C}\r\n");
        }
        sb.Append($"Cash: { this.CashBalance:C}\r\n");

        sb.Append($"**Total Market Value: {this.MarketValue:C}**");

        return sb.ToString();
    }
}
