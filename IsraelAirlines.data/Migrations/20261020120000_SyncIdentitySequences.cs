using IsraelAirlines.data.Contex;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IsraelAirlines.data.Migrations
{
    // Seed rows are inserted with explicit Ids, which does not advance the
    // identity sequences in PostgreSQL. This moves each sequence past MAX(Id).
    [DbContext(typeof(DataContext))]
    [Migration("20261020120000_SyncIdentitySequences")]
    public partial class SyncIdentitySequences : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            foreach (var table in new[] { "Users", "Aircrafts", "Flights", "Reservations" })
            {
                migrationBuilder.Sql($"""
                    SELECT setval(
                        pg_get_serial_sequence('"{table}"', 'Id'),
                        COALESCE((SELECT MAX("Id") FROM "{table}"), 0) + 1,
                        false);
                    """);
            }
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Nothing to undo: the sequences stay ahead of the existing rows.
        }
    }
}
