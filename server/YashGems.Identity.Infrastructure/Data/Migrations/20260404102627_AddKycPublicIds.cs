using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace YashGems.Identity.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddKycPublicIds : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "IdCardBackPublicId",
                table: "Users",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "IdCardFrontPublicId",
                table: "Users",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IdCardBackPublicId",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "IdCardFrontPublicId",
                table: "Users");
        }
    }
}
