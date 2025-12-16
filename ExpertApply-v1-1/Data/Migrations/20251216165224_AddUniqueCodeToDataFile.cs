using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Rexplor.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddUniqueCodeToDataFile : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "UniqueCode",
                table: "DataFiles",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValueSql: "'DF' + LEFT(NEWID(), 8)");

            migrationBuilder.CreateIndex(
                name: "IX_DataFiles_UniqueCode",
                table: "DataFiles",
                column: "UniqueCode",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_DataFiles_UniqueCode",
                table: "DataFiles");

            migrationBuilder.DropColumn(
                name: "UniqueCode",
                table: "DataFiles");
        }
    }
}
