
using AutoMapper;
using IsraelAirlines.core.Constants;
using IsraelAirlines.core.DTOs.Requests;
using IsraelAirlines.core.DTOs.Responses;
using IsraelAirlines.core.Entities;
using IsraelAirlines.core.Exceptions;
using IsraelAirlines.core.Interfaces.Repository;
using IsraelAirlines.core.Interfaces.Services;
using Microsoft.Extensions.Logging;

namespace IsraelAirlines.Service.Services;

public class FlightService : IFlightService
{
    private readonly IFlightRepository _flightRepository;
    private readonly IAircraftRepository _aircraftRepository;
    private readonly IFlightSeatRepository _seatRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ILogger<FlightService> _logger;

    public FlightService(
        IFlightRepository flightRepository,
        IAircraftRepository aircraftRepository,
        IFlightSeatRepository seatRepository,
        IUnitOfWork unitOfWork,
        IMapper mapper,
        ILogger<FlightService> logger)
    {
        _flightRepository = flightRepository;
        _aircraftRepository = aircraftRepository;
        _seatRepository = seatRepository;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<FlightResponse?> GetByIdAsync(
        int id,
        CancellationToken cancellationToken = default)
    {
        var flight = await _flightRepository.GetByIdAsync(
            id, cancellationToken);

        if (flight is null)
            return null;

        return _mapper.Map<FlightResponse>(flight);
    }

    public async Task<PagedResult<FlightResponse>> SearchAsync(
        FlightSearchQuery query,
        CancellationToken cancellationToken = default)
    {
        if (query.FromDate is { } from && query.ToDate is { } to && from > to)
        {
            throw new BadRequestException(
                "From date must not be after to date.");
        }

        var (flights, totalCount) = await _flightRepository.SearchAsync(
            query, cancellationToken);

        return new PagedResult<FlightResponse>
        {
            Items = _mapper.Map<IReadOnlyList<FlightResponse>>(flights),
            Page = query.Page,
            PageSize = query.PageSize,
            TotalCount = totalCount
        };
    }

    public async Task<IReadOnlyList<string>?> GetReservedSeatsAsync(
        int flightId,
        CancellationToken cancellationToken = default)
    {
        var flight = await _flightRepository.GetByIdAsync(
            flightId, cancellationToken);

        if (flight is null)
            return null;

        return await _seatRepository.GetReservedSeatNumbersAsync(
            flightId, cancellationToken);
    }

    public async Task<FlightResponse> CreateAsync(
        FlightRequest request,
        CancellationToken cancellationToken = default)
    {
        var departure = ValidateSchedule(request);
        var flightNumber = request.FlightNumber.Trim();

        var existingFlight = await _flightRepository.GetByFlightNumberAsync(
            flightNumber, cancellationToken);

        if (existingFlight is not null)
        {
            throw new ConflictException(
                "A flight with this number already exists.");
        }

        var aircraft = await GetActiveAircraftAsync(
            request.RegistrationNumber, cancellationToken);

        var flight = _mapper.Map<Flight>(request);
        flight.DepartureTime = departure;
        flight.ArrivalTime = departure.AddMinutes(request.DurationMinutes);
        flight.AircraftId = aircraft.Id;
        flight.Aircraft = aircraft;
        AddSeats(flight, aircraft);

        await _flightRepository.AddAsync(flight, cancellationToken);

        _logger.LogInformation(
            "Flight {FlightNumber} created with {SeatCount} seats",
            flight.FlightNumber, flight.Seats.Count);

        return _mapper.Map<FlightResponse>(flight);
    }

    public async Task<FlightResponse?> UpdateAsync(
        int id,
        FlightRequest request,
        CancellationToken cancellationToken = default)
    {
        var flight = await _flightRepository.GetForUpdateAsync(
            id, cancellationToken);

        if (flight is null)
            return null;

        var departure = ValidateSchedule(request);
        var flightNumber = request.FlightNumber.Trim();

        if (flightNumber != flight.FlightNumber)
        {
            var existingFlight = await _flightRepository.GetByFlightNumberAsync(
                flightNumber, cancellationToken);

            if (existingFlight is not null)
            {
                throw new ConflictException(
                    "A flight with this number already exists.");
            }
        }

        var registrationNumber = request.RegistrationNumber.Trim();

        if (registrationNumber != flight.Aircraft.RegistrationNumber)
        {
            // The seats belong to the aircraft layout, so they can only be rebuilt while empty.
            if (await _flightRepository.HasReservationsAsync(flight.Id, cancellationToken))
            {
                throw new ConflictException(
                    "A flight with reservations cannot be moved to another aircraft.");
            }

            var aircraft = await GetActiveAircraftAsync(
                registrationNumber, cancellationToken);

            flight.Aircraft = aircraft;
            flight.AircraftId = aircraft.Id;
            flight.Seats.Clear();
            AddSeats(flight, aircraft);
        }

        _mapper.Map(request, flight);
        flight.DepartureTime = departure;
        flight.ArrivalTime = departure.AddMinutes(request.DurationMinutes);

        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Flight {FlightId} updated", flight.Id);

        return _mapper.Map<FlightResponse>(flight);
    }

    public async Task<bool> DeleteAsync(
        int id,
        CancellationToken cancellationToken = default)
    {
        var flight = await _flightRepository.GetForUpdateAsync(
            id, cancellationToken);

        if (flight is null)
            return false;

        if (await _flightRepository.HasReservationsAsync(flight.Id, cancellationToken))
        {
            throw new ConflictException(
                "A flight with reservations cannot be deleted.");
        }

        _flightRepository.Remove(flight);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Flight {FlightId} deleted", id);

        return true;
    }

    // Business rules that the Data Annotations on FlightRequest cannot express.
    private static DateTime ValidateSchedule(FlightRequest request)
    {
        if (request.DurationMinutes <= 0)
        {
            throw new BadRequestException(
                "Duration minutes must be a positive number");
        }

        var departure = NormalizeToUtc(request.DepartureTime!.Value);

        if (departure <= DateTime.UtcNow)
        {
            throw new BadRequestException(
                "Departure time must be in the future.");
        }

        if (string.Equals(
            request.Origin.Trim(),
            request.Destination.Trim(),
            StringComparison.OrdinalIgnoreCase))
        {
            throw new BadRequestException(
                "Origin and destination must differ.");
        }

        return departure;
    }

    private async Task<Aircraft> GetActiveAircraftAsync(
        string registrationNumber,
        CancellationToken cancellationToken)
    {
        var aircraft = await _aircraftRepository.GetByRegistrationNumberAsync(
            registrationNumber.Trim(), cancellationToken);

        if (aircraft is null)
        {
            throw new NotFoundException(
                "The specified aircraft does not exist.");
        }

        if (!aircraft.IsActive)
        {
            throw new BadRequestException(
                "The specified aircraft is not active.");
        }

        return aircraft;
    }

    // Each flight gets its own copy of the aircraft's seats; these rows are what bookings compete for.
    private static void AddSeats(Flight flight, Aircraft aircraft)
    {
        foreach (var seatNumber in SeatMap.AllSeats(aircraft))
        {
            flight.Seats.Add(new FlightSeat { SeatNumber = seatNumber });
        }
    }

    // Npgsql rejects non-UTC values for timestamptz; a time without an offset is treated as UTC.
    private static DateTime NormalizeToUtc(DateTime value) => value.Kind switch
    {
        DateTimeKind.Utc => value,
        DateTimeKind.Local => value.ToUniversalTime(),
        _ => DateTime.SpecifyKind(value, DateTimeKind.Utc)
    };
}
