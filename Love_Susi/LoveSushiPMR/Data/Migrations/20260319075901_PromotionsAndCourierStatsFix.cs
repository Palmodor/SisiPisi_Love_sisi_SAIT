using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace LoveSushiPMR.Data.Migrations
{
    /// <inheritdoc />
    public partial class PromotionsAndCourierStatsFix : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
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

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Promotions",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "EndDate", "StartDate" },
                values: new object[] { new DateTime(2027, 3, 19, 7, 39, 36, 497, DateTimeKind.Utc).AddTicks(9730), new DateTime(2025, 3, 19, 7, 39, 36, 497, DateTimeKind.Utc).AddTicks(9726) });

            migrationBuilder.UpdateData(
                table: "Promotions",
                keyColumn: "Id",
                keyValue: 2,
                columns: new[] { "EndDate", "StartDate" },
                values: new object[] { new DateTime(2027, 3, 19, 7, 39, 36, 497, DateTimeKind.Utc).AddTicks(9739), new DateTime(2025, 3, 19, 7, 39, 36, 497, DateTimeKind.Utc).AddTicks(9738) });
        }
    }
}
