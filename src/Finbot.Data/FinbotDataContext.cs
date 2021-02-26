using Finbot.Data.Models;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace Finbot.Data
{
    public class FinbotDataContext : DbContext
    {
        private readonly string dbName;
        private readonly SqliteConnection connection;

        public DbSet<Position> Positions { get; set; }

        public DbSet<Portfolio> Portfolios { get; set; }

        public FinbotDataContext(string dbName) 
        {
            this.dbName = dbName;
        }

        public FinbotDataContext(SqliteConnection connection)
        {
            this.connection = connection;
        }

        protected override void OnConfiguring(DbContextOptionsBuilder options)
        {
            if (connection != null) options.UseSqlite(connection);
            else options.UseSqlite($"Data Source={dbName}");
        }
    }
}
