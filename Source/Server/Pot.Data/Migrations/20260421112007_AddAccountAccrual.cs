using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Pot.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddAccountAccrual : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "AccountAccrual",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    AccountId = table.Column<int>(type: "integer", nullable: false),
                    AccruedIsDirty = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    LastAccruedDate = table.Column<DateOnly>(type: "date", nullable: true),
                    RowId = table.Column<Guid>(type: "uuid", nullable: false),
                    Etag = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AccountAccrual", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AccountAccrual_Account_AccountId",
                        column: x => x.AccountId,
                        principalTable: "Account",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AccountAccrual_AccountId",
                table: "AccountAccrual",
                column: "AccountId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AccountAccrual_AccruedIsDirty_LastAccruedDate",
                table: "AccountAccrual",
                columns: new[] { "AccruedIsDirty", "LastAccruedDate" });

            migrationBuilder.CreateIndex(
                name: "IX_AccountAccrual_Etag",
                table: "AccountAccrual",
                column: "Etag");

            migrationBuilder.CreateIndex(
                name: "IX_AccountAccrual_RowId",
                table: "AccountAccrual",
                column: "RowId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AccountAccrual");
        }
    }
}
