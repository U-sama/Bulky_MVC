using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BulkyBook.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class FixColumnNameInProductImage : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ProductImages_Products_PriductId",
                table: "ProductImages");

            migrationBuilder.RenameColumn(
                name: "PriductId",
                table: "ProductImages",
                newName: "ProductId");

            migrationBuilder.RenameIndex(
                name: "IX_ProductImages_PriductId",
                table: "ProductImages",
                newName: "IX_ProductImages_ProductId");

            migrationBuilder.AddForeignKey(
                name: "FK_ProductImages_Products_ProductId",
                table: "ProductImages",
                column: "ProductId",
                principalTable: "Products",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ProductImages_Products_ProductId",
                table: "ProductImages");

            migrationBuilder.RenameColumn(
                name: "ProductId",
                table: "ProductImages",
                newName: "PriductId");

            migrationBuilder.RenameIndex(
                name: "IX_ProductImages_ProductId",
                table: "ProductImages",
                newName: "IX_ProductImages_PriductId");

            migrationBuilder.AddForeignKey(
                name: "FK_ProductImages_Products_PriductId",
                table: "ProductImages",
                column: "PriductId",
                principalTable: "Products",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
