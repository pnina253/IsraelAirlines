using IsraelAirlines.data.Contex;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IsraelAirlines.data.Migrations
{
    // Reservations look up flights by FlightNumber, so it must be unique.
    [DbContext(typeof(DataContext))]
    [Migration("20261020130000_AddUniqueFlightNumber")]
    public partial class AddUniqueFlightNumber : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateIndex(
                name: "IX_Flights_FlightNumber",
                table: "Flights",
                column: "FlightNumber",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Flights_FlightNumber",
                table: "Flights");
        }
    }
}
