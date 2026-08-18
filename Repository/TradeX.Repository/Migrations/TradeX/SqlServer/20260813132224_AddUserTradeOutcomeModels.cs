using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TradeX.Repository.Migrations.TradeX.SqlServer
{
    /// <inheritdoc />
    public partial class AddUserTradeOutcomeModels : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "UserTradeOutcomeModels",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ModelVersion = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    ModelPath = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: false),
                    SampleCount = table.Column<int>(type: "int", nullable: false),
                    PositiveCount = table.Column<int>(type: "int", nullable: false),
                    NonPositiveCount = table.Column<int>(type: "int", nullable: false),
                    TrainedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    FeatureSchemaVersion = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    IsActiveModel = table.Column<bool>(type: "bit", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    ModifiedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false, defaultValueSql: "GETUTCDATE()"),
                    CreatedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ModifiedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserTradeOutcomeModels", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserTradeOutcomeModels_User_UserId",
                        column: x => x.UserId,
                        principalTable: "User",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_UserTradeOutcomeModels_UserId_IsActiveModel",
                table: "UserTradeOutcomeModels",
                columns: new[] { "UserId", "IsActiveModel" });

            migrationBuilder.CreateIndex(
                name: "IX_UserTradeOutcomeModels_UserId_ModelVersion",
                table: "UserTradeOutcomeModels",
                columns: new[] { "UserId", "ModelVersion" });

        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "UserTradeOutcomeModels");
        }
    }
}
