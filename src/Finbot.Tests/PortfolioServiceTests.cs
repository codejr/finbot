namespace Finbot.Tests;
using System.Threading.Tasks;
using AutoFixture.Xunit2;
using Finbot.Core.IEX;
using Finbot.Core.Portfolios;
using Finbot.Data;
using Finbot.Data.Models;
using Microsoft.Data.Sqlite;
using Moq;
using Xunit;

public class PortfolioServiceTests
{
    private static async Task<FinbotDataContext> CreateContext()
    {
        var conn = new SqliteConnection("Data Source=:memory:");

        var context = new FinbotDataContext(conn);

        await conn.OpenAsync();

        await context.Database.EnsureCreatedAsync();

        return context;
    }

    private async Task<PortfolioService> CreatePortfolioServiceAsync(FinbotDataContext ctx = null) => new PortfolioService(null, ctx ?? await CreateContext());

    [Theory, AutoData]
    public async Task GetPortfolioNoneExistsCreatesNew(ulong userId)
    {
        var service = await this.CreatePortfolioServiceAsync();

        var portfolio = await service.GetPortfolioAsync(userId);

        Assert.Equal(userId.ToString(), portfolio.DiscordUserId);
    }

    [Theory, AutoData]
    public async Task GetPortfolioExistingGetExisting(ulong userId, Portfolio portfolio)
    {
        portfolio.DiscordUserId = userId.ToString();
        var db = await CreateContext();
        var service = await this.CreatePortfolioServiceAsync(db);
        await db.Portfolios.AddAsync(portfolio);
        await db.SaveChangesAsync();

        var result = await service.GetPortfolioAsync(ulong.Parse(portfolio.DiscordUserId));

        Assert.Equal(portfolio, result);
    }

    [Theory, AutoData]
    public Task MarketBuyNotOwnedReturnsPrice(Mock<IFinDataClient> finClient) => Task.CompletedTask;
}
