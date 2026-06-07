using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace iRLeagueDatabaseCore.Migrations
{
    /// <inheritdoc />
    public partial class DriverPointsBalance : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DriverPointsBalances",
                columns: table => new
                {
                    LeagueId = table.Column<long>(type: "bigint", nullable: false),
                    AccountId = table.Column<long>(type: "bigint", nullable: false),
                    EntryKindId = table.Column<long>(type: "bigint", nullable: false),
                    Balance = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    EntryCount = table.Column<int>(type: "int", nullable: false),
                    LastUpdatedOn = table.Column<DateTime>(type: "datetime", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DriverPointsBalances", x => new { x.LeagueId, x.AccountId, x.EntryKindId });
                    table.ForeignKey(
                        name: "FK_DriverPointsBalances_DriverPointsAccounts_LeagueId_AccountId",
                        columns: x => new { x.LeagueId, x.AccountId },
                        principalTable: "DriverPointsAccounts",
                        principalColumns: new[] { "LeagueId", "AccountId" },
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DriverPointsBalances_DriverPointsEntryKinds_EntryKindId",
                        column: x => x.EntryKindId,
                        principalTable: "DriverPointsEntryKinds",
                        principalColumn: "EntryKindId",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("Relational:Collation", "Latin1_General_CI_AS");

            migrationBuilder.CreateIndex(
                name: "IX_DriverPointsBalances_EntryKindId",
                table: "DriverPointsBalances",
                column: "EntryKindId");

            migrationBuilder.CreateIndex(
                name: "IX_DriverPointsBalances_LeagueId_AccountId",
                table: "DriverPointsBalances",
                columns: new[] { "LeagueId", "AccountId" });

            migrationBuilder.CreateIndex(
                name: "IX_DriverPointsBalances_LeagueId_EntryKindId",
                table: "DriverPointsBalances",
                columns: new[] { "LeagueId", "EntryKindId" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DriverPointsBalances");
        }
    }
}
