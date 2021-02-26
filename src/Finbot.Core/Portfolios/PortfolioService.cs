using Finbot.Core.IEX;
using Finbot.Core.IEX.Models;
using Finbot.Core.Models;
using Finbot.Data;
using Finbot.Data.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Finbot.Core.Portfolios
{
    public class PortfolioService : IPortfolioService
    {
        private const decimal defaultBalance = 100_000;

        private readonly IFinDataClient client;

        private readonly FinbotDataContext db;

        public PortfolioService(IFinDataClient client, FinbotDataContext db)
        {
            this.client = client;
            this.db = db;
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
                .ToDictionary(m => m.Symbol.ToUpper(), m => m.Price);

            foreach (var position in portfolio.Positions)
            {
                position.LatestPrice = secPrices[position.Symbol.ToUpper()] ?? 0;
            }

            return portfolio;
        }

        public async Task<Portfolio> GetPortfolioAsync(ulong userId)
        {
            var uid = userId.ToString();

            var portfolio = await db.Portfolios
                .AsQueryable()
                .Include(m => m.Positions)
                .FirstOrDefaultAsync(m => m.DiscordUserId == uid);

            if (portfolio == null)
            {
                portfolio = new Portfolio() { DiscordUserId = userId.ToString(), CashBalance = defaultBalance };
                await db.Portfolios.AddAsync(portfolio);
            }

            
            await EnrichPortfolioAsync(portfolio);

            await db.SaveChangesAsync();

            return portfolio;
        }

        public async Task<ISecurityPrice> MarketBuy(ulong userId, Trade trade)
        {
            // Gonna stop shorts until I can figure out how to charge margin 
            // and stop infinite leverage.
            if (trade.Quantity <= 0) throw new Exception("Cannot complete short trades.");

            var executionPrice = await GetPriceAsync(trade.Symbol, trade.SecurityType);

            var portfolio = await GetPortfolioAsync(userId);

            var tradePrice = trade.Quantity * executionPrice.Price;

            if (portfolio.CashBalance <= tradePrice)
            {
                throw new Exception("Insufficient funds. AKA too poor");
            }

            using (var transaction = await db.Database.BeginTransactionAsync())
            {
                try
                {
                    portfolio.CashBalance -= tradePrice ?? 0;

                    var position = portfolio.Positions
                        .FirstOrDefault(m => m.SecurityType == trade.SecurityType && m.Symbol == trade.Symbol);

                    if (position == null)
                    {
                        position = new Position() { Symbol = trade.Symbol.ToUpper(), SecurityType = trade.SecurityType };
                        portfolio.Positions.Add(position);
                        position.PortfolioId = portfolio.PortfolioId;
                        await db.Positions.AddAsync(position);
                    }

                    position.Symbol = position.Symbol.ToUpper();

                    var totalQuantity = position.Quantity + trade.Quantity ?? 0;
                    var oldWeightedPrice = position.Quantity / totalQuantity * position.AveragePrice;
                    var newWeightedPrice = trade.Quantity / totalQuantity * executionPrice.Price ?? 0;

                    position.Quantity += trade.Quantity ?? 0;
                    position.LatestPrice = executionPrice.Price ?? 0;
                    position.AveragePrice = oldWeightedPrice + newWeightedPrice;

                    await transaction.CommitAsync();

                    await db.SaveChangesAsync();
                }
                catch (Exception)
                {
                    await transaction.RollbackAsync();
                    throw;
                }
            }

            return executionPrice;
        }

        public async Task<ISecurityPrice> MarketSell(ulong userId, Trade trade)
        {
            if (trade.Quantity != null && trade.Quantity <= 0) throw new Exception("Invalid Quantity");

            var executionPrice = await GetPriceAsync(trade.Symbol, trade.SecurityType);

            var portfolio = await GetPortfolioAsync(userId);

            var position = portfolio.Positions
                .FirstOrDefault(p => p.SecurityType == trade.SecurityType && p.Symbol == trade.Symbol);

            if (position == null) throw new Exception($"You do not own any {trade.Symbol}");

            var sellQuantity = trade.Quantity ?? position.Quantity;

            if (sellQuantity > position.Quantity) throw new Exception($"You do not have {sellQuantity} of {trade.Symbol}. Maximum that can be sold is: {position.Quantity}");

            using(var transaction = await db.Database.BeginTransactionAsync())
            {
                try
                {
                    portfolio.CashBalance += sellQuantity* executionPrice.Price ?? 0;

                    if (sellQuantity == position.Quantity) 
                    {
                        db.Remove(position);
                    }
                    else
                    {
                        position.Quantity -= trade.Quantity ?? 0;
                    }

                    await transaction.CommitAsync();
                    await db.SaveChangesAsync();
                }
                catch (Exception)
                {
                    await transaction.RollbackAsync();
                    throw;
                }
            }

            return executionPrice;
        }

        public async Task<decimal> Liquidate(ulong userId) 
        {
            var portfolio = await GetPortfolioAsync(userId);

            var positions = await db.Positions.AsQueryable().Where(m => m.PortfolioId == portfolio.PortfolioId).ToListAsync();

            if (!positions.Any()) throw new Exception("You have no open positions.");

            var portfolioValue = positions.Sum(m => m.MarketValue);

            using(var transaction = await db.Database.BeginTransactionAsync()) 
            {
                try
                {
                    portfolio.CashBalance += portfolioValue;

                    db.RemoveRange(positions);

                    await transaction.CommitAsync();
                    await db.SaveChangesAsync();
                }
                catch (Exception) 
                {
                    await transaction.RollbackAsync();
                    throw;
                }
            }

            return portfolioValue;
        }

        public async Task SetBalance(ulong userId, decimal newBalance)
        {
            var portfolio = await db.Portfolios.AsQueryable().FirstAsync(m => m.DiscordUserId == userId.ToString());

            portfolio.CashBalance = newBalance;

            await db.SaveChangesAsync();
        }
    }
}
