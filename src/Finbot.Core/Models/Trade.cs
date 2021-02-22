using Finbot.Data.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace Finbot.Core.Models
{
    public class Trade
    {
        public string Symbol { get; set; }

        public SecurityType SecurityType { get; set; }

        public decimal? Quantity { get; set; }
    }
}
