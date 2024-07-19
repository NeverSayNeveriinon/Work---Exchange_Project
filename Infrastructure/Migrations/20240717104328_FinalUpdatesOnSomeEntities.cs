using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class FinalUpdatesOnSomeEntities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ExchangeValues_FirstCurrencyId",
                table: "ExchangeValues");

            migrationBuilder.RenameColumn(
                name: "IsSuccess",
                table: "Transactions",
                newName: "IsConfirmed");

            migrationBuilder.AlterColumn<string>(
                name: "ToAccountNumber",
                table: "Transactions",
                type: "nvarchar(10)",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<string>(
                name: "FromAccountNumber",
                table: "Transactions",
                type: "nvarchar(10)",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<decimal>(
                name: "CRate",
                table: "Transactions",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<int>(
                name: "TransactionType",
                table: "Transactions",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AlterColumn<string>(
                name: "Number",
                table: "CurrencyAccounts",
                type: "nvarchar(10)",
                maxLength: 10,
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int")
                .OldAnnotation("SqlServer:Identity", "1, 1");

            migrationBuilder.AlterColumn<decimal>(
                name: "CRate",
                table: "CommissionRates",
                type: "decimal(6,3)",
                precision: 6,
                scale: 3,
                nullable: false,
                oldClrType: typeof(double),
                oldType: "float(6)",
                oldPrecision: 6,
                oldScale: 3);

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: new Guid("692de906-ff0c-4ecb-9b79-9d94fc72dead"),
                columns: new[] { "PasswordHash", "SecurityStamp" },
                values: new object[] { "AQAAAAIAAYagAAAAEOUwbW8VevRw2R0HvuQCHMYhyqnpK2ifPCQBZT/ENAl/Im16CMOXhFtu45Fs9kScFw==", "a05f9e4a-a0cb-483c-b242-285e0e8fa27d" });

            migrationBuilder.CreateIndex(
                name: "IX_ExchangeValues_FirstCurrencyId_SecondCurrencyId",
                table: "ExchangeValues",
                columns: new[] { "FirstCurrencyId", "SecondCurrencyId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CommissionRates_MaxUSDRange",
                table: "CommissionRates",
                column: "MaxUSDRange",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_ExchangeValues_FirstCurrencyId_SecondCurrencyId",
                table: "ExchangeValues");

            migrationBuilder.DropIndex(
                name: "IX_CommissionRates_MaxUSDRange",
                table: "CommissionRates");

            migrationBuilder.DropColumn(
                name: "CRate",
                table: "Transactions");

            migrationBuilder.DropColumn(
                name: "TransactionType",
                table: "Transactions");

            migrationBuilder.RenameColumn(
                name: "IsConfirmed",
                table: "Transactions",
                newName: "IsSuccess");

            migrationBuilder.AlterColumn<int>(
                name: "ToAccountNumber",
                table: "Transactions",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(string),
                oldType: "nvarchar(10)",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "FromAccountNumber",
                table: "Transactions",
                type: "int",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(10)");

            migrationBuilder.AlterColumn<int>(
                name: "Number",
                table: "CurrencyAccounts",
                type: "int",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(10)",
                oldMaxLength: 10)
                .Annotation("SqlServer:Identity", "1, 1");

            migrationBuilder.AlterColumn<double>(
                name: "CRate",
                table: "CommissionRates",
                type: "float(6)",
                precision: 6,
                scale: 3,
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(6,3)",
                oldPrecision: 6,
                oldScale: 3);

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: new Guid("692de906-ff0c-4ecb-9b79-9d94fc72dead"),
                columns: new[] { "PasswordHash", "SecurityStamp" },
                values: new object[] { "AQAAAAIAAYagAAAAEGaAzQdIX4ZNM+6vICn6ueOCEM88NDVHZS8YfX9ZgUi3X7yL5lHzbgLD1RCEnm3m/A==", null });

            migrationBuilder.CreateIndex(
                name: "IX_ExchangeValues_FirstCurrencyId",
                table: "ExchangeValues",
                column: "FirstCurrencyId");
        }
    }
}
