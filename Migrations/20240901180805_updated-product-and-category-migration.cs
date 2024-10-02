using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace E_Commerce_Website.Migrations
{
    /// <inheritdoc />
    public partial class updatedproductandcategorymigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "cart_id",
                table: "tbl_product",
                newName: "cat_id");

            migrationBuilder.AddColumn<string>(
                name: "product_price",
                table: "tbl_product",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_tbl_product_cat_id",
                table: "tbl_product",
                column: "cat_id");

            migrationBuilder.AddForeignKey(
                name: "FK_tbl_product_tbl_category_cat_id",
                table: "tbl_product",
                column: "cat_id",
                principalTable: "tbl_category",
                principalColumn: "category_id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_tbl_product_tbl_category_cat_id",
                table: "tbl_product");

            migrationBuilder.DropIndex(
                name: "IX_tbl_product_cat_id",
                table: "tbl_product");

            migrationBuilder.DropColumn(
                name: "product_price",
                table: "tbl_product");

            migrationBuilder.RenameColumn(
                name: "cat_id",
                table: "tbl_product",
                newName: "cart_id");
        }
    }
}
