using ELearningManagementSystem.Shared.Constants;
using ELearningManagementSystem.Application.Common;
using ELearningManagementSystem.Application.Features.Authentication.DTOs;
using ELearningManagementSystem.Application.Features.Authentication.Interfaces;
using ELearningManagementSystem.Application.Interfaces;
using ELearningManagementSystem.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ELearningManagementSystem.Application.Features.AuditLogs.Services;
using ELearningManagementSystem.Application.Features.AuditLogs.DTOs;

namespace ELearningManagementSystem.Application.Features.Authentication.Services;

public class AuthService : IAuthService
{
    private readonly IAppDbContext _context;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtTokenService _jwtTokenService;
    private readonly ILogger<AuthService> _logger;
    private readonly IAuditLogService _auditLogService;
    private readonly IEmailSender _emailSender;

    private const string StudentRoleName = "Student";

    public AuthService(
        IAppDbContext context,
        IPasswordHasher passwordHasher,
        IJwtTokenService jwtTokenService,
        ILogger<AuthService> logger,
        IAuditLogService auditLogService,
        IEmailSender emailSender)
    {
        _context = context;
        _passwordHasher = passwordHasher;
        _jwtTokenService = jwtTokenService;
        _logger = logger;
        _auditLogService = auditLogService;
        _emailSender = emailSender;
    }

    public async Task<Result<RegisterResponse>> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            var normalizedEmail = request.Email.Trim().ToLowerInvariant();

            // Check for duplicate email (including soft-deleted â€” we do not allow re-use of deleted emails)
            var existingUser = await _context.Users
                .AnyAsync(u => u.Email.ToLower() == normalizedEmail, cancellationToken);

            if (existingUser)
            {
                _logger.LogWarning("Registration failed: email already exists. Email={Email}", normalizedEmail);
                return Result.Failure<RegisterResponse>("EmailAlreadyExists: An account with this email already exists.");
            }

            // Find the Student role server-side â€” never trust client input
            var studentRole = await _context.Roles
                .FirstOrDefaultAsync(r => r.RoleName == StudentRoleName, cancellationToken);

            if (studentRole is null)
            {
                _logger.LogError("Registration failed: Student role is not configured in the database.");
                return Result.Failure<RegisterResponse>("StudentRoleNotConfigured: The Student role is not configured. Please contact an administrator.");
            }

            var passwordHash = _passwordHasher.Hash(request.Password);

            var user = new User
            {
                FullName = request.FullName.Trim(),
                Email = normalizedEmail,
                PasswordHash = passwordHash,
                Status = true,
                DeleteFlag = false,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync(cancellationToken);

            var userRole = new UserRole
            {
                UserId = user.UserId,
                RoleId = studentRole.RoleId,
                AssignedAt = DateTime.UtcNow,
                AssignedBy = null
            };

            _context.UserRoles.Add(userRole);
            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("User registered successfully. UserId={UserId}, Email={Email}, Role=Student",
                user.UserId, user.Email);

            await _auditLogService.CreateAuditLogAsync(new CreateAuditLogRequest
            {
                UserId = user.UserId,
                Action = "Create",
                TableName = "Users",
                RecordId = user.UserId
            }, cancellationToken);

            return Result.Success(new RegisterResponse(
                user.UserId,
                user.FullName,
                user.Email,
                "Registration successful. Please log in."));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error during registration. Email={Email}", request.Email);
            throw;
        }
    }

    public async Task<Result<AuthResponse>> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            var normalizedEmail = request.Email.Trim().ToLowerInvariant();

            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Email.ToLower() == normalizedEmail, cancellationToken);

            // Generic error â€” do not reveal whether email exists or password is wrong
            if (user is null)
            {
                _logger.LogWarning("Login failed: user not found. Email={Email}", normalizedEmail);
                return Result.Failure<AuthResponse>("InvalidCredentials: Invalid email or password.");
            }

            if (!_passwordHasher.Verify(request.Password, user.PasswordHash))
            {
                _logger.LogWarning("Login failed: incorrect password. UserId={UserId}", user.UserId);
                return Result.Failure<AuthResponse>("InvalidCredentials: Invalid email or password.");
            }

            // Deactivation is checked after password verification so the account's
            // status is only revealed to someone holding valid credentials.
            if (user.DeleteFlag)
            {
                _logger.LogWarning("Login failed: account deactivated. UserId={UserId}, Email={Email}", user.UserId, user.Email);
                return Result.Failure<AuthResponse>("AccountDeactivated: Your account has been deactivated. Please contact support.");
            }

            // Load all user roles
            var userRoles = await _context.UserRoles
                .Include(ur => ur.Role)
                .Where(ur => ur.UserId == user.UserId)
                .Select(ur => ur.Role.RoleName)
                .ToListAsync(cancellationToken);

            if (!userRoles.Any())
            {
                userRoles.Add(StudentRoleName);
            }

            // Determine primary role name for DTO (highest priority)
            string roleName = StudentRoleName;
            if (userRoles.Contains(AppConstants.SystemAdministratorRole)) roleName = AppConstants.SystemAdministratorRole;
            else if (userRoles.Contains(AppConstants.AdministratorRole)) roleName = AppConstants.AdministratorRole;
            else if (userRoles.Any()) roleName = userRoles.First();

            // Load dynamic permissions
            var permissions = await GetUserPermissionsAsync(user.UserId, cancellationToken);

            // Issue access token
            var accessToken = _jwtTokenService.GenerateToken(user, userRoles, permissions);
            var expiresAt = _jwtTokenService.GetExpiry();

            // Issue refresh token
            var refreshTokenValue = GenerateSecureToken();
            var refreshTokenHash = ComputeTokenHash(refreshTokenValue);

            var refreshToken = new RefreshToken
            {
                UserId = user.UserId,
                TokenHash = refreshTokenHash,
                ExpiresAt = DateTime.UtcNow.AddDays(7),
                CreatedAt = DateTime.UtcNow
            };

            _context.RefreshTokens.Add(refreshToken);
            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Login successful. UserId={UserId}, Email={Email}", user.UserId, user.Email);

            return Result.Success(new AuthResponse(
                accessToken,
                expiresAt,
                refreshTokenValue,
                new AuthUserDto(user.UserId, user.FullName, user.Email, roleName, permissions, user.MustChangePassword)));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error during login. Email={Email}", request.Email);
            throw;
        }
    }

    public async Task<Result<AuthResponse>> RefreshTokenAsync(string refreshToken, CancellationToken cancellationToken = default)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(refreshToken))
                return Result.Failure<AuthResponse>("InvalidRefreshToken: Refresh token is required.");

            var tokenHash = ComputeTokenHash(refreshToken);

            var storedToken = await _context.RefreshTokens
                .Include(rt => rt.User)
                .FirstOrDefaultAsync(rt => rt.TokenHash == tokenHash, cancellationToken);

            if (storedToken is null)
            {
                _logger.LogWarning("Refresh failed: token not found.");
                return Result.Failure<AuthResponse>("InvalidRefreshToken: Invalid refresh token.");
            }

            if (storedToken.IsRevoked)
            {
                _logger.LogWarning("Refresh failed: token is revoked. UserId={UserId}", storedToken.UserId);
                return Result.Failure<AuthResponse>("RevokedRefreshToken: This refresh token has been revoked.");
            }

            if (storedToken.IsExpired)
            {
                _logger.LogWarning("Refresh failed: token is expired. UserId={UserId}", storedToken.UserId);
                return Result.Failure<AuthResponse>("ExpiredRefreshToken: This refresh token has expired. Please log in again.");
            }

            var user = storedToken.User;

            if (user.DeleteFlag)
            {
                _logger.LogWarning("Refresh failed: user account is deleted. UserId={UserId}", storedToken.UserId);
                return Result.Failure<AuthResponse>("AccountDeleted: This account no longer exists.");
            }

            // Revoke old token
            storedToken.RevokedAt = DateTime.UtcNow;

            // Load all user roles
            var userRoles = await _context.UserRoles
                .Include(ur => ur.Role)
                .Where(ur => ur.UserId == user.UserId)
                .Select(ur => ur.Role.RoleName)
                .ToListAsync(cancellationToken);

            if (!userRoles.Any())
            {
                userRoles.Add(StudentRoleName);
            }

            // Determine primary role name for DTO (highest priority)
            string roleName = StudentRoleName;
            if (userRoles.Contains(AppConstants.SystemAdministratorRole)) roleName = AppConstants.SystemAdministratorRole;
            else if (userRoles.Contains(AppConstants.AdministratorRole)) roleName = AppConstants.AdministratorRole;
            else if (userRoles.Any()) roleName = userRoles.First();

            // Load dynamic permissions
            var permissions = await GetUserPermissionsAsync(user.UserId, cancellationToken);

            var newAccessToken = _jwtTokenService.GenerateToken(user, userRoles, permissions);
            var expiresAt = _jwtTokenService.GetExpiry();

            var newRefreshTokenValue = GenerateSecureToken();
            var newRefreshTokenHash = ComputeTokenHash(newRefreshTokenValue);

            var newRefreshToken = new RefreshToken
            {
                UserId = user.UserId,
                TokenHash = newRefreshTokenHash,
                ExpiresAt = DateTime.UtcNow.AddDays(7),
                CreatedAt = DateTime.UtcNow
            };

            _context.RefreshTokens.Add(newRefreshToken);
            await _context.SaveChangesAsync(cancellationToken);

            // Now we can set the replacement reference
            storedToken.ReplacedByTokenId = newRefreshToken.RefreshTokenId;
            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Token refreshed successfully. UserId={UserId}", user.UserId);

            return Result.Success(new AuthResponse(
                newAccessToken,
                expiresAt,
                newRefreshTokenValue,
                new AuthUserDto(user.UserId, user.FullName, user.Email, roleName, permissions, user.MustChangePassword)));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error during token refresh.");
            throw;
        }
    }

    public async Task<Result<CurrentUserResponse>> GetCurrentUserAsync(int userId, CancellationToken cancellationToken = default)
    {
        try
        {
            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.UserId == userId && !u.DeleteFlag, cancellationToken);

            if (user is null)
            {
                _logger.LogWarning("GetCurrentUser failed: user not found or deleted. UserId={UserId}", userId);
                return Result.Failure<CurrentUserResponse>("AccountNotFound: User account not found.");
            }

            // Load all user roles
            var userRoles = await _context.UserRoles
                .Include(ur => ur.Role)
                .Where(ur => ur.UserId == userId)
                .Select(ur => ur.Role.RoleName)
                .ToListAsync(cancellationToken);

            if (!userRoles.Any())
            {
                userRoles.Add(StudentRoleName);
            }

            // Determine primary role name for DTO (highest priority)
            string roleName = StudentRoleName;
            if (userRoles.Contains(AppConstants.SystemAdministratorRole)) roleName = AppConstants.SystemAdministratorRole;
            else if (userRoles.Contains(AppConstants.AdministratorRole)) roleName = AppConstants.AdministratorRole;
            else if (userRoles.Any()) roleName = userRoles.First();

            // Load dynamic permissions
            var permissions = await GetUserPermissionsAsync(user.UserId, cancellationToken);

            return Result.Success(new CurrentUserResponse(
                user.UserId,
                user.FullName,
                user.Email,
                roleName,
                permissions,
                user.MustChangePassword));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error in GetCurrentUser. UserId={UserId}", userId);
            throw;
        }
    }

    public async Task<Result> LogoutAsync(string refreshToken, CancellationToken cancellationToken = default)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(refreshToken))
                return Result.Success(); // Idempotent â€” nothing to revoke

            var tokenHash = ComputeTokenHash(refreshToken);

            var storedToken = await _context.RefreshTokens
                .FirstOrDefaultAsync(rt => rt.TokenHash == tokenHash && !rt.RevokedAt.HasValue, cancellationToken);

            if (storedToken is not null)
            {
                storedToken.RevokedAt = DateTime.UtcNow;
                await _context.SaveChangesAsync(cancellationToken);
                _logger.LogInformation("Logout: refresh token revoked. UserId={UserId}", storedToken.UserId);
            }

            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error during logout.");
            throw;
        }
    }

    public async Task<Result> ForgotPasswordAsync(string email, CancellationToken cancellationToken = default)
    {
        try
        {
            var normalizedEmail = email.Trim().ToLowerInvariant();

            var user = await _context.Users
                .FirstOrDefaultAsync(u => u.Email.ToLower() == normalizedEmail && !u.DeleteFlag, cancellationToken);

            // Always return a generic success message â€” do NOT reveal whether the email exists.
            if (user is null)
            {
                _logger.LogInformation("Password reset requested for unknown email. Email={Email}", normalizedEmail);
                return Result.Success();
            }

            // Invalidate any previously issued, unused OTPs for this user so only the latest one works.
            var outstanding = await _context.PasswordResetTokens
                .Where(t => t.UserId == user.UserId && !t.UsedAt.HasValue)
                .ToListAsync(cancellationToken);

            foreach (var token in outstanding)
            {
                token.UsedAt = DateTime.UtcNow;
            }

            var otp = GenerateOtp();
            var tokenHash = ComputeTokenHash(otp);

            var resetToken = new PasswordResetToken
            {
                UserId = user.UserId,
                TokenHash = tokenHash,
                ExpiresAt = DateTime.UtcNow.AddMinutes(1),
                CreatedAt = DateTime.UtcNow
            };

            _context.PasswordResetTokens.Add(resetToken);
            await _context.SaveChangesAsync(cancellationToken);

            var body = $"""
                <html>
                <body style="font-family:Arial,sans-serif;color:#1e293b;line-height:1.6;">
                    <h2 style="color:#4f46e5;">Your one-time password</h2>
                    <p>Hi {user.FullName},</p>
                    <p>Use the following code to reset your password. It is valid for <strong>1 minute</strong>:</p>
                    <p style="margin:24px 0;text-align:center;">
                        <span style="display:inline-block;background:#eef2ff;color:#4f46e5;font-size:28px;font-weight:700;letter-spacing:6px;padding:12px 24px;border-radius:10px;">{otp}</span>
                    </p>
                    <p>If you didn't request this, you can safely ignore this email.</p>
                </body>
                </html>
                """;

            await _emailSender.SendAsync(user.Email, "Your password reset code", body, cancellationToken);

            _logger.LogInformation("Password reset OTP sent. UserId={UserId}, Email={Email}", user.UserId, user.Email);

            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error during forgot-password. Email={Email}", email);
            throw;
        }
    }

    public async Task<Result> VerifyOtpAsync(string otp, CancellationToken cancellationToken = default)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(otp))
                return Result.Failure("InvalidOtp: The OTP is invalid.");

            var tokenHash = ComputeTokenHash(otp.Trim());

            var resetToken = await _context.PasswordResetTokens
                .FirstOrDefaultAsync(t => t.TokenHash == tokenHash && !t.UsedAt.HasValue, cancellationToken);

            if (resetToken is null)
            {
                _logger.LogWarning("OTP verification failed: OTP not found.");
                return Result.Failure("InvalidOtp: The OTP is invalid or has already been used.");
            }

            if (resetToken.IsExpired)
            {
                _logger.LogWarning("OTP verification failed: OTP expired.");
                return Result.Failure("ExpiredOtp: This OTP has expired. Please request a new one.");
            }

            // Record successful verification server-side. This is what unlocks the
            // reset step; the 1-minute lifetime applies only to entering the OTP.
            resetToken.VerifiedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("OTP verified successfully. UserId={UserId}", resetToken.UserId);

            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error during OTP verification.");
            throw;
        }
    }

    public async Task<Result> ResetPasswordAsync(string otp, string newPassword, CancellationToken cancellationToken = default)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(otp))
                return Result.Failure("InvalidOtp: The OTP is invalid.");

            var tokenHash = ComputeTokenHash(otp.Trim());

            var resetToken = await _context.PasswordResetTokens
                .Include(t => t.User)
                .FirstOrDefaultAsync(t => t.TokenHash == tokenHash && !t.UsedAt.HasValue, cancellationToken);

            if (resetToken is null)
            {
                _logger.LogWarning("Password reset failed: OTP not found.");
                return Result.Failure("InvalidOtp: The OTP is invalid or has already been used.");
            }

            // Only a successfully verified OTP may reset the password. The 1-minute
            // lifetime applies to the OTP entry step; once verified, the user may take
            // as long as needed to enter a new password.
            if (!resetToken.VerifiedAt.HasValue)
            {
                _logger.LogWarning("Password reset failed: OTP was never verified. UserId={UserId}", resetToken.UserId);
                return Result.Failure("OtpNotVerified: Verify your one-time password before resetting your password.");
            }

            var user = resetToken.User;

            if (user.DeleteFlag)
            {
                _logger.LogWarning("Password reset failed: account deleted. UserId={UserId}", user.UserId);
                return Result.Failure("AccountNotFound: This account is no longer available.");
            }

            user.PasswordHash = _passwordHasher.Hash(newPassword);
            user.MustChangePassword = false;
            user.UpdatedAt = DateTime.UtcNow;

            resetToken.UsedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync(cancellationToken);

            await _auditLogService.CreateAuditLogAsync(new CreateAuditLogRequest
            {
                UserId = user.UserId,
                Action = "ResetPassword",
                TableName = "Users",
                RecordId = user.UserId
            }, cancellationToken);

            _logger.LogInformation("Password reset successful. UserId={UserId}, Email={Email}", user.UserId, user.Email);

            return Result.Success();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error during password reset.");
            throw;
        }
    }

    // â”€â”€ Helpers â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€â”€

    private async Task<List<string>> GetUserPermissionsAsync(int userId, CancellationToken cancellationToken)
    {
        return await _context.UserRoles
            .AsNoTracking()
            .Where(ur => ur.UserId == userId)
            .SelectMany(ur => ur.Role.RolePermissions)
            .Select(rp => rp.Permission.PermissionCode)
            .Distinct()
            .ToListAsync(cancellationToken);
    }

    private static string GenerateOtp()
    {
        var bytes = new byte[3];
        using var rng = System.Security.Cryptography.RandomNumberGenerator.Create();
        rng.GetBytes(bytes);
        var code = ((bytes[0] << 16) | (bytes[1] << 8) | bytes[2]) % 1_000_000;
        return code.ToString("D6");
    }

    private static string GenerateSecureToken()
    {
        var bytes = new byte[64];
        using var rng = System.Security.Cryptography.RandomNumberGenerator.Create();
        rng.GetBytes(bytes);
        return Convert.ToBase64String(bytes);
    }

    private static string ComputeTokenHash(string token)
    {
        var bytes = System.Text.Encoding.UTF8.GetBytes(token);
        var hash = System.Security.Cryptography.SHA256.HashData(bytes);
        return Convert.ToBase64String(hash);
    }
}
