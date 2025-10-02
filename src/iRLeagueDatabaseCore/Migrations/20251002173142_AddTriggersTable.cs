using System;
using Microsoft.EntityFrameworkCore.Migrations;
using MySql.EntityFrameworkCore.Metadata;

#nullable disable

namespace iRLeagueDatabaseCore.Migrations
{
    /// <inheritdoc />
    public partial class AddTriggersTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Triggers",
                columns: table => new
                {
                    LeagueId = table.Column<long>(type: "bigint", nullable: false),
                    TriggerId = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("MySQL:ValueGenerationStrategy", MySQLValueGenerationStrategy.IdentityColumn),
                    Name = table.Column<string>(type: "longtext", nullable: true),
                    Description = table.Column<string>(type: "longtext", nullable: true),
                    TriggerType = table.Column<string>(type: "varchar(255)", nullable: false),
                    EventType = table.Column<int>(type: "int", nullable: true),
                    TimeElapses = table.Column<DateTimeOffset>(type: "datetime", nullable: true),
                    Interval = table.Column<long>(type: "bigint", nullable: true),
                    RefId1 = table.Column<long>(type: "bigint", nullable: true),
                    RefId2 = table.Column<long>(type: "bigint", nullable: true),
                    Action = table.Column<string>(type: "longtext", nullable: false),
                    ActionParameters = table.Column<string>(type: "longtext", nullable: true),
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
                    table.PrimaryKey("PK_Triggers", x => new { x.LeagueId, x.TriggerId });
                    table.UniqueConstraint("AK_Triggers_TriggerId", x => x.TriggerId);
                    table.ForeignKey(
                        name: "FK_Triggers_Leagues_LeagueId",
                        column: x => x.LeagueId,
                        principalTable: "Leagues",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("Relational:Collation", "Latin1_General_CI_AS");

            migrationBuilder.CreateIndex(
                name: "IX_Triggers_RefId1",
                table: "Triggers",
                column: "RefId1");

            migrationBuilder.CreateIndex(
                name: "IX_Triggers_RefId2",
                table: "Triggers",
                column: "RefId2");

            migrationBuilder.CreateIndex(
                name: "IX_Triggers_TriggerType",
                table: "Triggers",
                column: "TriggerType");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Triggers");
        }
    }
}
