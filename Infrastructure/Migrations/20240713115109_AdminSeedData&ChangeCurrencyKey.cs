using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AdminSeedDataChangeCurrencyKey : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "AspNetRoles",
                columns: new[] { "Id", "ConcurrencyStamp", "Name", "NormalizedName" },
                values: new object[,]
                {
                    { new Guid("0d8ea822-1454-4853-9753-78fcdbd429d3"), "878eedaf-e795-411e-a0fc-847d0d4193dc", "Admin", "ADMIN" },
                    { new Guid("6c1fc012-261f-4e18-aab5-0b4a685b2860"), "7702fafb-3813-46db-9695-66a6e8fe9d41", "User", "USER" }
                });

            migrationBuilder.InsertData(
                table: "AspNetUsers",
                columns: new[] { "Id", "AccessFailedCount", "ConcurrencyStamp", "DefinedAccountNumbers", "Email", "EmailConfirmed", "LockoutEnabled", "LockoutEnd", "NormalizedEmail", "NormalizedUserName", "PasswordHash", "PersonName", "SecurityStamp", "TwoFactorEnabled", "UserName" },
                values: new object[] { new Guid("692de906-ff0c-4ecb-9b79-9d94fc72dead"), 0, "d594e4f9-c74e-43a4-90ac-f7fec50c15e1", "[]", "admin@gmail.com", true, false, null, "ADMIN@GMAIL.COM", "ADMIN@GMAIL.COM", "AQAAAAIAAYagAAAAEGaAzQdIX4ZNM+6vICn6ueOCEM88NDVHZS8YfX9ZgUi3X7yL5lHzbgLD1RCEnm3m/A==", "Admin Admini", null, false, "admin@gmail.com" });

            migrationBuilder.InsertData(
                table: "AspNetUserRoles",
                columns: new[] { "RoleId", "UserId" },
                values: new object[] { new Guid("0d8ea822-1454-4853-9753-78fcdbd429d3"), new Guid("692de906-ff0c-4ecb-9b79-9d94fc72dead") });

            migrationBuilder.CreateIndex(
                name: "IX_Currencies_CurrencyType",
                table: "Currencies",
                column: "CurrencyType",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Currencies_CurrencyType",
                table: "Currencies");

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: new Guid("6c1fc012-261f-4e18-aab5-0b4a685b2860"));

            migrationBuilder.DeleteData(
                table: "AspNetUserRoles",
                keyColumns: new[] { "RoleId", "UserId" },
                keyValues: new object[] { new Guid("0d8ea822-1454-4853-9753-78fcdbd429d3"), new Guid("692de906-ff0c-4ecb-9b79-9d94fc72dead") });

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: new Guid("0d8ea822-1454-4853-9753-78fcdbd429d3"));

            migrationBuilder.DeleteData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: new Guid("692de906-ff0c-4ecb-9b79-9d94fc72dead"));
        }
    }
}
