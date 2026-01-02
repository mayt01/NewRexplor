using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Rexplor.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddAffiliateSystem : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "UsedDiscountCode",
                table: "Orders",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50);

            migrationBuilder.AddColumn<string>(
                name: "AffiliateCode",
                table: "Orders",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "AffiliateCommission",
                table: "Orders",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "AffiliateId",
                table: "Orders",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CommissionPaidDate",
                table: "Orders",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CommissionType",
                table: "Orders",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsCommissionPaid",
                table: "Orders",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<decimal>(
                name: "AffiliateCommissionPercent",
                table: "Discounts",
                type: "decimal(18,2)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "AffiliateId",
                table: "Discounts",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AffiliateRestrictions",
                table: "Discounts",
                type: "nvarchar(100)",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsAffiliateDiscount",
                table: "Discounts",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                name: "Affiliates",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FullName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PhoneNumber = table.Column<string>(type: "nvarchar(11)", maxLength: 11, nullable: false),
                    AffiliateCode = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    RegistrationDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    DiscountId = table.Column<int>(type: "int", nullable: true),
                    SuccessfulReferrals = table.Column<int>(type: "int", nullable: false),
                    TotalReferralAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    LastReferralDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    TotalCommission = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    CommissionPaid = table.Column<decimal>(type: "decimal(18,2)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Affiliates", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Affiliates_Discounts_DiscountId",
                        column: x => x.DiscountId,
                        principalTable: "Discounts",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "AffiliateReferrals",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AffiliateId = table.Column<int>(type: "int", nullable: false),
                    OrderId = table.Column<int>(type: "int", nullable: false),
                    DiscountCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    OrderAmount = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Commission = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    CommissionPercent = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    PurchaseDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsEmailSent = table.Column<bool>(type: "bit", nullable: false),
                    EmailSentDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsCommissionPaid = table.Column<bool>(type: "bit", nullable: false),
                    CommissionPaidDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AffiliateReferrals", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AffiliateReferrals_Affiliates_AffiliateId",
                        column: x => x.AffiliateId,
                        principalTable: "Affiliates",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AffiliateReferrals_Orders_OrderId",
                        column: x => x.OrderId,
                        principalTable: "Orders",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Orders_AffiliateId",
                table: "Orders",
                column: "AffiliateId");

            migrationBuilder.CreateIndex(
                name: "IX_Discounts_AffiliateId",
                table: "Discounts",
                column: "AffiliateId");

            migrationBuilder.CreateIndex(
                name: "IX_AffiliateReferrals_AffiliateId",
                table: "AffiliateReferrals",
                column: "AffiliateId");

            migrationBuilder.CreateIndex(
                name: "IX_AffiliateReferrals_OrderId",
                table: "AffiliateReferrals",
                column: "OrderId");

            migrationBuilder.CreateIndex(
                name: "IX_Affiliates_DiscountId",
                table: "Affiliates",
                column: "DiscountId");

            migrationBuilder.AddForeignKey(
                name: "FK_Discounts_Affiliates_AffiliateId",
                table: "Discounts",
                column: "AffiliateId",
                principalTable: "Affiliates",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Orders_Affiliates_AffiliateId",
                table: "Orders",
                column: "AffiliateId",
                principalTable: "Affiliates",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Discounts_Affiliates_AffiliateId",
                table: "Discounts");

            migrationBuilder.DropForeignKey(
                name: "FK_Orders_Affiliates_AffiliateId",
                table: "Orders");

            migrationBuilder.DropTable(
                name: "AffiliateReferrals");

            migrationBuilder.DropTable(
                name: "Affiliates");

            migrationBuilder.DropIndex(
                name: "IX_Orders_AffiliateId",
                table: "Orders");

            migrationBuilder.DropIndex(
                name: "IX_Discounts_AffiliateId",
                table: "Discounts");

            migrationBuilder.DropColumn(
                name: "AffiliateCode",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "AffiliateCommission",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "AffiliateId",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "CommissionPaidDate",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "CommissionType",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "IsCommissionPaid",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "AffiliateCommissionPercent",
                table: "Discounts");

            migrationBuilder.DropColumn(
                name: "AffiliateId",
                table: "Discounts");

            migrationBuilder.DropColumn(
                name: "AffiliateRestrictions",
                table: "Discounts");

            migrationBuilder.DropColumn(
                name: "IsAffiliateDiscount",
                table: "Discounts");

            migrationBuilder.AlterColumn<string>(
                name: "UsedDiscountCode",
                table: "Orders",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(50)",
                oldMaxLength: 50,
                oldNullable: true);

            migrationBuilder.AddColumn<decimal>(
                name: "DiscountAmount",
                table: "OrderItems",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);
        }
    }
}
