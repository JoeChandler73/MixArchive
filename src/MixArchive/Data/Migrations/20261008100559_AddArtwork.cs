using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MixArchive.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddArtwork : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ArtworkFileName",
                table: "Mixes",
                type: "TEXT",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ArtworkFileName",
                table: "Mixes");
        }
    }
}
