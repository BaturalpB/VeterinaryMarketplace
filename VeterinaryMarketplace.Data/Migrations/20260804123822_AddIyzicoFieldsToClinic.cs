using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VeterinaryMarketplace.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddIyzicoFieldsToClinic : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CompanyTitle",
                table: "Clinic",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Iban",
                table: "Clinic",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SubMerchantKey",
                table: "Clinic",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TaxNumber",
                table: "Clinic",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TaxOffice",
                table: "Clinic",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CompanyTitle",
                table: "Clinic");

            migrationBuilder.DropColumn(
                name: "Iban",
                table: "Clinic");

            migrationBuilder.DropColumn(
                name: "SubMerchantKey",
                table: "Clinic");

            migrationBuilder.DropColumn(
                name: "TaxNumber",
                table: "Clinic");

            migrationBuilder.DropColumn(
                name: "TaxOffice",
                table: "Clinic");
        }
    }
}
