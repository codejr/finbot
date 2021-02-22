using Finbot.Data.Models;
using Microsoft.EntityFrameworkCore;
using System;

namespace Finbot.Data
{
    public class FinbotDataContext : DbContext
    {
        public DbSet<Position> Positions { get; set; }

        public DbSet<Portfolio> Portfolios { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder options)
            => options.UseSqlite("Data Source=trading.db");
    }
}
