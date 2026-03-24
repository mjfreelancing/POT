using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Pot.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddStableExpenseAccrual : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<double>(
                name: "StableExpenseAccrual",
                table: "Account",
                type: "double precision",
                nullable: false,
                defaultValue: 0.0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "StableExpenseAccrual",
                table: "Account");
        }
    }
}
