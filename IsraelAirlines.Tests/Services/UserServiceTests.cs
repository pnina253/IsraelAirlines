using IsraelAirlines.core.Constants;
using IsraelAirlines.core.DTOs.Requests;
using IsraelAirlines.core.Entities;
using IsraelAirlines.core.Exceptions;
using IsraelAirlines.core.Interfaces;
using IsraelAirlines.core.Interfaces.Repository;
using IsraelAirlines.Service.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using System.Security.Authentication;

namespace IsraelAirlines.Tests.Services;

public class UserServiceTests
{
    private readonly Mock<IUserRepository> _userRepository = new();
    private readonly Mock<IJwtService> _jwtService = new();
    private readonly PasswordHasher<User> _passwordHasher = new();
    private readonly UserService _service;

    public UserServiceTests()
    {
        _service = new UserService(
            _userRepository.Object,
            _passwordHasher,
            TestMapper.Create(),
            _jwtService.Object,
            NullLogger<UserService>.Instance);
    }

    private static RegisterRequest Register() => new()
    {
        FullName = " Dana Cohen ",
        Email = "dana@example.com",
        Password = "Secret123!"
    };

    [Fact]
    public async Task RegisterAsync_AlwaysAssignsUserRoleAndHashesPassword()
    {
        User? added = null;
        _userRepository
            .Setup(r => r.AddAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()))
            .Callback<User, CancellationToken>((u, _) => added = u);

        var result = await _service.RegisterAsync(Register());

        Assert.NotNull(added);
        Assert.Equal(Roles.User, added.Role);
        Assert.Equal("Dana Cohen", added.FullName);
        Assert.NotEqual("Secret123!", added.PasswordHash);
        Assert.Equal(
            PasswordVerificationResult.Success,
            _passwordHasher.VerifyHashedPassword(added, added.PasswordHash, "Secret123!"));
        Assert.Equal("dana@example.com", result.Email);
    }

    [Fact]
    public async Task RegisterAsync_WhenEmailExists_ThrowsConflict()
    {
        _userRepository
            .Setup(r => r.GetByEmailAsync("dana@example.com", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new User { Id = 1, Email = "dana@example.com" });

        await Assert.ThrowsAsync<ConflictException>(() => _service.RegisterAsync(Register()));

        _userRepository.Verify(
            r => r.AddAsync(It.IsAny<User>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    private User SetupUser(string password)
    {
        var user = new User { Id = 4, Email = "dana@example.com", Role = Roles.Admin };
        user.PasswordHash = _passwordHasher.HashPassword(user, password);

        _userRepository
            .Setup(r => r.GetByEmailAsync("dana@example.com", It.IsAny<CancellationToken>()))
            .ReturnsAsync(user);

        return user;
    }

    [Fact]
    public async Task LoginAsync_WithWrongPassword_ThrowsAuthenticationException()
    {
        SetupUser("Secret123!");

        await Assert.ThrowsAsync<AuthenticationException>(() => _service.LoginAsync(
            new LoginRequest { Email = "dana@example.com", Password = "Wrong1234!" }));

        _jwtService.Verify(
            j => j.GenerateToken(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>()),
            Times.Never);
    }

    [Fact]
    public async Task LoginAsync_WithCorrectPassword_ReturnsTokenWithTheUsersRole()
    {
        SetupUser("Secret123!");
        _jwtService
            .Setup(j => j.GenerateToken(4, "dana@example.com", Roles.Admin))
            .Returns("token");

        var result = await _service.LoginAsync(
            new LoginRequest { Email = "dana@example.com", Password = "Secret123!" });

        Assert.Equal("token", result.Token);
        Assert.Equal(Roles.Admin, result.Role);
    }
}
