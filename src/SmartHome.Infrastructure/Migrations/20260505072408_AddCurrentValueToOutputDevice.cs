using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartHome.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddCurrentValueToOutputDevice : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "current_value",
                table: "output_devices",
                type: "numeric",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "output_devices",
                keyColumn: "device_id",
                keyValue: new Guid("33333333-3333-3333-3333-333333333333"),
                column: "current_value",
                value: 0m);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "current_value",
                table: "output_devices");
        }
    }
}
