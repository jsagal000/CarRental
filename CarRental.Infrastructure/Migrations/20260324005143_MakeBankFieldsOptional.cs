using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CarRental.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class MakeBankFieldsOptional : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Bank",
                table: "Partners",
                type: "TEXT",
                maxLength: 100,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldMaxLength: 100);

            migrationBuilder.AlterColumn<string>(
                name: "AccountNumber",
                table: "Partners",
                type: "TEXT",
                maxLength: 30,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldMaxLength: 30);

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 24, 0, 51, 40, 653, DateTimeKind.Utc).AddTicks(16));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Bank",
                table: "Partners",
                type: "TEXT",
                maxLength: 100,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldMaxLength: 100,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "AccountNumber",
                table: "Partners",
                type: "TEXT",
                maxLength: 30,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "TEXT",
                oldMaxLength: 30,
                oldNullable: true);

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 24, 0, 49, 26, 767, DateTimeKind.Utc).AddTicks(3096));
        }
    }
}
