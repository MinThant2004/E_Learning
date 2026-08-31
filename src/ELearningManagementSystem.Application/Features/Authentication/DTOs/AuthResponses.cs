using System.Collections.Generic;

namespace ELearningManagementSystem.Application.Features.Authentication.DTOs;

public record AuthResponse(
    string AccessToken,
    DateTime ExpiresAt,
    string RefreshToken,
    AuthUserDto User);

public record AuthUserDto(
    int UserId,
    string FullName,
    string Email,
    string Role,
    List<string> Permissions,
    bool MustChangePassword);

public record CurrentUserResponse(
    int UserId,
    string FullName,
    string Email,
    string Role,
    List<string> Permissions,
    bool MustChangePassword);

public record RegisterResponse(
    int UserId,
    string FullName,
    string Email,
    string Message);
