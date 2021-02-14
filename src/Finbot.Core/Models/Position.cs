using System;
using System.Collections.Generic;
using System.Text;

namespace Finbot.Core.Models
{
    public enum SecurityType
    {
        Stock,
        Crypto
    }

    public class Position
    {
        public string Symbol { get; set; }

        public decimal Quantity { get; set; }

        public decimal AveragePrice { get; set; }

        public decimal LatestPrice { get; set; }

        public SecurityType SecurityType { get; set; }

        public decimal UnrealizedPnl => Quantity * (LatestPrice - AveragePrice);

        public decimal MarketValue => LatestPrice * Quantity;
    }
}
