using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Pot.Data.Migrations
{
    /// <inheritdoc />
    public partial class RenamedDisabledToExcludeFromCalcs : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Disabled",
                table: "Income",
                newName: "ExcludeFromCalcs");

            migrationBuilder.RenameColumn(
                name: "Disabled",
                table: "Expense",
                newName: "ExcludeFromCalcs");

            migrationBuilder.RenameColumn(
                name: "Disabled",
                table: "Account",
                newName: "ExcludeFromCalcs");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "ExcludeFromCalcs",
                table: "Income",
                newName: "Disabled");

            migrationBuilder.RenameColumn(
                name: "ExcludeFromCalcs",
                table: "Expense",
                newName: "Disabled");

            migrationBuilder.RenameColumn(
                name: "ExcludeFromCalcs",
                table: "Account",
                newName: "Disabled");
        }
    }
}
