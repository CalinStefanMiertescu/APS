using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace APS.Migrations
{
    /// <inheritdoc />
    public partial class AddAutoRenewToUser : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "AutoRenew",
                table: "AspNetUsers",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AutoRenew",
                table: "AspNetUsers");
        }
    }
}
