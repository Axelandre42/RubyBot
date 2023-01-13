using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RubyBot.Migrations
{
    /// <inheritdoc />
    public partial class Initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Personas",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", nullable: false),
                    Alias = table.Column<string>(type: "TEXT", nullable: false),
                    Locked = table.Column<bool>(type: "INTEGER", nullable: false),
                    Precision = table.Column<byte>(type: "INTEGER", nullable: false),
                    Strength = table.Column<byte>(type: "INTEGER", nullable: false),
                    Agility = table.Column<byte>(type: "INTEGER", nullable: false),
                    Parade = table.Column<byte>(type: "INTEGER", nullable: false),
                    Resistance = table.Column<byte>(type: "INTEGER", nullable: false),
                    PlayerId = table.Column<ulong>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Personas", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Players",
                columns: table => new
                {
                    Id = table.Column<ulong>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Seed = table.Column<int>(type: "INTEGER", nullable: false),
                    LastUsedId = table.Column<int>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Players", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Players_Personas_LastUsedId",
                        column: x => x.LastUsedId,
                        principalTable: "Personas",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_Personas_PlayerId",
                table: "Personas",
                column: "PlayerId");

            migrationBuilder.CreateIndex(
                name: "IX_Players_LastUsedId",
                table: "Players",
                column: "LastUsedId",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Personas_Players_PlayerId",
                table: "Personas",
                column: "PlayerId",
                principalTable: "Players",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Personas_Players_PlayerId",
                table: "Personas");

            migrationBuilder.DropTable(
                name: "Players");

            migrationBuilder.DropTable(
                name: "Personas");
        }
    }
}
