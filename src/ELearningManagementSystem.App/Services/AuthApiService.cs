using System.Collections.Generic;
using System.Net.Http.Json;
using Blazored.LocalStorage;

namespace ELearningManagementSystem.App.Services;

public class AuthApiService
{
    private readonly HttpClient _httpClient;
    private readonly TokenStorageService _tokenStorage;
    private readonly CustomAuthStateProvider _authStateProvider;

    public AuthApiService(
        HttpClient httpClient,
        TokenStorageService tokenStorage,
        CustomAuthStateProvider authStateProvider)
    {
        _httpClient = httpClient;
        _tokenStorage = tokenStorage;
        _authStateProvider = authStateProvider;
    }

    private string GetErrorMessage(ErrorResponse? error, string fallback)
    {
        if (error is null) return fallback;
        if (!string.IsNullOrWhiteSpace(error.Error)) return error.Error;
        if (error.Errors is not null && error.Errors.Length > 0) return string.Join(" ", error.Errors);
        return fallback;
    }

    private string ExtractMessage(string message)
    {
        return message.Contains(":") ? message.Split(':')[1].Trim() : message;
    }

    public async Task<AuthResult> RegisterAsync(string fullName, string email, string password)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync("api/auth/register", new
            {
                fullName,
                email,
                password
            });

            if (response.IsSuccessStatusCode)
                return AuthResult.Ok("Registration successful. Please log in.");

            var error = await response.Content.ReadFromJsonAsync<ErrorResponse>();
            return AuthResult.Fail(GetErrorMessage(error, "Registration failed."));
        }
        catch (Exception ex)
        {
            return AuthResult.Fail($"An unexpected error occurred: {ex.Message}");
        }
    }

    public async Task<AuthResult> LoginAsync(string email, string password, bool rememberMe = false)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync("api/auth/login", new
            {
                email,
                password
            });

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadFromJsonAsync<ErrorResponse>();
                return AuthResult.Fail(GetErrorMessage(error, "Login failed."));
            }

            var authResponse = await response.Content.ReadFromJsonAsync<AuthResponse>();
            if (authResponse is null)
                return AuthResult.Fail("Invalid response from server.");

            await _tokenStorage.SaveTokensAsync(authResponse.AccessToken, authResponse.RefreshToken, rememberMe);

            await _authStateProvider.MarkAsAuthenticatedAsync(authResponse.AccessToken);

            return AuthResult.Ok("Login successful.", authResponse.User?.MustChangePassword ?? false);
        }
        catch (Exception ex)
        {
            return AuthResult.Fail($"An unexpected error occurred: {ex.Message}");
        }
    }

    public async Task<bool> RefreshAccessTokenAsync()
    {
        try
        {
            var refreshToken = await _tokenStorage.GetRefreshTokenAsync();
            if (string.IsNullOrWhiteSpace(refreshToken))
                return false;

            var response = await _httpClient.PostAsJsonAsync("api/auth/refresh", new
            {
                refreshToken
            });

            if (!response.IsSuccessStatusCode)
            {
                await LogoutAsync();
                return false;
            }

            var authResponse = await response.Content.ReadFromJsonAsync<AuthResponse>();
            if (authResponse is null)
            {
                await LogoutAsync();
                return false;
            }

            await _tokenStorage.SaveTokensKeepingStorageAsync(authResponse.AccessToken, authResponse.RefreshToken);
            await _authStateProvider.MarkAsAuthenticatedAsync(authResponse.AccessToken);

            return true;
        }
        catch
        {
            await LogoutAsync();
            return false;
        }
    }

    public async Task LogoutAsync()
    {
        try
        {
            var refreshToken = await _tokenStorage.GetRefreshTokenAsync();
            if (!string.IsNullOrWhiteSpace(refreshToken))
            {
                await _httpClient.PostAsJsonAsync("api/auth/logout", new { refreshToken });
            }
        }
        catch { /* Best effort — always clear local state */ }
        finally
        {
            await _tokenStorage.ClearAsync();
            await _authStateProvider.MarkAsLoggedOutAsync();
        }
    }

    /// <summary>
    /// Clears the local session immediately (tokens + auth state) and revokes the
    /// refresh token on the server in the background — lets the caller navigate
    /// to /login without waiting for the network round-trip.
    /// </summary>
    public async Task LogoutImmediateAsync()
    {
        var refreshToken = await _tokenStorage.GetRefreshTokenAsync();

        await _tokenStorage.ClearAsync();
        await _authStateProvider.MarkAsLoggedOutAsync();

        if (!string.IsNullOrWhiteSpace(refreshToken))
        {
            _ = RevokeRefreshTokenInBackgroundAsync(refreshToken);
        }
    }

    private async Task RevokeRefreshTokenInBackgroundAsync(string refreshToken)
    {
        try
        {
            await _httpClient.PostAsJsonAsync("api/auth/logout", new { refreshToken });
        }
        catch { /* Best effort — session is already cleared locally */ }
    }

    public async Task<AuthResult> ForgotPasswordAsync(string email)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync("api/auth/forgot-password", new { email });

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadFromJsonAsync<ErrorResponse>();
                return AuthResult.Fail(GetErrorMessage(error, "Unable to process your request."));
            }

            var result = await response.Content.ReadFromJsonAsync<MessageResponse>();
            return AuthResult.Ok(result?.Message is not null ? ExtractMessage(result.Message) : "If an account exists for this email, a one-time password has been sent.");
        }
        catch (Exception ex)
        {
            return AuthResult.Fail($"An unexpected error occurred: {ex.Message}");
        }
    }

    public async Task<AuthResult> VerifyOtpAsync(string otp)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync("api/auth/verify-otp", new { otp });

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadFromJsonAsync<ErrorResponse>();
                return AuthResult.Fail(GetErrorMessage(error, "The OTP could not be verified."));
            }

            return AuthResult.Ok("OTP verified successfully.");
        }
        catch (Exception ex)
        {
            return AuthResult.Fail($"An unexpected error occurred: {ex.Message}");
        }
    }

    public async Task<AuthResult> ResetPasswordAsync(string otp, string newPassword)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync("api/auth/reset-password", new
            {
                otp,
                newPassword
            });

            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadFromJsonAsync<ErrorResponse>();
                return AuthResult.Fail(GetErrorMessage(error, "Unable to reset your password."));
            }

            var result = await response.Content.ReadFromJsonAsync<MessageResponse>();
            return AuthResult.Ok(result?.Message is not null ? ExtractMessage(result.Message) : "Your password has been reset.");
        }
        catch (Exception ex)
        {
            return AuthResult.Fail($"An unexpected error occurred: {ex.Message}");
        }
    }
}

// Local response models (not shared — Blazor WASM must not reference the Application project)
public record AuthResponse(
    string AccessToken,
    DateTime ExpiresAt,
    string RefreshToken,
    AuthUserInfo User);

public record AuthUserInfo(
    int UserId,
    string FullName,
    string Email,
    string Role,
    List<string> Permissions,
    bool MustChangePassword);

public record ErrorResponse(string? Error, string[]? Errors);

public record MessageResponse(string? Message);

public record AuthResult(bool IsSuccess, string Message, bool MustChangePassword)
{
    public static AuthResult Ok(string message, bool mustChangePassword = false) => new(true, message, mustChangePassword);
    public static AuthResult Fail(string message) => new(false, message, false);
}
