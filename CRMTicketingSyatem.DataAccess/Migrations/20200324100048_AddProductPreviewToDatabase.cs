using Microsoft.EntityFrameworkCore.Migrations;

namespace CRMTicketingSystem.DataAccess.Migrations
{
    public partial class AddProductPreviewToDatabase : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "PreviewUrl",
                table: "Products",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PreviewUrl",
                table: "Products");
        }
    }
}
