using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Pot.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddExpenseAndIncomeNote : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Note",
                table: "Income",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Note",
                table: "Expense",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Note",
                table: "Income");

            migrationBuilder.DropColumn(
                name: "Note",
                table: "Expense");
        }
    }
}
