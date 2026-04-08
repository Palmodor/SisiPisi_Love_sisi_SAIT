using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LoveSushiPMR.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddMissingColumns : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Promotions",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "EndDate", "StartDate" },
                values: new object[] { new DateTime(2027, 4, 7, 7, 43, 5, 780, DateTimeKind.Utc).AddTicks(422), new DateTime(2025, 4, 7, 7, 43, 5, 780, DateTimeKind.Utc).AddTicks(419) });

            migrationBuilder.UpdateData(
                table: "Promotions",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "EndDate", "StartDate" },
                values: new object[] { new DateTime(2027, 4, 7, 7, 43, 5, 780, DateTimeKind.Utc).AddTicks(429), new DateTime(2025, 4, 7, 7, 43, 5, 780, DateTimeKind.Utc).AddTicks(428) });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Promotions",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "EndDate", "StartDate" },
                values: new object[] { new DateTime(2027, 3, 19, 7, 58, 57, 642, DateTimeKind.Utc).AddTicks(3677), new DateTime(2025, 3, 19, 7, 58, 57, 642, DateTimeKind.Utc).AddTicks(3674) });

            migrationBuilder.UpdateData(
                table: "Promotions",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "EndDate", "StartDate" },
                values: new object[] { new DateTime(2027, 3, 19, 7, 58, 57, 642, DateTimeKind.Utc).AddTicks(3686), new DateTime(2025, 3, 19, 7, 58, 57, 642, DateTimeKind.Utc).AddTicks(3685) });
        }
    }
}
