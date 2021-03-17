using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace JumpenoWebassembly.Server.Migrations
{
    public partial class Statistics : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Statistics",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Date = table.Column<DateTime>(nullable: false),
                    GamesCount = table.Column<int>(nullable: false),
                    PlayersCount = table.Column<int>(nullable: false),
                    CPU = table.Column<float>(nullable: false),
                    Memory = table.Column<long>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Statistics", x => x.Id);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Statistics");
        }
    }
}
