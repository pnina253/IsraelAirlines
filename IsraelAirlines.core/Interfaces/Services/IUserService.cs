using IsraelAirlines.core.DTOs.Requests;
using IsraelAirlines.core.DTOs.Responses;
using IsraelAirlines.core.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace IsraelAirlines.core.Interfaces.Services
{
    public interface IUserService
    {
        Task<IReadOnlyList<UserResponse>> GetAllAsync(
           CancellationToken cancellationToken = default);
        Task<UserResponse?> GetByIdAsync(
            int id,
            CancellationToken cancellationToken = default);

        Task<UserResponse?> GetByEmailAsync(
            string email,
            CancellationToken cancellationToken = default);

        Task<UserResponse> RegisterAsync(
            RegisterRequest request,
            CancellationToken cancellationToken = default);

        Task<LoginResponse> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default);

    }
}
