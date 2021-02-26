using AutoFixture.Xunit2;
using Finbot.Core.IEX;
using Finbot.Core.Portfolios;
using Finbot.Data;
using Finbot.Data.Models;
using Microsoft.Data.Sqlite;
using Moq;
using System.Threading.Tasks;
using Xunit;

namespace Finbot.Tests
{
    public class PortfolioServiceTests
    {
        private async Task<FinbotDataContext> CreateContext() 
        {
            var conn = new SqliteConnection("Data Source=:memory:");

            var context = new Data.FinbotDataContext(conn);

            await conn.OpenAsync();

            await context.Database.EnsureCreatedAsync();

            return context;
        }

        private async Task<PortfolioService> CreatePortfolioServiceAsync(FinbotDataContext ctx = null)
        {
            return new PortfolioService(null, ctx ?? await CreateContext());
        }

        [Theory, AutoData]
        public async Task GetPortfolio_NoneExists_CreatesNew(ulong userId)
        {
            var service = await CreatePortfolioServiceAsync();

            var portfolio = await service.GetPortfolioAsync(userId);

            Assert.Equal(userId.ToString(), portfolio.DiscordUserId);
        }

        [Theory, AutoData]
        public async Task GetPortfolio_Existing_GetExisting(ulong userId, Portfolio portfolio)
        {
            portfolio.DiscordUserId = userId.ToString();
            var db = await CreateContext();
            var service = await CreatePortfolioServiceAsync(db);
            await db.Portfolios.AddAsync(portfolio);
            await db.SaveChangesAsync();

            var result = await service.GetPortfolioAsync(ulong.Parse(portfolio.DiscordUserId));

            Assert.Equal(portfolio, result);
        }

        [Theory, AutoData]
        public async Task MarketBuy_NotOwned_ReturnsPrice(ulong userId, Mock<IFinDataClient> finClient)
        {
        }
    }
}
