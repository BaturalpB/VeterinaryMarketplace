using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VeterinaryMarketplace.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddClinicApproval : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsApproved",
                table: "Clinic",
                type: "bit",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsApproved",
                table: "Clinic");
        }
    }
}
