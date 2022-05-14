namespace Finbot.Core.Portfolios;
using System;
using System.Linq;
using System.Threading.Tasks;
using Finbot.Core.IEX;
using Finbot.Core.IEX.Models;
using Finbot.Core.Models;
using Finbot.Data;
using Finbot.Data.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

public class PortfolioService : IPortfolioService
{
    private const decimal DefaultBalance = 100_000;

    private readonly IFinDataClient client;

    private readonly FinbotDataContext db;

    private readonly ILogger logger;

    public PortfolioService(IFinDataClient client, FinbotDataContext db, ILogger logger)
    {
        this.client = client;
        this.db = db;
        this.logger = logger;
    }

    private async Task<ISecurityPrice> GetPriceAsync(string symbol, SecurityType securityType)
    {
        try
        {
            return securityType == SecurityType.Crypto ?
                  await this.client.GetCryptoPriceAsync(symbol) :
                  await this.client.GetPriceAsync(symbol);
        }
        catch (ArgumentException)
        {
            logger?.LogWarning($"Price check for security {symbol}:{securityType} failed");

            return null;
        }
    }

    private async Task<Portfolio> EnrichPortfolioAsync(Portfolio portfolio)
    {
        var secPrices = (await Task.WhenAll(portfolio.Positions.Select(m => this.GetPriceAsync(m.Symbol, m.SecurityType))))
            .ToDictionary(m => m.Symbol.ToUpper(), m => m?.Price);

        foreach (var position in portfolio.Positions)
        {
            position.LatestPrice = secPrices[position.Symbol.ToUpper()] ?? position.LatestPrice;
        }

        return portfolio;
    }

    public async Task<Portfolio> GetPortfolioAsync(ulong userId)
    {
        var uid = userId.ToString();

        var portfolio = await this.db.Portfolios
            .AsQueryable()
            .Include(m => m.Positions)
            .FirstOrDefaultAsync(m => m.DiscordUserId == uid);

        if (portfolio == null)
        {
            portfolio = new Portfolio() { DiscordUserId = userId.ToString(), CashBalance = DefaultBalance };
            await this.db.Portfolios.AddAsync(portfolio);
        }


        await this.EnrichPortfolioAsync(portfolio);

        await this.db.SaveChangesAsync();

        return portfolio;
    }

    public async Task<ISecurityPrice> MarketBuy(ulong userId, Trade trade)
    {
        // Gonna stop shorts until I can figure out how to charge margin 
        // and stop infinite leverage.
        if (trade.Quantity <= 0)
        {
            throw new Exception("Cannot complete short trades.");
        }

        var executionPrice = await this.GetPriceAsync(trade.Symbol, trade.SecurityType);

        if (executionPrice == null)
        {
            throw new Exception("Cannot price that security. It either does not exist or has been delisted.");
        }

        var portfolio = await this.GetPortfolioAsync(userId);

        var tradePrice = trade.Quantity * executionPrice.Price;

        if (portfolio.CashBalance <= tradePrice)
        {
            throw new Exception("Insufficient funds. AKA too poor");
        }

        using (var transaction = await this.db.Database.BeginTransactionAsync())
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
                    await this.db.Positions.AddAsync(position);
                }

                position.Symbol = position.Symbol.ToUpper();

                var totalQuantity = position.Quantity + trade.Quantity ?? 0;
                var oldWeightedPrice = position.Quantity / totalQuantity * position.AveragePrice;
                var newWeightedPrice = trade.Quantity / totalQuantity * executionPrice.Price ?? 0;

                position.Quantity += trade.Quantity ?? 0;
                position.LatestPrice = executionPrice.Price ?? 0;
                position.AveragePrice = oldWeightedPrice + newWeightedPrice;

                await transaction.CommitAsync();

                await this.db.SaveChangesAsync();
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
        if (trade.Quantity is not null and <= 0)
        {
            throw new Exception("Invalid Quantity");
        }

        var executionPrice = await this.GetPriceAsync(trade.Symbol, trade.SecurityType);

        var portfolio = await this.GetPortfolioAsync(userId);

        var position = portfolio.Positions
            .FirstOrDefault(p => p.SecurityType == trade.SecurityType && p.Symbol == trade.Symbol);

        if (position == null)
        {
            throw new Exception($"You do not own any {trade.Symbol}");
        }

        var sellQuantity = trade.Quantity ?? position.Quantity;

        if (sellQuantity > position.Quantity)
        {
            throw new Exception($"You do not have {sellQuantity} of {trade.Symbol}. Maximum that can be sold is: {position.Quantity}");
        }

        using (var transaction = await this.db.Database.BeginTransactionAsync())
        {
            try
            {
                portfolio.CashBalance += sellQuantity * executionPrice.Price ?? position.LatestPrice;

                if (sellQuantity == position.Quantity)
                {
                    this.db.Remove(position);
                }
                else
                {
                    position.Quantity -= trade.Quantity ?? 0;
                }

                await transaction.CommitAsync();
                await this.db.SaveChangesAsync();
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
        var portfolio = await this.GetPortfolioAsync(userId);

        var positions = await this.db.Positions.AsQueryable().Where(m => m.PortfolioId == portfolio.PortfolioId).ToListAsync();

        if (!positions.Any())
        {
            throw new Exception("You have no open positions.");
        }

        var portfolioValue = positions.Sum(m => m.MarketValue);

        using (var transaction = await this.db.Database.BeginTransactionAsync())
        {
            try
            {
                portfolio.CashBalance += portfolioValue;

                this.db.RemoveRange(positions);

                await transaction.CommitAsync();
                await this.db.SaveChangesAsync();
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
        var portfolio = await this.db.Portfolios.AsQueryable().FirstAsync(m => m.DiscordUserId == userId.ToString());

        portfolio.CashBalance = newBalance;

        await this.db.SaveChangesAsync();
    }

    public async Task AddFunds(ulong userId, decimal amount)
    {
        var portfolio = await this.db.Portfolios.AsQueryable().FirstAsync(m => m.DiscordUserId == userId.ToString());

        portfolio.CashBalance += amount;

        await this.db.SaveChangesAsync();
    }
}
