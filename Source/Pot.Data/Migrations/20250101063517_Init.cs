using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Pot.Data.Migrations
{
    /// <inheritdoc />
    public partial class Init : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Account",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Bsb = table.Column<string>(type: "character varying(7)", maxLength: 7, nullable: false),
                    Number = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    Description = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Balance = table.Column<double>(type: "double precision", nullable: false),
                    Reserved = table.Column<double>(type: "double precision", nullable: false),
                    Allocated = table.Column<double>(type: "double precision", nullable: false),
                    DailyAccrual = table.Column<double>(type: "double precision", nullable: false),
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
                    Description = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    NextDue = table.Column<DateOnly>(type: "date", nullable: false),
                    AccrualStart = table.Column<DateOnly>(type: "date", nullable: false),
                    Frequency = table.Column<string>(type: "character varying(10)", maxLength: 10, nullable: true),
                    FrequencyCount = table.Column<int>(type: "integer", nullable: false),
                    Recurring = table.Column<bool>(type: "boolean", nullable: false),
                    Amount = table.Column<double>(type: "double precision", nullable: false),
                    Allocated = table.Column<double>(type: "double precision", nullable: false),
                    AccountId = table.Column<int>(type: "integer", nullable: false),
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

            migrationBuilder.CreateIndex(
                name: "IX_Account_Description",
                table: "Account",
                column: "Description",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Expense_AccountId",
                table: "Expense",
                column: "AccountId");

            migrationBuilder.CreateIndex(
                name: "IX_Expense_Description",
                table: "Expense",
                column: "Description",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Expense");

            migrationBuilder.DropTable(
                name: "Account");
        }
    }
}
