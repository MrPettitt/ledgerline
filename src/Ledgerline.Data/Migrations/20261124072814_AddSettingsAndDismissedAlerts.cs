using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Ledgerline.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddSettingsAndDismissedAlerts : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DismissedAlerts",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Kind = table.Column<string>(type: "TEXT", nullable: false),
                    AlertDate = table.Column<DateOnly>(type: "TEXT", nullable: false),
                    DismissedAt = table.Column<DateTimeOffset>(type: "TEXT", nullable: false),
                    SubscriptionId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DismissedAlerts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DismissedAlerts_Subscriptions_SubscriptionId",
                        column: x => x.SubscriptionId,
                        principalTable: "Subscriptions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Settings",
                columns: table => new
                {
                    Key = table.Column<string>(type: "TEXT", nullable: false),
                    Value = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Settings", x => x.Key);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DismissedAlerts_SubscriptionId_Kind_AlertDate",
                table: "DismissedAlerts",
                columns: new[] { "SubscriptionId", "Kind", "AlertDate" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DismissedAlerts");

            migrationBuilder.DropTable(
                name: "Settings");
        }
    }
}
