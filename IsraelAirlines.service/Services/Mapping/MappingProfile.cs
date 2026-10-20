
using AutoMapper;
using IsraelAirlines.core.DTOs.Requests;
using IsraelAirlines.core.DTOs.Responses;
using IsraelAirlines.core.Entities;

namespace IsraelAirlines.Service.Services.Mapping;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        // Entity -> response

        CreateMap<Aircraft, AircraftResponse>()
            .ForMember(
                dest => dest.Amenities,
                opt => opt.MapFrom(src => src.Amenities
                    .OrderBy(a => a.Name)
                    .Select(a => a.Name)));

        CreateMap<Flight, FlightResponse>()
            .ForMember(
                dest => dest.AircraftModel,
                opt => opt.MapFrom(src => src.Aircraft.Model))
            .ForMember(
                dest => dest.RegistrationNumber,
                opt => opt.MapFrom(src => src.Aircraft.RegistrationNumber))
            .ForMember(
                dest => dest.Amenities,
                opt => opt.MapFrom(src => src.Aircraft.Amenities
                    .OrderBy(a => a.Name)
                    .Select(a => a.Name)));

        CreateMap<User, UserResponse>();

        CreateMap<Reservation, ReservationResponse>()
            .ForMember(
                dest => dest.FlightNumber,
                opt => opt.MapFrom(src => src.Flight.FlightNumber));

        // Request -> entity. Fields the service computes or protects are ignored here,
        // so mapping onto an existing entity (update) never overwrites them.

        CreateMap<AircraftRequest, Aircraft>()
            .ForMember(dest => dest.Model, opt => opt.MapFrom(src => src.Model.Trim()))
            .ForMember(dest => dest.RegistrationNumber, opt => opt.MapFrom(src => src.RegistrationNumber.Trim()))
            .ForMember(dest => dest.SeatLayout, opt => opt.Ignore())
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.Flights, opt => opt.Ignore())
            .ForMember(dest => dest.Amenities, opt => opt.Ignore());

        CreateMap<FlightRequest, Flight>()
            .ForMember(dest => dest.FlightNumber, opt => opt.MapFrom(src => src.FlightNumber.Trim()))
            .ForMember(dest => dest.Origin, opt => opt.MapFrom(src => src.Origin.Trim()))
            .ForMember(dest => dest.Destination, opt => opt.MapFrom(src => src.Destination.Trim()))
            .ForMember(dest => dest.Price, opt => opt.MapFrom(src => src.Price ?? 0))
            .ForMember(dest => dest.DepartureTime, opt => opt.Ignore())
            .ForMember(dest => dest.ArrivalTime, opt => opt.Ignore())
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.AircraftId, opt => opt.Ignore())
            .ForMember(dest => dest.Aircraft, opt => opt.Ignore())
            .ForMember(dest => dest.Reservations, opt => opt.Ignore())
            .ForMember(dest => dest.Seats, opt => opt.Ignore());

        CreateMap<RegisterRequest, User>()
            .ForMember(dest => dest.FullName, opt => opt.MapFrom(src => src.FullName.Trim()))
            .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Email.Trim()))
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.PasswordHash, opt => opt.Ignore())
            .ForMember(dest => dest.Role, opt => opt.Ignore())
            .ForMember(dest => dest.Reservations, opt => opt.Ignore());
    }
}
