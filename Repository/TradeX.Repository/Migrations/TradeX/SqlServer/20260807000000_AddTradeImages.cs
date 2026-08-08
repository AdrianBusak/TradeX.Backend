using System;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using TradeX.Repository;

#nullable disable

namespace TradeX.Repository.Migrations.TradeX.SqlServer;

[DbContext(typeof(TradeXDbContext))]
[Migration("20260807000000_AddTradeImages")]
public partial class AddTradeImages : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "TradeImage",
            columns: table => new
            {
                Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                TradeId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                BlobPath = table.Column<string>(type: "nvarchar(1024)", maxLength: 1024, nullable: false),
                OriginalFileName = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                ContentType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                SizeBytes = table.Column<long>(type: "bigint", nullable: false),
                IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false, defaultValueSql: "GETUTCDATE()"),
                ModifiedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false, defaultValueSql: "GETUTCDATE()"),
                CreatedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                ModifiedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_TradeImage", x => x.Id);
                table.ForeignKey("FK_TradeImage_Trade_TradeId", x => x.TradeId, "Trade", "Id", onDelete: ReferentialAction.Restrict);
                table.ForeignKey("FK_TradeImage_User_UserId", x => x.UserId, "User", "Id", onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateIndex("IX_TradeImage_TradeId", "TradeImage", "TradeId");
        migrationBuilder.CreateIndex("IX_TradeImage_UserId", "TradeImage", "UserId");
        migrationBuilder.CreateIndex("IX_TradeImage_BlobPath", "TradeImage", "BlobPath", unique: true);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(name: "TradeImage");
    }
}
