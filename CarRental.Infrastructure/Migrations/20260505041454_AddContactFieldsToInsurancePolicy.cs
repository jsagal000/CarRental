using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CarRental.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddContactFieldsToInsurancePolicy : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PolicyType",
                table: "InsurancePolicies");

            migrationBuilder.AddColumn<int>(
                name: "Status",
                table: "InsurancePolicies",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "TypePolicy",
                table: "InsurancePolicies",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2026, 5, 5, 4, 14, 51, 607, DateTimeKind.Utc).AddTicks(4444));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Status",
                table: "InsurancePolicies");

            migrationBuilder.DropColumn(
                name: "TypePolicy",
                table: "InsurancePolicies");

            migrationBuilder.AddColumn<string>(
                name: "PolicyType",
                table: "InsurancePolicies",
                type: "TEXT",
                maxLength: 100,
                nullable: false,
                defaultValue: "");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 26, 3, 45, 37, 378, DateTimeKind.Utc).AddTicks(6979));
        }
    }
}
