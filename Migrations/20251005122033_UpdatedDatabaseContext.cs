using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IvyQrCodeProfileSharing.Migrations
{
    /// <inheritdoc />
    public partial class UpdatedDatabaseContext : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Profiles_Email",
                table: "Profiles",
                column: "Email",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Profiles_Email",
                table: "Profiles");
        }
    }
}
