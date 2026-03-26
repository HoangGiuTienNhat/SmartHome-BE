using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartHome.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class SeedData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "users",
                columns: new[] { "user_id", "email", "full_name", "password" },
                values: new object[] { new Guid("11111111-1111-1111-1111-111111111111"), "admin@gmail.com", "Admin", "123456" });

            migrationBuilder.InsertData(
                table: "rooms",
                columns: new[] { "room_id", "name", "ruser_id" },
                values: new object[] { new Guid("22222222-2222-2222-2222-222222222222"), "Living Room", new Guid("11111111-1111-1111-1111-111111111111") });

            migrationBuilder.InsertData(
                table: "devices",
                columns: new[] { "device_id", "droom_id", "feed_key", "install_date", "name", "state", "type", "update_date" },
                values: new object[] { new Guid("33333333-3333-3333-3333-333333333333"), new Guid("22222222-2222-2222-2222-222222222222"), "light_1", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc), "Main Light", "OFF", "OUTPUT", new DateTime(2024, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc) });

            migrationBuilder.InsertData(
                table: "output_devices",
                columns: new[] { "device_id", "auto", "onoff_state" },
                values: new object[] { new Guid("33333333-3333-3333-3333-333333333333"), false, "OFF" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "output_devices",
                keyColumn: "device_id",
                keyValue: new Guid("33333333-3333-3333-3333-333333333333"));

            migrationBuilder.DeleteData(
                table: "devices",
                keyColumn: "device_id",
                keyValue: new Guid("33333333-3333-3333-3333-333333333333"));

            migrationBuilder.DeleteData(
                table: "rooms",
                keyColumn: "room_id",
                keyValue: new Guid("22222222-2222-2222-2222-222222222222"));

            migrationBuilder.DeleteData(
                table: "users",
                keyColumn: "user_id",
                keyValue: new Guid("11111111-1111-1111-1111-111111111111"));
        }
    }
}
