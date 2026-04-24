using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Pot.Data.Migrations
{
    /// <inheritdoc />
    public partial class RemoveExpenseAccruedTracking : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AccruedIsDirty",
                table: "Expense");

            migrationBuilder.DropColumn(
                name: "LastAccruedUpdate",
                table: "Expense");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "AccruedIsDirty",
                table: "Expense",
                type: "boolean",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<DateOnly>(
                name: "LastAccruedUpdate",
                table: "Expense",
                type: "date",
                nullable: true);
        }
    }
}
