using Microsoft.EntityFrameworkCore.Migrations;

namespace JumpenoWebassembly.Server.Migrations
{
    public partial class AddedSkinIdToUser : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "SkinId",
                table: "Users",
                nullable: false,
                defaultValue: 0);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SkinId",
                table: "Users");
        }
    }
}
