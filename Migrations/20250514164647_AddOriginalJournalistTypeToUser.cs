using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace APS.Migrations
{
    /// <inheritdoc />
    public partial class AddOriginalJournalistTypeToUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "OriginalJournalistType",
                table: "AspNetUsers",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "OriginalJournalistType",
                table: "AspNetUsers");
        }
    }
}
