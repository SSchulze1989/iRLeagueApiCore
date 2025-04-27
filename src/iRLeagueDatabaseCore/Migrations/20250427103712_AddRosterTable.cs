using System;
using Microsoft.EntityFrameworkCore.Migrations;
using MySql.EntityFrameworkCore.Metadata;

#nullable disable

namespace iRLeagueDatabaseCore.Migrations
{
    /// <inheritdoc />
    public partial class AddRosterTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Rosters",
                columns: table => new
                {
                    LeagueId = table.Column<long>(type: "bigint", nullable: false),
                    RosterId = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    Name = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: false),
                    Description = table.Column<string>(type: "text", nullable: true),
                    CreatedOn = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    LastModifiedOn = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    Version = table.Column<int>(type: "int", nullable: false),
                    CreatedByUserId = table.Column<string>(type: "longtext", nullable: true),
                    CreatedByUserName = table.Column<string>(type: "longtext", nullable: true),
                    LastModifiedByUserId = table.Column<string>(type: "longtext", nullable: true),
                    LastModifiedByUserName = table.Column<string>(type: "longtext", nullable: true),
                    IsArchived = table.Column<bool>(type: "tinyint(1)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Rosters", x => new { x.LeagueId, x.RosterId });
                    table.UniqueConstraint("AK_Rosters_RosterId", x => x.RosterId);
                    table.ForeignKey(
                        name: "FK_Rosters_Leagues_LeagueId",
                        column: x => x.LeagueId,
                        principalTable: "Leagues",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("Relational:Collation", "Latin1_General_CI_AS");

            migrationBuilder.CreateTable(
                name: "RosterEntries",
                columns: table => new
                {
                    LeagueId = table.Column<long>(type: "bigint", nullable: false),
                    RosterId = table.Column<long>(type: "bigint", nullable: false),
                    MemberId = table.Column<long>(type: "bigint", nullable: false),
                    TeamId = table.Column<long>(type: "bigint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RosterEntries", x => new { x.LeagueId, x.RosterId, x.MemberId });
                    table.ForeignKey(
                        name: "FK_RosterEntries_LeagueMembers_LeagueId_MemberId",
                        columns: x => new { x.LeagueId, x.MemberId },
                        principalTable: "LeagueMembers",
                        principalColumns: new[] { "LeagueId", "MemberId" },
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_RosterEntries_Rosters_LeagueId_RosterId",
                        columns: x => new { x.LeagueId, x.RosterId },
                        principalTable: "Rosters",
                        principalColumns: new[] { "LeagueId", "RosterId" },
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_RosterEntries_Teams_LeagueId_TeamId",
                        columns: x => new { x.LeagueId, x.TeamId },
                        principalTable: "Teams",
                        principalColumns: new[] { "LeagueId", "TeamId" });
                })
                .Annotation("Relational:Collation", "Latin1_General_CI_AS");

            migrationBuilder.CreateIndex(
                name: "IX_RosterEntries_LeagueId_MemberId",
                table: "RosterEntries",
                columns: new[] { "LeagueId", "MemberId" });

            migrationBuilder.CreateIndex(
                name: "IX_RosterEntries_LeagueId_TeamId",
                table: "RosterEntries",
                columns: new[] { "LeagueId", "TeamId" });

            migrationBuilder.CreateIndex(
                name: "IX_RosterEntries_RosterId_MemberId",
                table: "RosterEntries",
                columns: new[] { "RosterId", "MemberId" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RosterEntries");

            migrationBuilder.DropTable(
                name: "Rosters");
        }
    }
}
