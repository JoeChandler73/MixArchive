using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MixArchive.Data.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Mixes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Title = table.Column<string>(type: "TEXT", nullable: false),
                    Description = table.Column<string>(type: "TEXT", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    ModifiedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Mixes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "MixFiles",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    MixId = table.Column<int>(type: "INTEGER", nullable: false),
                    FileName = table.Column<string>(type: "TEXT", nullable: false),
                    FileSize = table.Column<long>(type: "INTEGER", nullable: false),
                    FileHash = table.Column<string>(type: "TEXT", nullable: false),
                    LastScanned = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MixFiles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MixFiles_Mixes_MixId",
                        column: x => x.MixId,
                        principalTable: "Mixes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_MixFiles_FileHash",
                table: "MixFiles",
                column: "FileHash");

            migrationBuilder.CreateIndex(
                name: "IX_MixFiles_FileName",
                table: "MixFiles",
                column: "FileName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MixFiles_MixId",
                table: "MixFiles",
                column: "MixId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "MixFiles");

            migrationBuilder.DropTable(
                name: "Mixes");
        }
    }
}
