using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Pot.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddOneTimePassword : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "OneTimePassword",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CorrelationId = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Username = table.Column<string>(type: "citext", maxLength: 100, nullable: false),
                    Email = table.Column<string>(type: "citext", maxLength: 100, nullable: false),
                    Reason = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    RefCode = table.Column<string>(type: "character varying(6)", maxLength: 6, nullable: false),
                    OtpCode = table.Column<string>(type: "character varying(6)", maxLength: 6, nullable: false),
                    AttemptCount = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    Status = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    TempPasswordHash = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    CreatedUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ExpiryUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    VerifiedUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UserId = table.Column<int>(type: "integer", nullable: true),
                    RowId = table.Column<Guid>(type: "uuid", nullable: false),
                    Etag = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OneTimePassword", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OneTimePassword_User_UserId",
                        column: x => x.UserId,
                        principalTable: "User",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_OneTimePassword_Etag",
                table: "OneTimePassword",
                column: "Etag");

            migrationBuilder.CreateIndex(
                name: "IX_OneTimePassword_ExpiryUtc",
                table: "OneTimePassword",
                column: "ExpiryUtc");

            migrationBuilder.CreateIndex(
                name: "IX_OneTimePassword_Reason_Username_RefCode",
                table: "OneTimePassword",
                columns: new[] { "Reason", "Username", "RefCode" });

            migrationBuilder.CreateIndex(
                name: "IX_OneTimePassword_RowId",
                table: "OneTimePassword",
                column: "RowId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_OneTimePassword_Status_ExpiryUtc",
                table: "OneTimePassword",
                columns: new[] { "Status", "ExpiryUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_OneTimePassword_UserId",
                table: "OneTimePassword",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_OneTimePassword_Username_Status_CreatedUtc",
                table: "OneTimePassword",
                columns: new[] { "Username", "Status", "CreatedUtc" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "OneTimePassword");
        }
    }
}
