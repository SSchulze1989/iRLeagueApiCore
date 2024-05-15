using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace iRLeagueDatabaseCore.Migrations
{
    public partial class AddDropweekOverrides : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DropweekOverrides",
                columns: table => new
                {
                    LeagueId = table.Column<long>(type: "bigint", nullable: false),
                    StandingConfigId = table.Column<long>(type: "bigint", nullable: false),
                    ScoredResultRowId = table.Column<long>(type: "bigint", nullable: false),
                    Reason = table.Column<string>(type: "longtext", nullable: true),
                    ShouldDrop = table.Column<bool>(type: "tinyint(1)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DropweekOverrides", x => new { x.LeagueId, x.StandingConfigId, x.ScoredResultRowId });
                    table.ForeignKey(
                        name: "FK_DropweekOverrides_ScoredResultRows_LeagueId_ScoredResultRowId",
                        columns: x => new { x.LeagueId, x.ScoredResultRowId },
                        principalTable: "ScoredResultRows",
                        principalColumns: new[] { "LeagueId", "ScoredResultRowId" },
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DropweekOverrides_StandingConfigurations_LeagueId_StandingCo~",
                        columns: x => new { x.LeagueId, x.StandingConfigId },
                        principalTable: "StandingConfigurations",
                        principalColumns: new[] { "LeagueId", "StandingConfigId" },
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DropweekOverrides_LeagueId_ScoredResultRowId",
                table: "DropweekOverrides",
                columns: new[] { "LeagueId", "ScoredResultRowId" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DropweekOverrides");
        }
    }
}
