﻿// <auto-generated />
using Finbot.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Finbot.Data.Migrations
{
    [DbContext(typeof(FinbotDataContext))]
    partial class FinbotDataContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "5.0.3");

            modelBuilder.Entity("Finbot.Data.Models.Portfolio", b =>
                {
                    b.Property<int>("PortfolioId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<decimal>("CashBalance")
                        .HasColumnType("TEXT");

                    b.Property<string>("DiscordUserId")
                        .HasColumnType("TEXT");

                    b.HasKey("PortfolioId");

                    b.ToTable("Portfolios");
                });

            modelBuilder.Entity("Finbot.Data.Models.Position", b =>
                {
                    b.Property<int>("PositionId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("INTEGER");

                    b.Property<decimal>("AveragePrice")
                        .HasColumnType("TEXT");

                    b.Property<decimal>("LatestPrice")
                        .HasColumnType("TEXT");

                    b.Property<int>("PortfolioId")
                        .HasColumnType("INTEGER");

                    b.Property<decimal>("Quantity")
                        .HasColumnType("TEXT");

                    b.Property<int>("SecurityType")
                        .HasColumnType("INTEGER");

                    b.Property<string>("Symbol")
                        .HasColumnType("TEXT");

                    b.HasKey("PositionId");

                    b.HasIndex("PortfolioId");

                    b.ToTable("Positions");
                });

            modelBuilder.Entity("Finbot.Data.Models.Position", b =>
                {
                    b.HasOne("Finbot.Data.Models.Portfolio", "Portfolio")
                        .WithMany("Positions")
                        .HasForeignKey("PortfolioId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Portfolio");
                });

            modelBuilder.Entity("Finbot.Data.Models.Portfolio", b =>
                {
                    b.Navigation("Positions");
                });
#pragma warning restore 612, 618
        }
    }
}