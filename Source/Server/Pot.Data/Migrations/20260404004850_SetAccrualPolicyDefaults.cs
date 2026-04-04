using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Pot.Data.Migrations
{
    /// <inheritdoc />
    public partial class SetAccrualPolicyDefaults : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                                UPDATE "Expense"
                                SET "AccrualPolicy" = 'None'
                                WHERE "Frequency" = 'OneTime';
                                """);

            migrationBuilder.Sql("""
                                UPDATE "Expense"
                                SET "AccrualPolicy" = 'Automatic'
                                WHERE "AccrualPolicy" IS NULL;
                                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                UPDATE "Expense"
                SET "AccrualPolicy" = NULL;
                """);
        }
    }
}
