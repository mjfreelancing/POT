using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Pot.Data.Migrations
{
    /// <inheritdoc />
    public partial class Initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:PostgresExtension:citext", ",,");

            migrationBuilder.CreateTable(
                name: "Account",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Bsb = table.Column<string>(type: "character varying(7)", maxLength: 7, nullable: false),
                    Number = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Description = table.Column<string>(type: "citext", maxLength: 100, nullable: false),
                    Balance = table.Column<double>(type: "double precision", nullable: false),
                    Reserved = table.Column<double>(type: "double precision", nullable: false),
                    TotalExpenseAccrued = table.Column<double>(type: "double precision", nullable: false),
                    DailyExpenseAccrual = table.Column<double>(type: "double precision", nullable: false),
                    RowId = table.Column<Guid>(type: "uuid", nullable: false),
                    Etag = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Account", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Expense",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Description = table.Column<string>(type: "citext", maxLength: 100, nullable: false),
                    AccrualStart = table.Column<DateOnly>(type: "date", nullable: false),
                    NextDue = table.Column<DateOnly>(type: "date", nullable: false),
                    EndDate = table.Column<DateOnly>(type: "date", nullable: true),
                    Frequency = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    FrequencyCount = table.Column<int>(type: "integer", nullable: false),
                    Amount = table.Column<double>(type: "double precision", nullable: false),
                    Recurring = table.Column<bool>(type: "boolean", nullable: false),
                    Accrued = table.Column<double>(type: "double precision", nullable: false),
                    AccountId = table.Column<int>(type: "integer", nullable: false),
                    RowId = table.Column<Guid>(type: "uuid", nullable: false),
                    Etag = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Expense", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Expense_Account_AccountId",
                        column: x => x.AccountId,
                        principalTable: "Account",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Income",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Description = table.Column<string>(type: "citext", maxLength: 100, nullable: false),
                    NextDue = table.Column<DateOnly>(type: "date", nullable: false),
                    EndDate = table.Column<DateOnly>(type: "date", nullable: true),
                    Frequency = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: false),
                    FrequencyCount = table.Column<int>(type: "integer", nullable: false),
                    Amount = table.Column<double>(type: "double precision", nullable: false),
                    AccountId = table.Column<int>(type: "integer", nullable: false),
                    RowId = table.Column<Guid>(type: "uuid", nullable: false),
                    Etag = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Income", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Income_Account_AccountId",
                        column: x => x.AccountId,
                        principalTable: "Account",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Account_Bsb_Number",
                table: "Account",
                columns: new[] { "Bsb", "Number" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Account_Description",
                table: "Account",
                column: "Description",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Account_Etag",
                table: "Account",
                column: "Etag");

            migrationBuilder.CreateIndex(
                name: "IX_Account_RowId",
                table: "Account",
                column: "RowId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Expense_AccountId_Description",
                table: "Expense",
                columns: new[] { "AccountId", "Description" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Expense_Etag",
                table: "Expense",
                column: "Etag");

            migrationBuilder.CreateIndex(
                name: "IX_Expense_NextDue",
                table: "Expense",
                column: "NextDue");

            migrationBuilder.CreateIndex(
                name: "IX_Expense_RowId",
                table: "Expense",
                column: "RowId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Income_AccountId_Description",
                table: "Income",
                columns: new[] { "AccountId", "Description" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Income_Etag",
                table: "Income",
                column: "Etag");

            migrationBuilder.CreateIndex(
                name: "IX_Income_NextDue",
                table: "Income",
                column: "NextDue");

            migrationBuilder.CreateIndex(
                name: "IX_Income_RowId",
                table: "Income",
                column: "RowId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Expense");

            migrationBuilder.DropTable(
                name: "Income");

            migrationBuilder.DropTable(
                name: "Account");
        }
    }
}
