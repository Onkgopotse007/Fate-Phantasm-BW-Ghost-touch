using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RPG_dotnet.Migrations
{
    /// <inheritdoc />
    public partial class AddGameLogicOverhaul : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "GameActionLogs",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    sessionId = table.Column<int>(type: "int", nullable: false),
                    actorCharacterId = table.Column<int>(type: "int", nullable: false),
                    targetCharacterId = table.Column<int>(type: "int", nullable: true),
                    actionType = table.Column<int>(type: "int", nullable: false),
                    abilityId = table.Column<int>(type: "int", nullable: true),
                    damageDealt = table.Column<int>(type: "int", nullable: false),
                    manaSpent = table.Column<double>(type: "float", nullable: false),
                    manaGained = table.Column<double>(type: "float", nullable: false),
                    newPosition = table.Column<float>(type: "real", nullable: true),
                    turnIndex = table.Column<int>(type: "int", nullable: false),
                    timestamp = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GameActionLogs", x => x.id);
                    table.ForeignKey(
                        name: "FK_GameActionLogs_GameSessions_sessionId",
                        column: x => x.sessionId,
                        principalTable: "GameSessions",
                        principalColumn: "gameSessionId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_GameActionLogs_sessionId",
                table: "GameActionLogs",
                column: "sessionId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "GameActionLogs");
        }
    }
}
