using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Rexplor.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddMarketerFieldsToDiscount : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsForMarketer",
                table: "Discounts",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "MarketerEmail",
                table: "Discounts",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MarketerName",
                table: "Discounts",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "SalesCount",
                table: "Discounts",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsForMarketer",
                table: "Discounts");

            migrationBuilder.DropColumn(
                name: "MarketerEmail",
                table: "Discounts");

            migrationBuilder.DropColumn(
                name: "MarketerName",
                table: "Discounts");

            migrationBuilder.DropColumn(
                name: "SalesCount",
                table: "Discounts");
        }
    }
}
