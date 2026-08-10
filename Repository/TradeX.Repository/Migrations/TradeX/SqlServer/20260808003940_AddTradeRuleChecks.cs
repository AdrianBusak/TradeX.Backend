using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TradeX.Repository.Migrations.TradeX.SqlServer;

public partial class AddTradeRuleChecks : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "TradeRuleChecks",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                TradeId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                StrategyRuleId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                IsFollowed = table.Column<bool>(type: "bit", nullable: false),
                Note = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false, defaultValueSql: "GETUTCDATE()"),
                ModifiedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false, defaultValueSql: "GETUTCDATE()"),
                CreatedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                ModifiedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_TradeRuleChecks", x => x.Id);
                table.ForeignKey(
                    name: "FK_TradeRuleChecks_StrategyRule_StrategyRuleId",
                    column: x => x.StrategyRuleId,
                    principalTable: "StrategyRule",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Restrict);
                table.ForeignKey(
                    name: "FK_TradeRuleChecks_Trade_TradeId",
                    column: x => x.TradeId,
                    principalTable: "Trade",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Restrict);
                table.ForeignKey(
                    name: "FK_TradeRuleChecks_User_UserId",
                    column: x => x.UserId,
                    principalTable: "User",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateIndex("IX_TradeRuleChecks_IsFollowed", "TradeRuleChecks", "IsFollowed");
        migrationBuilder.CreateIndex("IX_TradeRuleChecks_StrategyRuleId", "TradeRuleChecks", "StrategyRuleId");
        migrationBuilder.CreateIndex("IX_TradeRuleChecks_TradeId", "TradeRuleChecks", "TradeId");
        migrationBuilder.CreateIndex(
            "IX_TradeRuleChecks_TradeId_StrategyRuleId",
            "TradeRuleChecks",
            new[] { "TradeId", "StrategyRuleId" },
            unique: true);
        migrationBuilder.CreateIndex("IX_TradeRuleChecks_UserId", "TradeRuleChecks", "UserId");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(name: "TradeRuleChecks");
    }
}
