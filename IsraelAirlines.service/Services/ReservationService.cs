
using AutoMapper;
using IsraelAirlines.core.Constants;
using IsraelAirlines.core.DTOs.Requests;
using IsraelAirlines.core.DTOs.Responses;
using IsraelAirlines.core.Entities;
using IsraelAirlines.core.Interfaces.Repository;
using IsraelAirlines.core.Interfaces.Services;
using IsraelAirlines.core.Exceptions;
namespace IsraelAirlines.Service.Services;

public class ReservationService : IReservationService
{
    private readonly IReservationRepository _reservationRepository;
    private readonly IFlightRepository _flightRepository;
    private readonly IMapper _mapper;

    public ReservationService(
        IReservationRepository reservationRepository,
        IFlightRepository flightRepository,
        IMapper mapper)
    {
        _reservationRepository = reservationRepository;
        _flightRepository = flightRepository;
        _mapper = mapper;
    }

    public async Task<IReadOnlyList<ReservationResponse>> GetAllAsync(
        CancellationToken cancellationToken)
    {
        var reservations = await _reservationRepository.GetAllAsync(
            cancellationToken);

        return _mapper.Map<IReadOnlyList<ReservationResponse>>(
            reservations);
    }

    public async Task<ReservationResponse?> GetByIdAsync(
        int id,
        CancellationToken cancellationToken = default)
    {
        var reservation = await _reservationRepository.GetByIdAsync(
            id, cancellationToken);

        if (reservation is null)
            return null;

        return _mapper.Map<ReservationResponse>(reservation);
    }

    public async Task<IReadOnlyList<ReservationResponse>> GetByUserIdAsync(
        int userId,
        CancellationToken cancellationToken = default)
    {
        var reservations = await _reservationRepository.GetByUserIdAsync(
            userId, cancellationToken);

        return _mapper.Map<IReadOnlyList<ReservationResponse>>(
            reservations);
    }

    public async Task<ReservationResponse> CreateAsync(
        int userId,
        ReservationRequest request,
        CancellationToken cancellationToken = default)
    {
        var flight = await _flightRepository.GetByFlightNumberAsync(
            request.FlightNumber, cancellationToken);

        if (flight is null)
        {
            throw new NotFoundException(
                "The specified flight does not exist.");
        }

        var seatNumber = request.SeatNumber.Trim().ToUpperInvariant();

        if (!SeatMap.IsValidSeat(flight.Aircraft, seatNumber))
        {
            throw new BadRequestException(
                "The specified seat does not exist on this aircraft.");
        }

        var seatIsReserved =
            await _reservationRepository.IsSeatReservedAsync(
                flight.Id,
                seatNumber,
                cancellationToken);

        if (seatIsReserved)
        {
            throw new ConflictException(
                "This seat is already reserved for the selected flight.");
        }

        var reservation = new Reservation
        {
            UserId = userId,
            FlightId = flight.Id,
            Flight = flight,
            SeatNumber = seatNumber,
            CreatedAt = DateTime.UtcNow
        };

        await _reservationRepository.AddAsync(
            reservation, cancellationToken);

        return _mapper.Map<ReservationResponse>(reservation);
    }
}
