using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace GradeProgressMonitoring.Migrations
{
    /// <inheritdoc />
    public partial class AddCheckerLockApproveFieldsToComponentItem : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // ✅ Only add columns to existing table ComponentItems
            migrationBuilder.AddColumn<bool>(
                name: "IsLocked",
                table: "ComponentItems",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsApproved",
                table: "ComponentItems",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "ApprovedAt",
                table: "ComponentItems",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ApprovedByUserId",
                table: "ComponentItems",
                type: "TEXT",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {

            migrationBuilder.DropColumn(
                name: "IsLocked",
                table: "ComponentItems");

            migrationBuilder.DropColumn(
                name: "IsApproved",
                table: "ComponentItems");

            migrationBuilder.DropColumn(
                name: "ApprovedAt",
                table: "ComponentItems");

            migrationBuilder.DropColumn(
                name: "ApprovedByUserId",
                table: "ComponentItems");
        }
    }
}
