namespace ELearningManagementSystem.Application.Features.Authentication.DTOs;

public record RegisterRequest(string FullName, string Email, string Password);

public record LoginRequest(string Email, string Password);

public record RefreshTokenRequest(string RefreshToken);
