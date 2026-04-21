using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Pot.Data.Migrations
{
    /// <inheritdoc />
    public partial class BackfillAccountAccrual : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                """
                INSERT INTO "AccountAccrual" ("AccountId", "AccruedIsDirty", "LastAccruedDate", "RowId", "Etag")
                SELECT account."Id", TRUE, NULL, gen_random_uuid(), 0
                FROM "Account" account
                WHERE NOT EXISTS
                (
                    SELECT 1
                    FROM "AccountAccrual" accountAccrual
                    WHERE accountAccrual."AccountId" = account."Id"
                );
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
