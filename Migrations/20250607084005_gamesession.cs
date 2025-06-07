using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RPG_dotnet.Migrations
{
    /// <inheritdoc />
    public partial class gamesession : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "GameSessions",
                columns: table => new
                {
                    gameSessionId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    creatorUserId = table.Column<int>(type: "int", nullable: false),
                    opponentUserId = table.Column<int>(type: "int", nullable: false),
                    startedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    state = table.Column<int>(type: "int", nullable: false),
                    currentTurnIndex = table.Column<int>(type: "int", nullable: false),
                    winnerUserId = table.Column<int>(type: "int", nullable: true),
                    currentTurnPlayerId = table.Column<int>(type: "int", nullable: false),
                    minPosition = table.Column<float>(type: "real", nullable: false),
                    maxPosition = table.Column<float>(type: "real", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_GameSessions", x => x.gameSessionId);
                    table.ForeignKey(
                        name: "FK_GameSessions_Users_creatorUserId",
                        column: x => x.creatorUserId,
                        principalTable: "Users",
                        principalColumn: "id");
                    table.ForeignKey(
                        name: "FK_GameSessions_Users_opponentUserId",
                        column: x => x.opponentUserId,
                        principalTable: "Users",
                        principalColumn: "id");
                });

            migrationBuilder.CreateTable(
                name: "SessionCharacterStates",
                columns: table => new
                {
                    sessionCharacterId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    sessionId = table.Column<int>(type: "int", nullable: false),
                    characterId = table.Column<int>(type: "int", nullable: false),
                    userId = table.Column<int>(type: "int", nullable: false),
                    xPosition = table.Column<float>(type: "real", nullable: false),
                    currentHealth = table.Column<int>(type: "int", nullable: false),
                    currentMana = table.Column<double>(type: "float", nullable: false),
                    maxHealth = table.Column<int>(type: "int", nullable: false),
                    maxMana = table.Column<int>(type: "int", nullable: false),
                    isAlive = table.Column<bool>(type: "bit", nullable: false),
                    role = table.Column<int>(type: "int", nullable: false),
                    hasActedThisTurn = table.Column<bool>(type: "bit", nullable: false),
                    team = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SessionCharacterStates", x => x.sessionCharacterId);
                    table.ForeignKey(
                        name: "FK_SessionCharacterStates_Characters_characterId",
                        column: x => x.characterId,
                        principalTable: "Characters",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SessionCharacterStates_GameSessions_sessionId",
                        column: x => x.sessionId,
                        principalTable: "GameSessions",
                        principalColumn: "gameSessionId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SessionCharacterStates_Users_userId",
                        column: x => x.userId,
                        principalTable: "Users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_GameSessions_creatorUserId",
                table: "GameSessions",
                column: "creatorUserId");

            migrationBuilder.CreateIndex(
                name: "IX_GameSessions_opponentUserId",
                table: "GameSessions",
                column: "opponentUserId");

            migrationBuilder.CreateIndex(
                name: "IX_SessionCharacterStates_characterId",
                table: "SessionCharacterStates",
                column: "characterId");

            migrationBuilder.CreateIndex(
                name: "IX_SessionCharacterStates_sessionId",
                table: "SessionCharacterStates",
                column: "sessionId");

            migrationBuilder.CreateIndex(
                name: "IX_SessionCharacterStates_userId",
                table: "SessionCharacterStates",
                column: "userId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SessionCharacterStates");

            migrationBuilder.DropTable(
                name: "GameSessions");
        }
    }
}
