using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace iRLeagueDatabaseCore.Migrations
{
    /// <inheritdoc />
    public partial class AddStandingCountryCode : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CountryCode",
                table: "StandingRows",
                type: "longtext",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CountryCode",
                table: "StandingRows");
        }
    }
}
