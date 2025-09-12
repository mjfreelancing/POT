using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Pot.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddAccountSiteRelationship : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "SiteId",
                table: "Account",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Account_SiteId",
                table: "Account",
                column: "SiteId");

            migrationBuilder.AddForeignKey(
                name: "FK_Account_Site_SiteId",
                table: "Account",
                column: "SiteId",
                principalTable: "Site",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Account_Site_SiteId",
                table: "Account");

            migrationBuilder.DropIndex(
                name: "IX_Account_SiteId",
                table: "Account");

            migrationBuilder.DropColumn(
                name: "SiteId",
                table: "Account");
        }
    }
}
