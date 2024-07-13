using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddDateTimeProperty : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Transactions_CurrencyAccounts_FromAccountId",
                table: "Transactions");

            migrationBuilder.DropForeignKey(
                name: "FK_Transactions_CurrencyAccounts_ToAccountId",
                table: "Transactions");

            migrationBuilder.RenameColumn(
                name: "ToAccountId",
                table: "Transactions",
                newName: "ToAccountNumber");

            migrationBuilder.RenameColumn(
                name: "FromAccountId",
                table: "Transactions",
                newName: "FromAccountNumber");

            migrationBuilder.RenameIndex(
                name: "IX_Transactions_ToAccountId",
                table: "Transactions",
                newName: "IX_Transactions_ToAccountNumber");

            migrationBuilder.RenameIndex(
                name: "IX_Transactions_FromAccountId",
                table: "Transactions",
                newName: "IX_Transactions_FromAccountNumber");

            migrationBuilder.AddColumn<DateTime>(
                name: "DateTime",
                table: "Transactions",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "DateTimeOfOpen",
                table: "CurrencyAccounts",
                type: "datetime2",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AlterColumn<string>(
                name: "DefinedAccountNumbers",
                table: "AspNetUsers",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddForeignKey(
                name: "FK_Transactions_CurrencyAccounts_FromAccountNumber",
                table: "Transactions",
                column: "FromAccountNumber",
                principalTable: "CurrencyAccounts",
                principalColumn: "Number",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Transactions_CurrencyAccounts_ToAccountNumber",
                table: "Transactions",
                column: "ToAccountNumber",
                principalTable: "CurrencyAccounts",
                principalColumn: "Number");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Transactions_CurrencyAccounts_FromAccountNumber",
                table: "Transactions");

            migrationBuilder.DropForeignKey(
                name: "FK_Transactions_CurrencyAccounts_ToAccountNumber",
                table: "Transactions");

            migrationBuilder.DropColumn(
                name: "DateTime",
                table: "Transactions");

            migrationBuilder.DropColumn(
                name: "DateTimeOfOpen",
                table: "CurrencyAccounts");

            migrationBuilder.RenameColumn(
                name: "ToAccountNumber",
                table: "Transactions",
                newName: "ToAccountId");

            migrationBuilder.RenameColumn(
                name: "FromAccountNumber",
                table: "Transactions",
                newName: "FromAccountId");

            migrationBuilder.RenameIndex(
                name: "IX_Transactions_ToAccountNumber",
                table: "Transactions",
                newName: "IX_Transactions_ToAccountId");

            migrationBuilder.RenameIndex(
                name: "IX_Transactions_FromAccountNumber",
                table: "Transactions",
                newName: "IX_Transactions_FromAccountId");

            migrationBuilder.AlterColumn<string>(
                name: "DefinedAccountNumbers",
                table: "AspNetUsers",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Transactions_CurrencyAccounts_FromAccountId",
                table: "Transactions",
                column: "FromAccountId",
                principalTable: "CurrencyAccounts",
                principalColumn: "Number",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Transactions_CurrencyAccounts_ToAccountId",
                table: "Transactions",
                column: "ToAccountId",
                principalTable: "CurrencyAccounts",
                principalColumn: "Number");
        }
    }
}
