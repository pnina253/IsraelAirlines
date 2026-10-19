
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
    private readonly IMapper _mapper;

    public FlightService(
        IFlightRepository flightRepository,
        IAircraftRepository aircraftRepository,
        IMapper mapper)
    {
        _flightRepository = flightRepository;
        _aircraftRepository = aircraftRepository;
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

    public async Task<IReadOnlyList<FlightResponse>> SearchAsync(
        string? origin,
        string? destination,
        CancellationToken cancellationToken = default)
    {
        var flights = await _flightRepository.SearchAsync(
            origin, destination, cancellationToken);

        return _mapper.Map<IReadOnlyList<FlightResponse>>(flights);
    }

    public async Task<FlightResponse> CreateAsync(
        FlightRequest request,
        CancellationToken cancellationToken = default)
    {
        if (request.DurationMinutes <= 0) 
        {
            throw new ArgumentException(
                "Duration minutes must be a positive number");
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
            throw new ArgumentException(
                "The specified aircraft is not active.");
        }

        var flight = new Flight
        {
            FlightNumber = request.FlightNumber.Trim(),
            Origin = request.Origin.Trim(),
            Destination = request.Destination.Trim(),
            DepartureTime = request.DeparetureTime,
            DurationMinutes = request.DurationMinutes,
            AircraftId = aircraft.Id,
            ArrivalTime = request.DeparetureTime.AddMinutes(request.DurationMinutes),
            Aircraft = aircraft
        };

        await _flightRepository.AddAsync(flight, cancellationToken);

        return _mapper.Map<FlightResponse>(flight);
    }

    
}
