
using AutoMapper;
using IsraelAirlines.core.DTOs.Requests;
using IsraelAirlines.core.DTOs.Responses;
using IsraelAirlines.core.Entities;
using IsraelAirlines.core.Interfaces;
using IsraelAirlines.core.Interfaces.Repository;
using IsraelAirlines.core.Interfaces.Services;
using IsraelAirlines.core.Exceptions;
namespace IsraelAirlines.Service.Services;

public class FlightService : IFlightService
{
    private readonly IFlightRepository _flightRepository;
    private readonly IAircraftRepository _aircraftRepository;
    private readonly IReservationRepository _reservationRepository;
    private readonly IMapper _mapper;

    public FlightService(
        IFlightRepository flightRepository,
        IAircraftRepository aircraftRepository,
        IReservationRepository reservationRepository,
        IMapper mapper)
    {
        _flightRepository = flightRepository;
        _aircraftRepository = aircraftRepository;
        _reservationRepository = reservationRepository;
        _mapper = mapper;
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

        return await _reservationRepository.GetReservedSeatsAsync(
            flightId, cancellationToken);
    }

    public async Task<FlightResponse> CreateAsync(
        FlightRequest request,
        CancellationToken cancellationToken = default)
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

        var origin = request.Origin.Trim();
        var destination = request.Destination.Trim();

        if (string.Equals(origin, destination, StringComparison.OrdinalIgnoreCase))
        {
            throw new BadRequestException(
                "Origin and destination must differ.");
        }

        var flightNumber = request.FlightNumber.Trim();

        var existingFlight = await _flightRepository.GetByFlightNumberAsync(
            flightNumber, cancellationToken);

        if (existingFlight is not null)
        {
            throw new ConflictException(
                "A flight with this number already exists.");
        }

        var aircraft = await _aircraftRepository.GetByRegistrationNumberAsync(
            request.RegistrationNumber, cancellationToken);

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

        var flight = new Flight
        {
            FlightNumber = flightNumber,
            Origin = origin,
            Destination = destination,
            DepartureTime = departure,
            DurationMinutes = request.DurationMinutes,
            AircraftId = aircraft.Id,
            ArrivalTime = departure.AddMinutes(request.DurationMinutes),
            Price = request.Price!.Value,
            Aircraft = aircraft
        };

        await _flightRepository.AddAsync(flight, cancellationToken);

        return _mapper.Map<FlightResponse>(flight);
    }

    // Npgsql rejects non-UTC values for timestamptz; a time without an offset is treated as UTC.
    private static DateTime NormalizeToUtc(DateTime value) => value.Kind switch
    {
        DateTimeKind.Utc => value,
        DateTimeKind.Local => value.ToUniversalTime(),
        _ => DateTime.SpecifyKind(value, DateTimeKind.Utc)
    };
}
