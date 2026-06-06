using System;
using Microsoft.EntityFrameworkCore.Migrations;
using MySql.EntityFrameworkCore.Metadata;

#nullable disable

namespace iRLeagueDatabaseCore.Migrations
{
    /// <inheritdoc />
    public partial class DriverPointsAccount : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DriverPointsAccounts",
                columns: table => new
                {
                    LeagueId = table.Column<long>(type: "bigint", nullable: false),
                    AccountId = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    MemberId = table.Column<long>(type: "bigint", nullable: false),
                    Name = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: false),
                    Description = table.Column<string>(type: "varchar(2048)", maxLength: 2048, nullable: true),
                    CreatedOn = table.Column<DateTime>(type: "datetime", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DriverPointsAccounts", x => new { x.LeagueId, x.AccountId });
                    table.UniqueConstraint("AK_DriverPointsAccounts_AccountId", x => x.AccountId);
                })
                .Annotation("Relational:Collation", "Latin1_General_CI_AS");

            migrationBuilder.CreateTable(
                name: "DriverPointsEntryKinds",
                columns: table => new
                {
                    EntryKindId = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    LeagueId = table.Column<long>(type: "bigint", nullable: false),
                    Name = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: false),
                    Description = table.Column<string>(type: "varchar(2048)", maxLength: 2048, nullable: true),
                    EntryType = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DriverPointsEntryKinds", x => x.EntryKindId);
                    table.ForeignKey(
                        name: "FK_DriverPointsEntryKinds_Leagues_LeagueId",
                        column: x => x.LeagueId,
                        principalTable: "Leagues",
                        principalColumn: "Id");
                })
                .Annotation("Relational:Collation", "Latin1_General_CI_AS");

            migrationBuilder.CreateTable(
                name: "DriverPointsExpiryRules",
                columns: table => new
                {
                    LeagueId = table.Column<long>(type: "bigint", nullable: false),
                    ExpiryRuleId = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    PenaltyType = table.Column<int>(type: "int", nullable: true),
                    EntryKindId = table.Column<long>(type: "bigint", nullable: true),
                    IntervalDays = table.Column<int>(type: "int", nullable: true),
                    EventsDriven = table.Column<int>(type: "int", nullable: true),
                    EventsMissed = table.Column<int>(type: "int", nullable: true),
                    ResetOnNewSeason = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    Description = table.Column<string>(type: "varchar(2048)", maxLength: 2048, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DriverPointsExpiryRules", x => new { x.LeagueId, x.ExpiryRuleId });
                    table.UniqueConstraint("AK_DriverPointsExpiryRules_ExpiryRuleId", x => x.ExpiryRuleId);
                })
                .Annotation("Relational:Collation", "Latin1_General_CI_AS");

            migrationBuilder.CreateTable(
                name: "DriverPointsAccountSeasons",
                columns: table => new
                {
                    LeagueId = table.Column<long>(type: "bigint", nullable: false),
                    AccountId = table.Column<long>(type: "bigint", nullable: false),
                    SeasonId = table.Column<long>(type: "bigint", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DriverPointsAccountSeasons", x => new { x.LeagueId, x.AccountId, x.SeasonId });
                    table.ForeignKey(
                        name: "FK_DriverPointsAccountSeasons_DriverPointsAccounts_LeagueId_Acc~",
                        columns: x => new { x.LeagueId, x.AccountId },
                        principalTable: "DriverPointsAccounts",
                        principalColumns: new[] { "LeagueId", "AccountId" },
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DriverPointsAccountSeasons_Seasons_LeagueId_SeasonId",
                        columns: x => new { x.LeagueId, x.SeasonId },
                        principalTable: "Seasons",
                        principalColumns: new[] { "LeagueId", "SeasonId" });
                })
                .Annotation("Relational:Collation", "Latin1_General_CI_AS");

            migrationBuilder.CreateTable(
                name: "DriverPointsEntries",
                columns: table => new
                {
                    LeagueId = table.Column<long>(type: "bigint", nullable: false),
                    AccountEntryId = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    AccountId = table.Column<long>(type: "bigint", nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    CreatedById = table.Column<long>(type: "bigint", nullable: false),
                    Description = table.Column<string>(type: "varchar(2048)", maxLength: 2048, nullable: true),
                    Value = table.Column<string>(type: "json", nullable: true),
                    ArchivedOn = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    SourceRef = table.Column<string>(type: "longtext", nullable: true),
                    EntryKindId = table.Column<long>(type: "bigint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DriverPointsEntries", x => new { x.LeagueId, x.AccountEntryId });
                    table.UniqueConstraint("AK_DriverPointsEntries_AccountEntryId", x => x.AccountEntryId);
                    table.ForeignKey(
                        name: "FK_DriverPointsEntries_DriverPointsAccounts_LeagueId_AccountId",
                        columns: x => new { x.LeagueId, x.AccountId },
                        principalTable: "DriverPointsAccounts",
                        principalColumns: new[] { "LeagueId", "AccountId" },
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DriverPointsEntries_DriverPointsEntryKinds_EntryKindId",
                        column: x => x.EntryKindId,
                        principalTable: "DriverPointsEntryKinds",
                        principalColumn: "EntryKindId");
                })
                .Annotation("Relational:Collation", "Latin1_General_CI_AS");

            migrationBuilder.CreateIndex(
                name: "IX_DriverPointsAccounts_LeagueId_MemberId",
                table: "DriverPointsAccounts",
                columns: new[] { "LeagueId", "MemberId" });

            migrationBuilder.CreateIndex(
                name: "IX_DriverPointsAccountSeasons_LeagueId_SeasonId",
                table: "DriverPointsAccountSeasons",
                columns: new[] { "LeagueId", "SeasonId" });

            migrationBuilder.CreateIndex(
                name: "IX_DriverPointsEntries_EntryKindId",
                table: "DriverPointsEntries",
                column: "EntryKindId");

            migrationBuilder.CreateIndex(
                name: "IX_DriverPointsEntries_LeagueId_AccountId",
                table: "DriverPointsEntries",
                columns: new[] { "LeagueId", "AccountId" });

            migrationBuilder.CreateIndex(
                name: "IX_DriverPointsEntries_LeagueId_EntryKindId",
                table: "DriverPointsEntries",
                columns: new[] { "LeagueId", "EntryKindId" });

            migrationBuilder.CreateIndex(
                name: "IX_DriverPointsEntryKinds_LeagueId_EntryType",
                table: "DriverPointsEntryKinds",
                columns: new[] { "LeagueId", "EntryType" });

            migrationBuilder.CreateIndex(
                name: "IX_DriverPointsExpiryRules_LeagueId_PenaltyType",
                table: "DriverPointsExpiryRules",
                columns: new[] { "LeagueId", "PenaltyType" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DriverPointsAccountSeasons");

            migrationBuilder.DropTable(
                name: "DriverPointsEntries");

            migrationBuilder.DropTable(
                name: "DriverPointsExpiryRules");

            migrationBuilder.DropTable(
                name: "DriverPointsAccounts");

            migrationBuilder.DropTable(
                name: "DriverPointsEntryKinds");
        }
    }
}
