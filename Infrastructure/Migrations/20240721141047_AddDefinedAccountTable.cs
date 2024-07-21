using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddDefinedAccountTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DefinedAccountNumbers",
                table: "AspNetUsers");

            migrationBuilder.AlterColumn<string>(
                name: "ToAccountNumber",
                table: "Transactions",
                type: "nvarchar(450)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(10)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "FromAccountNumber",
                table: "Transactions",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(10)");

            migrationBuilder.AlterColumn<Guid>(
                name: "Id",
                table: "Transactions",
                type: "uniqueidentifier",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int")
                .OldAnnotation("SqlServer:Identity", "1, 1");

            migrationBuilder.AlterColumn<string>(
                name: "Number",
                table: "CurrencyAccounts",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(10)",
                oldMaxLength: 10);

            migrationBuilder.CreateTable(
                name: "DefinedAccount",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserProfileId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CurrencyAccountNumber = table.Column<string>(type: "nvarchar(450)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DefinedAccount", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DefinedAccount_AspNetUsers_UserProfileId",
                        column: x => x.UserProfileId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_DefinedAccount_CurrencyAccounts_CurrencyAccountNumber",
                        column: x => x.CurrencyAccountNumber,
                        principalTable: "CurrencyAccounts",
                        principalColumn: "Number",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "AspNetUserClaims",
                columns: new[] { "Id", "ClaimType", "ClaimValue", "UserId" },
                values: new object[] { 1, "http://schemas.microsoft.com/ws/2008/06/identity/claims/role", "Admin", new Guid("692de906-ff0c-4ecb-9b79-9d94fc72dead") });

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: new Guid("692de906-ff0c-4ecb-9b79-9d94fc72dead"),
                column: "PasswordHash",
                value: "AQAAAAIAAYagAAAAEEFvwG3CLR9Y0mH8v8nold6LNEibdnE6bIJ/8B5KsXJquN/xDxdQSdJ2yJy7jv8+lg==");

            migrationBuilder.CreateIndex(
                name: "IX_DefinedAccount_CurrencyAccountNumber",
                table: "DefinedAccount",
                column: "CurrencyAccountNumber");

            migrationBuilder.CreateIndex(
                name: "IX_DefinedAccount_UserProfileId",
                table: "DefinedAccount",
                column: "UserProfileId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DefinedAccount");

            migrationBuilder.DeleteData(
                table: "AspNetUserClaims",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.AlterColumn<string>(
                name: "ToAccountNumber",
                table: "Transactions",
                type: "nvarchar(10)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "FromAccountNumber",
                table: "Transactions",
                type: "nvarchar(10)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AlterColumn<int>(
                name: "Id",
                table: "Transactions",
                type: "int",
                nullable: false,
                oldClrType: typeof(Guid),
                oldType: "uniqueidentifier")
                .Annotation("SqlServer:Identity", "1, 1");

            migrationBuilder.AlterColumn<string>(
                name: "Number",
                table: "CurrencyAccounts",
                type: "nvarchar(10)",
                maxLength: 10,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AddColumn<string>(
                name: "DefinedAccountNumbers",
                table: "AspNetUsers",
                type: "nvarchar(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: new Guid("692de906-ff0c-4ecb-9b79-9d94fc72dead"),
                columns: new[] { "DefinedAccountNumbers", "PasswordHash" },
                values: new object[] { "[]", "AQAAAAIAAYagAAAAEKQ6kmK01k7iwH/Nv1FdXoChJL8KHekbtQTPuxaU2vhy4A86ypUMSimrJUzzNYkfQg==" });
        }
    }
}
