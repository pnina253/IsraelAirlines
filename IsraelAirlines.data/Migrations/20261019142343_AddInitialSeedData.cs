using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace IsraelAirlines.data.Migrations
{
    /// <inheritdoc />
    public partial class AddInitialSeedData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Aircrafts",
                columns: new[] { "Id", "IsActive", "Model", "RegistrationNumber" },
                values: new object[,]
                {
                    { 1, true, "Boeing 737", "4X-AAA" },
                    { 2, true, "Airbus A320", "4X-BBB" }
                });

            migrationBuilder.InsertData(
                table: "Flights",
                columns: new[] { "Id", "AircraftId", "ArrivalTime", "DepartureTime", "Destination", "DurationMinutes", "FlightNumber", "Origin" },
                values: new object[,]
                {
                    { 1, 1, new DateTime(2027, 1, 10, 13, 0, 0, 0, DateTimeKind.Utc), new DateTime(2027, 1, 10, 8, 0, 0, 0, DateTimeKind.Utc), "London", 300, "IA101", "Tel Aviv" },
                    { 2, 2, new DateTime(2027, 1, 11, 14, 30, 0, 0, DateTimeKind.Utc), new DateTime(2027, 1, 11, 9, 0, 0, 0, DateTimeKind.Utc), "Paris", 330, "IA202", "Tel Aviv" }
                });
        }



        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Flights",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "Aircrafts",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "Flights",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Aircrafts",
                keyColumn: "Id",
                keyValue: 1);
        }
    }
}
