using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Pot.Data.Migrations
{
    /// <inheritdoc />
    public partial class SetUserStatus : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                UPDATE "User"
                SET "Status" = 'Enabled'
                WHERE "Status" IS NULL;
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                UPDATE "User"
                SET "Status" = NULL;
                """);
        }
    }
}
