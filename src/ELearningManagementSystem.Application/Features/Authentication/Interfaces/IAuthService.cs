using ELearningManagementSystem.Application.Common;
using ELearningManagementSystem.Application.Features.Authentication.DTOs;

namespace ELearningManagementSystem.Application.Features.Authentication.Interfaces;

public interface IAuthService
{
    Task<Result<RegisterResponse>> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken = default);
    Task<Result<AuthResponse>> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default);
    Task<Result<AuthResponse>> RefreshTokenAsync(string refreshToken, CancellationToken cancellationToken = default);
    Task<Result<CurrentUserResponse>> GetCurrentUserAsync(int userId, CancellationToken cancellationToken = default);
    Task<Result> LogoutAsync(string refreshToken, CancellationToken cancellationToken = default);
}
