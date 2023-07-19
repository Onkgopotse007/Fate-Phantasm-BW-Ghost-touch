using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RPG_dotnet.Migrations
{
    /// <inheritdoc />
    public partial class userRoleColumn : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "userRole",
                table: "Users",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "userRole",
                table: "Users");
        }
    }
}
