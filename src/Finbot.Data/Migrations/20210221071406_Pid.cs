using Microsoft.EntityFrameworkCore.Migrations;

namespace Finbot.Data.Migrations
{
    public partial class Pid : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Positions_Portfolios_PortfolioId",
                table: "Positions");

            migrationBuilder.DropColumn(
                name: "PorfolioId",
                table: "Positions");

            migrationBuilder.AlterColumn<int>(
                name: "PortfolioId",
                table: "Positions",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "INTEGER",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Positions_Portfolios_PortfolioId",
                table: "Positions",
                column: "PortfolioId",
                principalTable: "Portfolios",
                principalColumn: "PortfolioId",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Positions_Portfolios_PortfolioId",
                table: "Positions");

            migrationBuilder.AlterColumn<int>(
                name: "PortfolioId",
                table: "Positions",
                type: "INTEGER",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "INTEGER");

            migrationBuilder.AddColumn<int>(
                name: "PorfolioId",
                table: "Positions",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddForeignKey(
                name: "FK_Positions_Portfolios_PortfolioId",
                table: "Positions",
                column: "PortfolioId",
                principalTable: "Portfolios",
                principalColumn: "PortfolioId",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
