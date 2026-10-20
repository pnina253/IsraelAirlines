using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace IsraelAirlines.data.Migrations
{
    /// <inheritdoc />
    public partial class AddFlightSeatsAndAmenities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Amenities",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Amenities", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "FlightSeats",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    FlightId = table.Column<int>(type: "integer", nullable: false),
                    SeatNumber = table.Column<string>(type: "character varying(5)", maxLength: 5, nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false),
                    Version = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FlightSeats", x => x.Id);
                    table.ForeignKey(
                        name: "FK_FlightSeats_Flights_FlightId",
                        column: x => x.FlightId,
                        principalTable: "Flights",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AircraftAmenities",
                columns: table => new
                {
                    AircraftId = table.Column<int>(type: "integer", nullable: false),
                    AmenityId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AircraftAmenities", x => new { x.AircraftId, x.AmenityId });
                    table.ForeignKey(
                        name: "FK_AircraftAmenities_Aircrafts_AircraftId",
                        column: x => x.AircraftId,
                        principalTable: "Aircrafts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AircraftAmenities_Amenities_AmenityId",
                        column: x => x.AmenityId,
                        principalTable: "Amenities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "Amenities",
                columns: new[] { "Id", "Name" },
                values: new object[,]
                {
                    { 1, "Wi-Fi" },
                    { 2, "Meal" },
                    { 3, "Seat screens" },
                    { 4, "USB charging" }
                });

            migrationBuilder.InsertData(
                table: "AircraftAmenities",
                columns: new[] { "AircraftId", "AmenityId" },
                values: new object[,]
                {
                    { 1, 1 },
                    { 1, 2 },
                    { 2, 1 },
                    { 2, 2 },
                    { 2, 3 },
                    { 2, 4 }
                });

            migrationBuilder.CreateIndex(
                name: "IX_AircraftAmenities_AmenityId",
                table: "AircraftAmenities",
                column: "AmenityId");

            migrationBuilder.CreateIndex(
                name: "IX_Amenities_Name",
                table: "Amenities",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_FlightSeats_FlightId_SeatNumber",
                table: "FlightSeats",
                columns: new[] { "FlightId", "SeatNumber" },
                unique: true);

            // Amenities are seeded with explicit Ids; move the identity sequence past them.
            migrationBuilder.Sql("""
                SELECT setval(
                    pg_get_serial_sequence('"Amenities"', 'Id'),
                    COALESCE((SELECT MAX("Id") FROM "Amenities"), 0) + 1,
                    false);
                """);

            // Build the seats of every existing flight from its aircraft layout
            // (rows x seat letters), then mark the seats that already have a reservation.
            migrationBuilder.Sql("""
                INSERT INTO "FlightSeats" ("FlightId", "SeatNumber", "Status", "Version")
                SELECT f."Id", r.n::text || l.letter, 0, gen_random_uuid()
                FROM "Flights" f
                JOIN "Aircrafts" a ON a."Id" = f."AircraftId"
                CROSS JOIN LATERAL generate_series(1, a."Rows") AS r(n)
                CROSS JOIN LATERAL regexp_split_to_table(replace(a."SeatLayout", ' ', ''), '') AS l(letter);
                """);

            migrationBuilder.Sql("""
                UPDATE "FlightSeats" s
                SET "Status" = 1
                FROM "Reservations" r
                WHERE r."FlightId" = s."FlightId" AND r."SeatNumber" = s."SeatNumber";
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AircraftAmenities");

            migrationBuilder.DropTable(
                name: "FlightSeats");

            migrationBuilder.DropTable(
                name: "Amenities");
        }
    }
}
