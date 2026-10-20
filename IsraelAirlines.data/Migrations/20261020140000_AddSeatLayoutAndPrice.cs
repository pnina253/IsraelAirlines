using IsraelAirlines.data.Context;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IsraelAirlines.data.Migrations
{
    // Seat layout lets the client draw a seat map and the server validate seat numbers.
    // Existing aircraft get a 30-row "ABC DEF" layout; existing flights get price 0 (unset).
    [DbContext(typeof(DataContext))]
    [Migration("20261020140000_AddSeatLayoutAndPrice")]
    public partial class AddSeatLayoutAndPrice : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Rows",
                table: "Aircrafts",
                type: "integer",
                nullable: false,
                defaultValue: 30);

            migrationBuilder.AddColumn<string>(
                name: "SeatLayout",
                table: "Aircrafts",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "ABC DEF");

            migrationBuilder.AddColumn<int>(
                name: "BusinessRows",
                table: "Aircrafts",
                type: "integer",
                nullable: false,
                defaultValue: 3);

            migrationBuilder.AddColumn<decimal>(
                name: "Price",
                table: "Flights",
                type: "numeric(10,2)",
                precision: 10,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            // Demo prices for the two seeded flights.
            migrationBuilder.Sql("UPDATE \"Flights\" SET \"Price\" = 1250 WHERE \"Id\" = 1;");
            migrationBuilder.Sql("UPDATE \"Flights\" SET \"Price\" = 1100 WHERE \"Id\" = 2;");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Price",
                table: "Flights");

            migrationBuilder.DropColumn(
                name: "BusinessRows",
                table: "Aircrafts");

            migrationBuilder.DropColumn(
                name: "SeatLayout",
                table: "Aircrafts");

            migrationBuilder.DropColumn(
                name: "Rows",
                table: "Aircrafts");
        }
    }
}
