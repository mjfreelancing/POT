using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Pot.Data.Migrations
{
    /// <inheritdoc />
    public partial class DropAccountAccrualAndAggregates : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AccountAccrual");

            migrationBuilder.DropColumn(
                name: "Accrued",
                table: "Expense");

            migrationBuilder.DropColumn(
                name: "DailyExpenseAccrual",
                table: "Account");

            migrationBuilder.DropColumn(
                name: "StableExpenseAccrual",
                table: "Account");

            migrationBuilder.DropColumn(
                name: "TotalExpenseAccrued",
                table: "Account");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<double>(
                name: "Accrued",
                table: "Expense",
                type: "double precision",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "DailyExpenseAccrual",
                table: "Account",
                type: "double precision",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "StableExpenseAccrual",
                table: "Account",
                type: "double precision",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.AddColumn<double>(
                name: "TotalExpenseAccrued",
                table: "Account",
                type: "double precision",
                nullable: false,
                defaultValue: 0.0);

            migrationBuilder.CreateTable(
                name: "AccountAccrual",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    AccountId = table.Column<int>(type: "integer", nullable: false),
                    AccruedIsDirty = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    Etag = table.Column<long>(type: "bigint", nullable: false),
                    LastAccruedDate = table.Column<DateOnly>(type: "date", nullable: true),
                    RowId = table.Column<Guid>(type: "uuid", nullable: false)
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
    }
}
