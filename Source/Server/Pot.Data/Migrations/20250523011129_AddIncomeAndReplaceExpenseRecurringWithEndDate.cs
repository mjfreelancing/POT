using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Pot.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddIncomeAndReplaceExpenseRecurringWithEndDate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Recurring",
                table: "Expense");

            migrationBuilder.AddColumn<DateOnly>(
                name: "EndDate",
                table: "Expense",
                type: "date",
                nullable: true);

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
                    AccountId = table.Column<int>(type: "integer", nullable: true),
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
                name: "Income");

            migrationBuilder.DropColumn(
                name: "EndDate",
                table: "Expense");

            migrationBuilder.AddColumn<bool>(
                name: "Recurring",
                table: "Expense",
                type: "boolean",
                nullable: false,
                defaultValue: false);
        }
    }
}
