using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Pot.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddExpenseNextDueIndex : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Expense_NextDue",
                table: "Expense",
                column: "NextDue");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Expense_NextDue",
                table: "Expense");
        }
    }
}
