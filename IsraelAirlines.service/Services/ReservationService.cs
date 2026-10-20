
using AutoMapper;
using IsraelAirlines.core.Constants;
using IsraelAirlines.core.DTOs.Requests;
using IsraelAirlines.core.DTOs.Responses;
using IsraelAirlines.core.Entities;
using IsraelAirlines.core.Enums;
using IsraelAirlines.core.Exceptions;
using IsraelAirlines.core.Interfaces.Repository;
using IsraelAirlines.core.Interfaces.Services;
using Microsoft.Extensions.Logging;

namespace IsraelAirlines.Service.Services;

public class ReservationService : IReservationService
{
    public const string SeatTakenMessage =
        "This seat was just taken by another passenger. Please choose another seat.";

    private readonly IReservationRepository _reservationRepository;
    private readonly IFlightRepository _flightRepository;
    private readonly IFlightSeatRepository _seatRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ILogger<ReservationService> _logger;

    public ReservationService(
        IReservationRepository reservationRepository,
        IFlightRepository flightRepository,
        IFlightSeatRepository seatRepository,
        IUnitOfWork unitOfWork,
        IMapper mapper,
        ILogger<ReservationService> logger)
    {
        _reservationRepository = reservationRepository;
        _flightRepository = flightRepository;
        _seatRepository = seatRepository;
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _logger = logger;
    }

    public async Task<IReadOnlyList<ReservationResponse>> GetAllAsync(
        CancellationToken cancellationToken = default)
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
        var flightNumber = request.FlightNumber.Trim();
        var seatNumber = NormalizeSeat(request.SeatNumber);

        _logger.LogDebug(
            "User {UserId} requests seat {SeatNumber} on flight {FlightNumber}",
            userId, seatNumber, flightNumber);

        var flight = await _flightRepository.GetByFlightNumberAsync(
            flightNumber, cancellationToken);

        if (flight is null)
        {
            throw new NotFoundException(
                "The specified flight does not exist.");
        }

        if (!SeatMap.IsValidSeat(flight.Aircraft, seatNumber))
        {
            throw new BadRequestException(
                "The specified seat does not exist on this aircraft.");
        }

        var seat = await GetSeatAsync(flight.Id, seatNumber, cancellationToken);

        // The seat was read with its Version. If another request reserves it after this
        // check, our UPDATE no longer matches that Version and the save is rejected.
        EnsureAvailable(seat, flight.FlightNumber, userId);

        seat.Status = SeatStatus.Reserved;

        var reservation = new Reservation
        {
            UserId = userId,
            FlightId = flight.Id,
            SeatNumber = seatNumber,
            CreatedAt = DateTime.UtcNow
        };

        _reservationRepository.Add(reservation);

        // The seat update and the new reservation are saved together, in one transaction.
        await SaveSeatChangeAsync(flight.FlightNumber, seatNumber, userId, cancellationToken);

        _logger.LogInformation(
            "Reservation {ReservationId} created: seat {SeatNumber} on flight {FlightNumber} for user {UserId}",
            reservation.Id, seatNumber, flight.FlightNumber, userId);

        var response = _mapper.Map<ReservationResponse>(reservation);
        response.FlightNumber = flight.FlightNumber;
        return response;
    }

    public async Task<ReservationResponse> ChangeSeatAsync(
        int reservationId,
        int userId,
        bool isAdmin,
        ReservationUpdateRequest request,
        CancellationToken cancellationToken = default)
    {
        var reservation = await GetOwnedReservationAsync(
            reservationId, userId, isAdmin, cancellationToken);

        var newSeatNumber = NormalizeSeat(request.SeatNumber);

        if (newSeatNumber == reservation.SeatNumber)
            return _mapper.Map<ReservationResponse>(reservation);

        var newSeat = await GetSeatAsync(
            reservation.FlightId, newSeatNumber, cancellationToken);

        EnsureAvailable(newSeat, reservation.Flight.FlightNumber, userId);

        var oldSeat = await _seatRepository.GetForUpdateAsync(
            reservation.FlightId, reservation.SeatNumber, cancellationToken);

        if (oldSeat is not null)
            oldSeat.Status = SeatStatus.Available;

        newSeat.Status = SeatStatus.Reserved;
        reservation.SeatNumber = newSeatNumber;

        await SaveSeatChangeAsync(
            reservation.Flight.FlightNumber, newSeatNumber, userId, cancellationToken);

        _logger.LogInformation(
            "Reservation {ReservationId} moved to seat {SeatNumber}",
            reservation.Id, newSeatNumber);

        return _mapper.Map<ReservationResponse>(reservation);
    }

    public async Task CancelAsync(
        int reservationId,
        int userId,
        bool isAdmin,
        CancellationToken cancellationToken = default)
    {
        var reservation = await GetOwnedReservationAsync(
            reservationId, userId, isAdmin, cancellationToken);

        var seat = await _seatRepository.GetForUpdateAsync(
            reservation.FlightId, reservation.SeatNumber, cancellationToken);

        if (seat is not null)
            seat.Status = SeatStatus.Available;

        _reservationRepository.Remove(reservation);

        try
        {
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
        catch (ConcurrencyConflictException)
        {
            _logger.LogWarning(
                "Concurrency conflict while cancelling reservation {ReservationId}",
                reservationId);

            throw new ConcurrencyConflictException(
                "The reservation was changed by another request. Refresh and try again.");
        }

        _logger.LogInformation(
            "Reservation {ReservationId} cancelled by user {UserId}",
            reservationId, userId);
    }

    private static string NormalizeSeat(string seatNumber) =>
        seatNumber.Trim().ToUpperInvariant();

    private async Task<FlightSeat> GetSeatAsync(
        int flightId,
        string seatNumber,
        CancellationToken cancellationToken)
    {
        return await _seatRepository.GetForUpdateAsync(
                flightId, seatNumber, cancellationToken)
            ?? throw new BadRequestException(
                "The specified seat does not exist on this flight.");
    }

    private void EnsureAvailable(FlightSeat seat, string flightNumber, int userId)
    {
        if (seat.Status == SeatStatus.Available)
            return;

        _logger.LogWarning(
            "Seat {SeatNumber} on flight {FlightNumber} is already reserved; request by user {UserId} rejected",
            seat.SeatNumber, flightNumber, userId);

        throw new ConflictException(
            "This seat is already reserved for the selected flight.");
    }

    // Two requests read the same available seat; the first save wins and the second lands here.
    private async Task SaveSeatChangeAsync(
        string flightNumber,
        string seatNumber,
        int userId,
        CancellationToken cancellationToken)
    {
        try
        {
            await _unitOfWork.SaveChangesAsync(cancellationToken);
        }
        catch (ConflictException)
        {
            // ConcurrencyConflictException: the seat's Version changed since we read it.
            // ConflictException: the unique (flight, seat) index rejected a second reservation.
            _logger.LogWarning(
                "Concurrency conflict on seat {SeatNumber} of flight {FlightNumber}: another request took it before user {UserId}",
                seatNumber, flightNumber, userId);

            throw new ConcurrencyConflictException(SeatTakenMessage);
        }
    }

    private async Task<Reservation> GetOwnedReservationAsync(
        int reservationId,
        int userId,
        bool isAdmin,
        CancellationToken cancellationToken)
    {
        var reservation = await _reservationRepository.GetForUpdateAsync(
            reservationId, cancellationToken)
            ?? throw new NotFoundException("The reservation does not exist.");

        if (!isAdmin && reservation.UserId != userId)
        {
            throw new ForbiddenException(
                "You can only change your own reservations.");
        }

        return reservation;
    }
}
