using Finbot.Core.IEX;
using Finbot.Core.IEX.Models;
using Finbot.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Finbot.Core.Portfolios
{
    public class PortfolioService : IPortfolioService
    {
        private const decimal defaultBalance = 100_000;

        private readonly IFinDataClient client;

        private IDictionary<ulong, Portfolio> Portfolios { get; set; } = new Dictionary<ulong, Portfolio>();

        public PortfolioService(IFinDataClient client)
        {
            this.client = client;
        }

        private async Task<ISecurityPrice> GetPriceAsync(string symbol, SecurityType securityType)
        {
            return securityType == SecurityType.Crypto ?
                await client.GetCryptoPriceAsync(symbol) :
                await client.GetPriceAsync(symbol);
        }

        private async Task<Portfolio> EnrichPortfolioAsync(Portfolio portfolio)
        {
            var secPrices = (await Task.WhenAll(portfolio.Positions.Select(m => GetPriceAsync(m.Symbol, m.SecurityType))))
                .ToDictionary(m => m.Symbol, m => m.Price);

            foreach (var position in portfolio.Positions)
            {
                position.LatestPrice = secPrices[position.Symbol] ?? 0;
            }

            return portfolio;
        }

        public async Task<Portfolio> GetPortfolioAsync(ulong userId)
        {
            if (!Portfolios.ContainsKey(userId))
            {
                Portfolios.Add(userId, new Portfolio() { UserId = userId, CashBalance = defaultBalance });
            }

            return await EnrichPortfolioAsync(Portfolios[userId]);
        }

        public async Task<ISecurityPrice> MarketBuy(ulong userId, Trade trade)
        {
            // Gonna stop shorts until I can figure out how to charge margin 
            // and stop infinite leverage.
            if (trade.Quantity <= 0) throw new Exception("Cannot complete short trades.");

            var executionPrice = await GetPriceAsync(trade.Symbol, trade.SecurityType);

            var portfolio = await GetPortfolioAsync(userId);

            lock (portfolio)
            {
                var tradePrice = trade.Quantity * executionPrice.Price;

                if (portfolio.CashBalance <= tradePrice)
                {
                    throw new Exception("Insufficient funds. AKA too poor");
                }

                portfolio.CashBalance -= tradePrice ?? 0;

                var position = portfolio.Positions
                    .FirstOrDefault(m => m.SecurityType == trade.SecurityType && m.Symbol == trade.Symbol);

                if (position == null)
                {
                    position = new Position() { Symbol = trade.Symbol, SecurityType = trade.SecurityType };
                    portfolio.Positions.Add(position);
                }

                var totalQuantity = position.Quantity + trade.Quantity;
                var oldWeightedPrice = position.Quantity / totalQuantity * position.AveragePrice;
                var newWeightedPrice = trade.Quantity / totalQuantity * executionPrice.Price ?? 0;

                position.Quantity += trade.Quantity;
                position.LatestPrice = executionPrice.Price ?? 0;
                position.AveragePrice = oldWeightedPrice + newWeightedPrice;
            }

            return executionPrice;
        }
    }
}
