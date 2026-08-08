using System.Collections.Generic;
using System.Net.Http.Json;
using Blazored.LocalStorage;

namespace ELearningManagementSystem.App.Services;

public class AuthApiService
{
    private readonly HttpClient _httpClient;
    private readonly ILocalStorageService _localStorage;
    private readonly CustomAuthStateProvider _authStateProvider;

    public AuthApiService(
        HttpClient httpClient,
        ILocalStorageService localStorage,
        CustomAuthStateProvider authStateProvider)
    {
        _httpClient = httpClient;
        _localStorage = localStorage;
        _authStateProvider = authStateProvider;
    }

    private string GetErrorMessage(ErrorResponse? error, string fallback)
    {
        if (error is null) return fallback;
        if (!string.IsNullOrWhiteSpace(error.Error)) return error.Error;
        if (error.Errors is not null && error.Errors.Length > 0) return string.Join(" ", error.Errors);
        return fallback;
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

    public async Task<AuthResult> LoginAsync(string email, string password)
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

            // Store tokens
            await _localStorage.SetItemAsStringAsync("authToken", authResponse.AccessToken);
            await _localStorage.SetItemAsStringAsync("refreshToken", authResponse.RefreshToken);

            await _authStateProvider.MarkAsAuthenticatedAsync(authResponse.AccessToken);

            return AuthResult.Ok("Login successful.");
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
            var refreshToken = await _localStorage.GetItemAsStringAsync("refreshToken");
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

            await _localStorage.SetItemAsStringAsync("authToken", authResponse.AccessToken);
            await _localStorage.SetItemAsStringAsync("refreshToken", authResponse.RefreshToken);
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
            var refreshToken = await _localStorage.GetItemAsStringAsync("refreshToken");
            if (!string.IsNullOrWhiteSpace(refreshToken))
            {
                await _httpClient.PostAsJsonAsync("api/auth/logout", new { refreshToken });
            }
        }
        catch { /* Best effort — always clear local state */ }
        finally
        {
            await _localStorage.RemoveItemAsync("authToken");
            await _localStorage.RemoveItemAsync("refreshToken");
            await _authStateProvider.MarkAsLoggedOutAsync();
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
    List<string> Permissions);

public record ErrorResponse(string? Error, string[]? Errors);

public record AuthResult(bool IsSuccess, string Message)
{
    public static AuthResult Ok(string message) => new(true, message);
    public static AuthResult Fail(string message) => new(false, message);
}
