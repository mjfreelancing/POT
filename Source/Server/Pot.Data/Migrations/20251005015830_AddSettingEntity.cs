using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Pot.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddSettingEntity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Setting",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Category = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    Key = table.Column<string>(type: "text", nullable: false),
                    Value = table.Column<string>(type: "text", nullable: true),
                    Description = table.Column<string>(type: "citext", maxLength: 100, nullable: false),
                    SiteId = table.Column<int>(type: "integer", nullable: true),
                    RowId = table.Column<Guid>(type: "uuid", nullable: false),
                    Etag = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Setting", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Setting_Site_SiteId",
                        column: x => x.SiteId,
                        principalTable: "Site",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Setting_Etag",
                table: "Setting",
                column: "Etag");

            migrationBuilder.CreateIndex(
                name: "IX_Setting_RowId",
                table: "Setting",
                column: "RowId",
                unique: true);

            // migrationBuilder.CreateIndex(
            //     name: "IX_Setting_SiteId_Category_Key",
            //     table: "Setting",
            //     columns: new[] { "SiteId", "Category", "Key" },
            //     unique: true);

            // Create two separate unique constraints:
            // 1. For site-specific settings (where SiteId is not null)
            migrationBuilder.Sql(@"
                CREATE UNIQUE INDEX ""IX_Setting_SiteId_Category_Key""
                ON ""Setting"" (""SiteId"", ""Category"", ""Key"")
                WHERE ""SiteId"" IS NOT NULL;
            ");

            // 2. For global settings (where SiteId is null)
            migrationBuilder.Sql(@"
                CREATE UNIQUE INDEX ""IX_Setting_Global_Category_Key""
                ON ""Setting"" (""Category"", ""Key"")
                WHERE ""SiteId"" IS NULL;
            ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // First drop the custom indexes
            migrationBuilder.Sql("DROP INDEX IF EXISTS \"IX_Setting_SiteId_Category_Key\"");
            migrationBuilder.Sql("DROP INDEX IF EXISTS \"IX_Setting_Global_Category_Key\"");

            migrationBuilder.DropTable(
                name: "Setting");
        }
    }
}
