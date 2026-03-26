using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartHome.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdateLightFeedKey : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "devices",
                keyColumn: "device_id",
                keyValue: new Guid("33333333-3333-3333-3333-333333333333"),
                column: "feed_key",
                value: "light-1");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "devices",
                keyColumn: "device_id",
                keyValue: new Guid("33333333-3333-3333-3333-333333333333"),
                column: "feed_key",
                value: "light_1");
        }
    }
}
