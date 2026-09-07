using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Pot.Data.Migrations
{
    /// <inheritdoc />
    public partial class MakeAccountDescriptionUniquePerSite : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Account_Description",
                table: "Account");

            migrationBuilder.DropIndex(
                name: "IX_Account_SiteId",
                table: "Account");

            migrationBuilder.CreateIndex(
                name: "IX_Account_SiteId_Description",
                table: "Account",
                columns: new[] { "SiteId", "Description" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Account_SiteId_Description",
                table: "Account");

            migrationBuilder.CreateIndex(
                name: "IX_Account_Description",
                table: "Account",
                column: "Description",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Account_SiteId",
                table: "Account",
                column: "SiteId");
        }
    }
}
