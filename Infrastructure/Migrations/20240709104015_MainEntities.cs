using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class MainEntities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PhoneNumber",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "PhoneNumberConfirmed",
                table: "AspNetUsers");

            migrationBuilder.AddColumn<string>(
                name: "DefinedAccountNumbers",
                table: "AspNetUsers",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateTable(
                name: "CommissionRates",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MaxUSDRange = table.Column<decimal>(type: "money", nullable: false),
                    CRate = table.Column<double>(type: "float(6)", precision: 6, scale: 3, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CommissionRates", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Currencies",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CurrencyType = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Currencies", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CurrencyAccounts",
                columns: table => new
                {
                    Number = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Balance = table.Column<decimal>(type: "money", nullable: false),
                    OwnerID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CurrencyID = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CurrencyAccounts", x => x.Number);
                    table.ForeignKey(
                        name: "FK_CurrencyAccounts_AspNetUsers_OwnerID",
                        column: x => x.OwnerID,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CurrencyAccounts_Currencies_CurrencyID",
                        column: x => x.CurrencyID,
                        principalTable: "Currencies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ExchangeValues",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    UnitOfFirstValue = table.Column<decimal>(type: "money", nullable: false),
                    UnitOfSecondValue = table.Column<decimal>(type: "money", nullable: false),
                    FirstCurrencyId = table.Column<int>(type: "int", nullable: false),
                    SecondCurrencyId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExchangeValues", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ExchangeValues_Currencies_FirstCurrencyId",
                        column: x => x.FirstCurrencyId,
                        principalTable: "Currencies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ExchangeValues_Currencies_SecondCurrencyId",
                        column: x => x.SecondCurrencyId,
                        principalTable: "Currencies",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Transactions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Amount = table.Column<decimal>(type: "money", nullable: false),
                    IsSuccess = table.Column<bool>(type: "bit", nullable: false),
                    FromAccountId = table.Column<int>(type: "int", nullable: false),
                    ToAccountId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Transactions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Transactions_CurrencyAccounts_FromAccountId",
                        column: x => x.FromAccountId,
                        principalTable: "CurrencyAccounts",
                        principalColumn: "Number",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Transactions_CurrencyAccounts_ToAccountId",
                        column: x => x.ToAccountId,
                        principalTable: "CurrencyAccounts",
                        principalColumn: "Number");
                });

            migrationBuilder.CreateIndex(
                name: "IX_CurrencyAccounts_CurrencyID",
                table: "CurrencyAccounts",
                column: "CurrencyID");

            migrationBuilder.CreateIndex(
                name: "IX_CurrencyAccounts_OwnerID",
                table: "CurrencyAccounts",
                column: "OwnerID");

            migrationBuilder.CreateIndex(
                name: "IX_ExchangeValues_FirstCurrencyId",
                table: "ExchangeValues",
                column: "FirstCurrencyId");

            migrationBuilder.CreateIndex(
                name: "IX_ExchangeValues_SecondCurrencyId",
                table: "ExchangeValues",
                column: "SecondCurrencyId");

            migrationBuilder.CreateIndex(
                name: "IX_Transactions_FromAccountId",
                table: "Transactions",
                column: "FromAccountId");

            migrationBuilder.CreateIndex(
                name: "IX_Transactions_ToAccountId",
                table: "Transactions",
                column: "ToAccountId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CommissionRates");

            migrationBuilder.DropTable(
                name: "ExchangeValues");

            migrationBuilder.DropTable(
                name: "Transactions");

            migrationBuilder.DropTable(
                name: "CurrencyAccounts");

            migrationBuilder.DropTable(
                name: "Currencies");

            migrationBuilder.DropColumn(
                name: "DefinedAccountNumbers",
                table: "AspNetUsers");

            migrationBuilder.AddColumn<string>(
                name: "PhoneNumber",
                table: "AspNetUsers",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "PhoneNumberConfirmed",
                table: "AspNetUsers",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }
    }
}
