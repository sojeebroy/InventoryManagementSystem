using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Inventory_Management_System.Migrations
{
    /// <inheritdoc />
    public partial class CustomIdAndFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Discussions_InventoryId_CreatedAt",
                table: "Discussions",
                columns: new[] { "InventoryId", "CreatedAt" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Discussions_InventoryId_CreatedAt",
                table: "Discussions");
        }
    }
}
