using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace iRLeagueDatabaseCore.Migrations
{
    /// <inheritdoc />
    public partial class UseRosterInChampSeason : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "RosterId",
                table: "ChampSeasons",
                type: "bigint",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_ChampSeasons_LeagueId_RosterId",
                table: "ChampSeasons",
                columns: new[] { "LeagueId", "RosterId" });

            migrationBuilder.AddForeignKey(
                name: "FK_ChampSeasons_Rosters_LeagueId_RosterId",
                table: "ChampSeasons",
                columns: new[] { "LeagueId", "RosterId" },
                principalTable: "Rosters",
                principalColumns: new[] { "LeagueId", "RosterId" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ChampSeasons_Rosters_LeagueId_RosterId",
                table: "ChampSeasons");

            migrationBuilder.DropIndex(
                name: "IX_ChampSeasons_LeagueId_RosterId",
                table: "ChampSeasons");

            migrationBuilder.DropColumn(
                name: "RosterId",
                table: "ChampSeasons");
        }
    }
}
