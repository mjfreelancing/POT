using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Pot.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddDisabledColumnNullable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "Disabled",
                table: "Income",
                type: "boolean",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "Disabled",
                table: "Expense",
                type: "boolean",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "Disabled",
                table: "Account",
                type: "boolean",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Disabled",
                table: "Income");

            migrationBuilder.DropColumn(
                name: "Disabled",
                table: "Expense");

            migrationBuilder.DropColumn(
                name: "Disabled",
                table: "Account");
        }
    }
}
