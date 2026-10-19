
using AutoMapper;
using IsraelAirlines.core.DTOs.Requests;
using IsraelAirlines.core.DTOs.Responses;
using IsraelAirlines.core.Entities;
using IsraelAirlines.core.Exceptions;
using IsraelAirlines.core.Interfaces;
using IsraelAirlines.core.Interfaces.Repository;
using IsraelAirlines.core.Interfaces.Services;
using Microsoft.AspNetCore.Identity;
using System.Security.Authentication;
namespace IsraelAirlines.Service.Services;

public class UserService : IUserService
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher<User> _passwordHasher;
    private readonly IMapper _mapper;
    private readonly IJwtService _jwtService;
    public UserService(
        IUserRepository userRepository,
        IPasswordHasher<User> passwordHasher,
        IMapper mapper,
        IJwtService jwtService)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
        _mapper = mapper;
        _jwtService = jwtService;
    }

    public async Task<UserResponse?> GetByIdAsync(
        int id,
        CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.GetByIdAsync(
            id, cancellationToken);

        if (user is null)
            return null;

        return _mapper.Map<UserResponse>(user);
    }

    public async Task<UserResponse?> GetByEmailAsync(
        string email,
        CancellationToken cancellationToken = default)
    {
        var user = await _userRepository.GetByEmailAsync(
            email, cancellationToken);

        if (user is null)
            return null;

        return _mapper.Map<UserResponse>(user);
    }

    public async Task<UserResponse> RegisterAsync(
        RegisterRequest request,
        CancellationToken cancellationToken = default)
    {
        var email = request.Email.Trim();

        var existingUser = await _userRepository.GetByEmailAsync(
            email, cancellationToken);

        if (existingUser is not null)
        {
            throw new ConflictException(
                "A user with this email already exists.");
        }

        var user = new User
        {
            FullName = request.FullName.Trim(),
            Email = email,
            Role = "User"
        };

        user.PasswordHash = _passwordHasher.HashPassword(
            user, request.Password);

        await _userRepository.AddAsync(user, cancellationToken);

        return _mapper.Map<UserResponse>(user);
    }

    public async Task<IReadOnlyList<UserResponse>> GetAllAsync(
        CancellationToken cancellationToken = default)
    {
        var users = await _userRepository.GetAllAsync(cancellationToken);

        return _mapper.Map<IReadOnlyList<UserResponse>>(users);
    }
    public async Task<LoginResponse> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default)
    {
        var email = request.Email.Trim();
        var user = await _userRepository.GetByEmailAsync(email, cancellationToken);

        if (user is null)
        {
            throw new AuthenticationException(
                "Invalid email or password.");
        }

        var result = _passwordHasher.VerifyHashedPassword(
            user,
            user.PasswordHash,
            request.Password);

        if (result == PasswordVerificationResult.Failed)
        {
            throw new AuthenticationException(
                "Invalid email or password.");
        }
        var token = _jwtService.GenerateToken(user.Id,user.Email,user.Role);
        var userResponse = _mapper.Map<UserResponse>(user);
        return new LoginResponse() { Token = token ,User = userResponse };
    }


}
