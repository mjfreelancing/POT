using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Pot.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddSiteNameModifyDescription : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Site_Description",
                table: "Site");

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "Site",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "citext",
                oldMaxLength: 100);

            migrationBuilder.AddColumn<string>(
                name: "Name",
                table: "Site",
                type: "citext",
                maxLength: 50,
                nullable: true);

            migrationBuilder.Sql(@"
                UPDATE ""Site""
                SET ""Name"" = 'Site ' || ""Id"";
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Name",
                table: "Site");

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "Site",
                type: "citext",
                maxLength: 100,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "character varying(100)",
                oldMaxLength: 100,
                oldNullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Site_Description",
                table: "Site",
                column: "Description",
                unique: true);
        }
    }
}
