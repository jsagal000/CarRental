using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CarRental.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddCriticalFieldsToMaintenanceRepairInsurance : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "CustomerCharge",
                table: "Repairs",
                type: "decimal(18, 2)",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "CustomerResponsible",
                table: "Repairs",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "InsurancePolicyId",
                table: "Repairs",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsCoveredByInsurance",
                table: "Repairs",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "Notes",
                table: "Repairs",
                type: "TEXT",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "OccurredDuringRental",
                table: "Repairs",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "RentalId",
                table: "Repairs",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Severity",
                table: "Repairs",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Status",
                table: "Repairs",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Type",
                table: "Repairs",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "Category",
                table: "Maintenances",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "InvoiceNumber",
                table: "Maintenances",
                type: "TEXT",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "NextMaintenanceDate",
                table: "Maintenances",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "NextMaintenanceMileage",
                table: "Maintenances",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Notes",
                table: "Maintenances",
                type: "TEXT",
                maxLength: 1000,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Type",
                table: "Maintenances",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "AgentName",
                table: "InsurancePolicies",
                type: "TEXT",
                maxLength: 100,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AgentPhone",
                table: "InsurancePolicies",
                type: "TEXT",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "AutoRenew",
                table: "InsurancePolicies",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<decimal>(
                name: "Deducible",
                table: "InsurancePolicies",
                type: "decimal(18, 2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "EmergencyPhone",
                table: "InsurancePolicies",
                type: "TEXT",
                maxLength: 20,
                nullable: true);

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 26, 3, 45, 37, 378, DateTimeKind.Utc).AddTicks(6979));

            migrationBuilder.CreateIndex(
                name: "IX_Repairs_InsurancePolicyId",
                table: "Repairs",
                column: "InsurancePolicyId");

            migrationBuilder.CreateIndex(
                name: "IX_Repairs_RentalId",
                table: "Repairs",
                column: "RentalId");

            migrationBuilder.AddForeignKey(
                name: "FK_Repairs_InsurancePolicies_InsurancePolicyId",
                table: "Repairs",
                column: "InsurancePolicyId",
                principalTable: "InsurancePolicies",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Repairs_Rentals_RentalId",
                table: "Repairs",
                column: "RentalId",
                principalTable: "Rentals",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Repairs_InsurancePolicies_InsurancePolicyId",
                table: "Repairs");

            migrationBuilder.DropForeignKey(
                name: "FK_Repairs_Rentals_RentalId",
                table: "Repairs");

            migrationBuilder.DropIndex(
                name: "IX_Repairs_InsurancePolicyId",
                table: "Repairs");

            migrationBuilder.DropIndex(
                name: "IX_Repairs_RentalId",
                table: "Repairs");

            migrationBuilder.DropColumn(
                name: "CustomerCharge",
                table: "Repairs");

            migrationBuilder.DropColumn(
                name: "CustomerResponsible",
                table: "Repairs");

            migrationBuilder.DropColumn(
                name: "InsurancePolicyId",
                table: "Repairs");

            migrationBuilder.DropColumn(
                name: "IsCoveredByInsurance",
                table: "Repairs");

            migrationBuilder.DropColumn(
                name: "Notes",
                table: "Repairs");

            migrationBuilder.DropColumn(
                name: "OccurredDuringRental",
                table: "Repairs");

            migrationBuilder.DropColumn(
                name: "RentalId",
                table: "Repairs");

            migrationBuilder.DropColumn(
                name: "Severity",
                table: "Repairs");

            migrationBuilder.DropColumn(
                name: "Status",
                table: "Repairs");

            migrationBuilder.DropColumn(
                name: "Type",
                table: "Repairs");

            migrationBuilder.DropColumn(
                name: "Category",
                table: "Maintenances");

            migrationBuilder.DropColumn(
                name: "InvoiceNumber",
                table: "Maintenances");

            migrationBuilder.DropColumn(
                name: "NextMaintenanceDate",
                table: "Maintenances");

            migrationBuilder.DropColumn(
                name: "NextMaintenanceMileage",
                table: "Maintenances");

            migrationBuilder.DropColumn(
                name: "Notes",
                table: "Maintenances");

            migrationBuilder.DropColumn(
                name: "Type",
                table: "Maintenances");

            migrationBuilder.DropColumn(
                name: "AgentName",
                table: "InsurancePolicies");

            migrationBuilder.DropColumn(
                name: "AgentPhone",
                table: "InsurancePolicies");

            migrationBuilder.DropColumn(
                name: "AutoRenew",
                table: "InsurancePolicies");

            migrationBuilder.DropColumn(
                name: "Deducible",
                table: "InsurancePolicies");

            migrationBuilder.DropColumn(
                name: "EmergencyPhone",
                table: "InsurancePolicies");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                column: "CreatedAt",
                value: new DateTime(2026, 3, 24, 0, 51, 40, 653, DateTimeKind.Utc).AddTicks(16));
        }
    }
}
