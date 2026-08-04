using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace VeterinaryMarketplace.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddEmergencyClosed : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsEmergencyClosed",
                table: "VeterinarianDetails",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsEmergencyClosed",
                table: "VeterinarianDetails");
        }
    }
}
