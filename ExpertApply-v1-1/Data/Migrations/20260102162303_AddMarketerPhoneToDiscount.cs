using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Rexplor.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddMarketerPhoneToDiscount : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "MarketerPhone",
                table: "Discounts",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "MarketerPhone",
                table: "Discounts");
        }
    }
}
