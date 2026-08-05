using System;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using TradeX.Repository;

#nullable disable

namespace TradeX.Repository.Migrations.TradeX.SqlServer;

[DbContext(typeof(TradeXDbContext))]
[Migration("20260801000000_AddTrades")]
public partial class AddTrades : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "TradingInstrument",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                Symbol = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                MarketType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false, defaultValueSql: "GETUTCDATE()"),
                ModifiedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false, defaultValueSql: "GETUTCDATE()"),
                CreatedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                ModifiedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_TradingInstrument", x => x.Id);
                table.ForeignKey("FK_TradingInstrument_User_UserId", x => x.UserId, "User", "Id", onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateTable(
            name: "Trade",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                StrategyId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                TradingInstrumentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                Direction = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                Session = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                Status = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                TradeDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                EntryPrice = table.Column<decimal>(type: "decimal(18,6)", precision: 18, scale: 6, nullable: true),
                ExitPrice = table.Column<decimal>(type: "decimal(18,6)", precision: 18, scale: 6, nullable: true),
                StopLoss = table.Column<decimal>(type: "decimal(18,6)", precision: 18, scale: 6, nullable: true),
                TakeProfit = table.Column<decimal>(type: "decimal(18,6)", precision: 18, scale: 6, nullable: true),
                LotSize = table.Column<decimal>(type: "decimal(18,6)", precision: 18, scale: 6, nullable: true),
                RiskAmount = table.Column<decimal>(type: "decimal(18,6)", precision: 18, scale: 6, nullable: true),
                PnL = table.Column<decimal>(type: "decimal(18,6)", precision: 18, scale: 6, nullable: true),
                RMultiple = table.Column<decimal>(type: "decimal(18,6)", precision: 18, scale: 6, nullable: true),
                Notes = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: true),
                IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false, defaultValueSql: "GETUTCDATE()"),
                ModifiedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false, defaultValueSql: "GETUTCDATE()"),
                CreatedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                ModifiedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_Trade", x => x.Id);
                table.ForeignKey("FK_Trade_Strategy_StrategyId", x => x.StrategyId, "Strategy", "Id", onDelete: ReferentialAction.Restrict);
                table.ForeignKey("FK_Trade_TradingInstrument_TradingInstrumentId", x => x.TradingInstrumentId, "TradingInstrument", "Id", onDelete: ReferentialAction.Restrict);
                table.ForeignKey("FK_Trade_User_UserId", x => x.UserId, "User", "Id", onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateTable(
            name: "TradeAccountAssignment",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                TradeId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                TradingAccountId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false, defaultValueSql: "GETUTCDATE()"),
                ModifiedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false, defaultValueSql: "GETUTCDATE()"),
                CreatedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                ModifiedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_TradeAccountAssignment", x => x.Id);
                table.ForeignKey("FK_TradeAccountAssignment_Trade_TradeId", x => x.TradeId, "Trade", "Id", onDelete: ReferentialAction.Restrict);
                table.ForeignKey("FK_TradeAccountAssignment_TradingAccount_TradingAccountId", x => x.TradingAccountId, "TradingAccount", "Id", onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateIndex("IX_TradingInstrument_UserId", "TradingInstrument", "UserId");
        migrationBuilder.CreateIndex("IX_TradingInstrument_UserId_Symbol", "TradingInstrument", new[] { "UserId", "Symbol" }, unique: true);
        migrationBuilder.CreateIndex("IX_Trade_UserId", "Trade", "UserId");
        migrationBuilder.CreateIndex("IX_Trade_StrategyId", "Trade", "StrategyId");
        migrationBuilder.CreateIndex("IX_Trade_TradingInstrumentId", "Trade", "TradingInstrumentId");
        migrationBuilder.CreateIndex("IX_Trade_TradeDate", "Trade", "TradeDate");
        migrationBuilder.CreateIndex("IX_TradeAccountAssignment_TradeId", "TradeAccountAssignment", "TradeId");
        migrationBuilder.CreateIndex("IX_TradeAccountAssignment_TradingAccountId", "TradeAccountAssignment", "TradingAccountId");
        migrationBuilder.CreateIndex("IX_TradeAccountAssignment_TradeId_TradingAccountId", "TradeAccountAssignment", new[] { "TradeId", "TradingAccountId" }, unique: true);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(name: "TradeAccountAssignment");
        migrationBuilder.DropTable(name: "Trade");
        migrationBuilder.DropTable(name: "TradingInstrument");
    }
}
