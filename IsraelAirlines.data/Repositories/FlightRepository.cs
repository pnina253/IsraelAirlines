using IsraelAirlines.core.DTOs.Requests;
using IsraelAirlines.core.Entities;
using IsraelAirlines.core.Exceptions;
using IsraelAirlines.core.Interfaces.Repository;
using IsraelAirlines.data.Contex;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using System;
using System.Collections.Generic;
using System.Text;

namespace IsraelAirlines.data.Repositories
{
    public class FlightRepository : IFlightRepository
    {
        private readonly DataContext _context;

        public FlightRepository(DataContext context)
        {
            _context = context;
        }

        public async Task<Flight?> GetByIdAsync(
            int id,
            CancellationToken cancellationToken = default)
        {
            return await _context.Flights
            .Include(f => f.Aircraft)
            .FirstOrDefaultAsync(f => f.Id == id, cancellationToken);
        }

        public async Task<(IReadOnlyList<Flight> Items, int TotalCount)> SearchAsync(
            FlightSearchQuery search,
            CancellationToken cancellationToken = default)
        {
            var query = _context.Flights
                .AsNoTracking()
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(search.Origin))
            {
                var pattern = ContainsPattern(search.Origin);
                query = query.Where(f => EF.Functions.ILike(f.Origin, pattern, "\\"));
            }

            if (!string.IsNullOrWhiteSpace(search.Destination))
            {
                var pattern = ContainsPattern(search.Destination);
                query = query.Where(f => EF.Functions.ILike(f.Destination, pattern, "\\"));
            }

            if (search.FromDate is { } fromDate)
            {
                var from = fromDate.ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);
                query = query.Where(f => f.DepartureTime >= from);
            }

            if (search.ToDate is { } toDate)
            {
                var to = toDate.AddDays(1).ToDateTime(TimeOnly.MinValue, DateTimeKind.Utc);
                query = query.Where(f => f.DepartureTime < to);
            }

            var totalCount = await query.CountAsync(cancellationToken);

            // A stable order is required, otherwise rows can repeat or vanish across pages.
            var items = await query
                .OrderBy(f => f.DepartureTime)
                .ThenBy(f => f.Id)
                .Skip((search.Page - 1) * search.PageSize)
                .Take(search.PageSize)
                .Include(f => f.Aircraft)
                .ToListAsync(cancellationToken);

            return (items, totalCount);
        }

        // Case-insensitive "contains" match; user input is escaped so % and _ are literal.
        private static string ContainsPattern(string value)
        {
            var escaped = value.Trim()
                .Replace("\\", "\\\\")
                .Replace("%", "\\%")
                .Replace("_", "\\_");

            return $"%{escaped}%";
        }

        public async Task<Flight?> GetByFlightNumberAsync(
            string flightNumber,
            CancellationToken cancellationToken)
        {
            return await _context.Flights
                .Include(f => f.Aircraft)
                .FirstOrDefaultAsync(
                    f => f.FlightNumber == flightNumber,
                    cancellationToken);
        }

        public async Task AddAsync(
            Flight flight,
            CancellationToken cancellationToken = default)
        {
            await _context.Flights.AddAsync(flight, cancellationToken);

            try
            {
                await _context.SaveChangesAsync(cancellationToken);
            }
            catch (DbUpdateException exception)
                when (exception.InnerException is PostgresException
                {
                    SqlState: PostgresErrorCodes.UniqueViolation
                } postgresException
                && postgresException.ConstraintName ==
                    "IX_Flights_FlightNumber")
            {
                throw new ConflictException(
                    "A flight with this number already exists.");
            }
        }
    }
}
