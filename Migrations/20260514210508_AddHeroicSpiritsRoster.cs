using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RPG_dotnet.Migrations
{
    /// <inheritdoc />
    public partial class AddHeroicSpiritsRoster : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "isStunned",
                table: "SessionCharacterStates",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "poisonDamagePerTick",
                table: "SessionCharacterStates",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "poisonStacks",
                table: "SessionCharacterStates",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "reviveCount",
                table: "SessionCharacterStates",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "description",
                table: "Characters",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<bool>(
                name: "isPlayable",
                table: "Characters",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "effect",
                table: "Abilities",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "effectValue",
                table: "Abilities",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "targetType",
                table: "Abilities",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "isStunned",
                table: "SessionCharacterStates");

            migrationBuilder.DropColumn(
                name: "poisonDamagePerTick",
                table: "SessionCharacterStates");

            migrationBuilder.DropColumn(
                name: "poisonStacks",
                table: "SessionCharacterStates");

            migrationBuilder.DropColumn(
                name: "reviveCount",
                table: "SessionCharacterStates");

            migrationBuilder.DropColumn(
                name: "description",
                table: "Characters");

            migrationBuilder.DropColumn(
                name: "isPlayable",
                table: "Characters");

            migrationBuilder.DropColumn(
                name: "effect",
                table: "Abilities");

            migrationBuilder.DropColumn(
                name: "effectValue",
                table: "Abilities");

            migrationBuilder.DropColumn(
                name: "targetType",
                table: "Abilities");
        }
    }
}
