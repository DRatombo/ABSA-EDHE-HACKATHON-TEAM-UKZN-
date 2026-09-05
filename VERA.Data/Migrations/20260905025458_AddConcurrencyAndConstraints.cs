using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VERA.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddConcurrencyAndConstraints : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                table: "Opportunities",
                type: "rowversion",
                rowVersion: true,
                nullable: false,
                defaultValue: new byte[0]);

            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                table: "FundingOffers",
                type: "rowversion",
                rowVersion: true,
                nullable: false,
                defaultValue: new byte[0]);

            migrationBuilder.AddColumn<byte[]>(
                name: "RowVersion",
                table: "FulfilmentRecords",
                type: "rowversion",
                rowVersion: true,
                nullable: false,
                defaultValue: new byte[0]);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "RowVersion",
                table: "Opportunities");

            migrationBuilder.DropColumn(
                name: "RowVersion",
                table: "FundingOffers");

            migrationBuilder.DropColumn(
                name: "RowVersion",
                table: "FulfilmentRecords");
        }
    }
}
