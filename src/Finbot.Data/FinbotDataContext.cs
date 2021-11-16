namespace Finbot.Data;
using Finbot.Data.Models;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

public class FinbotDataContext : DbContext
{
    private readonly string dbName;
    private readonly SqliteConnection connection;

    public DbSet<Position> Positions { get; set; }

    public DbSet<Portfolio> Portfolios { get; set; }

    public FinbotDataContext(string dbName) => this.dbName = dbName;

    public FinbotDataContext(SqliteConnection connection) => this.connection = connection;

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        if (this.connection != null)
        {
            optionsBuilder.UseSqlite(this.connection);
        }
        else
        {
            optionsBuilder.UseSqlite($"Data Source={this.dbName}");
        }
    }
}
