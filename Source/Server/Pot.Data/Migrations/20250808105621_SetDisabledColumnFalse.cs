using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Pot.Data.Migrations
{
    /// <inheritdoc />
    public partial class SetDisabledColumnFalse : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("UPDATE \"Account\" SET \"Disabled\" = false WHERE \"Disabled\" IS NULL");
            migrationBuilder.Sql("UPDATE \"Expense\" SET \"Disabled\" = false WHERE \"Disabled\" IS NULL");
            migrationBuilder.Sql("UPDATE \"Income\" SET \"Disabled\" = false WHERE \"Disabled\" IS NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
