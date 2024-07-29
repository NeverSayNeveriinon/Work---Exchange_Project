using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class ChangeSomeDataTypes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_DefinedAccount_UserProfileId",
                table: "DefinedAccount");

            migrationBuilder.DropColumn(
                name: "IsConfirmed",
                table: "Transactions");

            migrationBuilder.DropColumn(
                name: "UnitOfSecondValue",
                table: "ExchangeValues");

            migrationBuilder.AlterColumn<string>(
                name: "ToAccountNumber",
                table: "Transactions",
                type: "varchar(10)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "FromAccountNumber",
                table: "Transactions",
                type: "varchar(10)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AlterColumn<decimal>(
                name: "CRate",
                table: "Transactions",
                type: "decimal(6,5)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(18,2)");

            migrationBuilder.AlterColumn<decimal>(
                name: "Amount",
                table: "Transactions",
                type: "decimal(20,9)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "money");

            migrationBuilder.AddColumn<decimal>(
                name: "FromAccountChangeAmount",
                table: "Transactions",
                type: "decimal(20,9)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "ToAccountChangeAmount",
                table: "Transactions",
                type: "decimal(20,9)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<int>(
                name: "TransactionStatus",
                table: "Transactions",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AlterColumn<decimal>(
                name: "UnitOfFirstValue",
                table: "ExchangeValues",
                type: "decimal(20,9)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "money");

            migrationBuilder.AlterColumn<string>(
                name: "CurrencyAccountNumber",
                table: "DefinedAccount",
                type: "varchar(10)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)",
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "Balance",
                table: "CurrencyAccounts",
                type: "decimal(20,9)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "money");

            migrationBuilder.AlterColumn<string>(
                name: "Number",
                table: "CurrencyAccounts",
                type: "varchar(10)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AddColumn<decimal>(
                name: "StashBalance",
                table: "CurrencyAccounts",
                type: "decimal(20,9)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AlterColumn<string>(
                name: "CurrencyType",
                table: "Currencies",
                type: "varchar(3)",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<decimal>(
                name: "MaxUSDRange",
                table: "CommissionRates",
                type: "decimal(20,9)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "money");

            migrationBuilder.AlterColumn<decimal>(
                name: "CRate",
                table: "CommissionRates",
                type: "decimal(6,5)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(6,3)",
                oldPrecision: 6,
                oldScale: 3);

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: new Guid("692de906-ff0c-4ecb-9b79-9d94fc72dead"),
                column: "PasswordHash",
                value: "AQAAAAIAAYagAAAAENacNd1mt+odfWMdoLe6HkyICHLuiJ4sGt54p0IX5bRVpEzWzwGLiLl1UtS9TjDxWA==");

            migrationBuilder.InsertData(
                table: "Currencies",
                columns: new[] { "Id", "CurrencyType" },
                values: new object[] { 1, "USD" });

            migrationBuilder.CreateIndex(
                name: "IX_DefinedAccount_UserProfileId_CurrencyAccountNumber",
                table: "DefinedAccount",
                columns: new[] { "UserProfileId", "CurrencyAccountNumber" },
                unique: true,
                filter: "[CurrencyAccountNumber] IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_DefinedAccount_UserProfileId_CurrencyAccountNumber",
                table: "DefinedAccount");

            migrationBuilder.DeleteData(
                table: "Currencies",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DropColumn(
                name: "FromAccountChangeAmount",
                table: "Transactions");

            migrationBuilder.DropColumn(
                name: "ToAccountChangeAmount",
                table: "Transactions");

            migrationBuilder.DropColumn(
                name: "TransactionStatus",
                table: "Transactions");

            migrationBuilder.DropColumn(
                name: "StashBalance",
                table: "CurrencyAccounts");

            migrationBuilder.AlterColumn<string>(
                name: "ToAccountNumber",
                table: "Transactions",
                type: "nvarchar(450)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "varchar(10)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "FromAccountNumber",
                table: "Transactions",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(10)");

            migrationBuilder.AlterColumn<decimal>(
                name: "CRate",
                table: "Transactions",
                type: "decimal(18,2)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(6,5)");

            migrationBuilder.AlterColumn<decimal>(
                name: "Amount",
                table: "Transactions",
                type: "money",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(20,9)");

            migrationBuilder.AddColumn<bool>(
                name: "IsConfirmed",
                table: "Transactions",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AlterColumn<decimal>(
                name: "UnitOfFirstValue",
                table: "ExchangeValues",
                type: "money",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(20,9)");

            migrationBuilder.AddColumn<decimal>(
                name: "UnitOfSecondValue",
                table: "ExchangeValues",
                type: "money",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AlterColumn<string>(
                name: "CurrencyAccountNumber",
                table: "DefinedAccount",
                type: "nvarchar(450)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "varchar(10)",
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "Balance",
                table: "CurrencyAccounts",
                type: "money",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(20,9)");

            migrationBuilder.AlterColumn<string>(
                name: "Number",
                table: "CurrencyAccounts",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(10)");

            migrationBuilder.AlterColumn<int>(
                name: "CurrencyType",
                table: "Currencies",
                type: "int",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "varchar(3)");

            migrationBuilder.AlterColumn<decimal>(
                name: "MaxUSDRange",
                table: "CommissionRates",
                type: "money",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(20,9)");

            migrationBuilder.AlterColumn<decimal>(
                name: "CRate",
                table: "CommissionRates",
                type: "decimal(6,3)",
                precision: 6,
                scale: 3,
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "decimal(6,5)");

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: new Guid("692de906-ff0c-4ecb-9b79-9d94fc72dead"),
                column: "PasswordHash",
                value: "AQAAAAIAAYagAAAAEEFvwG3CLR9Y0mH8v8nold6LNEibdnE6bIJ/8B5KsXJquN/xDxdQSdJ2yJy7jv8+lg==");

            migrationBuilder.CreateIndex(
                name: "IX_DefinedAccount_UserProfileId",
                table: "DefinedAccount",
                column: "UserProfileId");
        }
    }
}
