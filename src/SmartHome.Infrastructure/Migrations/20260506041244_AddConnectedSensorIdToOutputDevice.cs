using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartHome.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddConnectedSensorIdToOutputDevice : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "connected_sensor_id",
                table: "output_devices",
                type: "uuid",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "output_devices",
                keyColumn: "device_id",
                keyValue: new Guid("33333333-3333-3333-3333-333333333333"),
                column: "connected_sensor_id",
                value: null);

            migrationBuilder.CreateIndex(
                name: "IX_output_devices_connected_sensor_id",
                table: "output_devices",
                column: "connected_sensor_id");

            migrationBuilder.AddForeignKey(
                name: "FK_output_devices_sensors_connected_sensor_id",
                table: "output_devices",
                column: "connected_sensor_id",
                principalTable: "sensors",
                principalColumn: "device_id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_output_devices_sensors_connected_sensor_id",
                table: "output_devices");

            migrationBuilder.DropIndex(
                name: "IX_output_devices_connected_sensor_id",
                table: "output_devices");

            migrationBuilder.DropColumn(
                name: "connected_sensor_id",
                table: "output_devices");
        }
    }
}
