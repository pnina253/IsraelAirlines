using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IsraelAirlines.data.Migrations
{
    /// <inheritdoc />
    public partial class RemoveReservationSeed : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Reservations",
                keyColumn: "Id",
                keyValue: 1);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Reservations",
                columns: new[] { "Id", "CreatedAt", "FlightId", "SeatNumber", "UserId" },
                values: new object[] { 1, new DateTime(2026, 10, 19, 12, 0, 0, 0, DateTimeKind.Utc), 1, "12A", 2 });
        }
    }
}
