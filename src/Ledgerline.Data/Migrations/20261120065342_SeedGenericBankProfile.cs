using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Ledgerline.Data.Migrations
{
    /// <inheritdoc />
    public partial class SeedGenericBankProfile : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "BankProfiles",
                columns: new[] { "Id", "AmountColumn", "DateColumn", "DateFormat", "DebitsAreNegative", "DescriptionColumn", "Name" },
                values: new object[] { 1, "Amount", "Date", "yyyy-MM-dd", true, "Description", "Generic CSV" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "BankProfiles",
                keyColumn: "Id",
                keyValue: 1);
        }
    }
}
