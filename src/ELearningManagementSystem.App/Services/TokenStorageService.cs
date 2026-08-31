using Blazored.LocalStorage;
using Blazored.SessionStorage;

namespace ELearningManagementSystem.App.Services;

/// <summary>
/// Stores auth tokens either persistently (localStorage, "Remember me") or
/// for the current tab/session only (sessionStorage). Reads fall back to the
/// other store so existing sessions keep working regardless of choice.
/// </summary>
public class TokenStorageService
{
    private readonly ILocalStorageService _localStorage;
    private readonly ISessionStorageService _sessionStorage;

    public TokenStorageService(ILocalStorageService localStorage, ISessionStorageService sessionStorage)
    {
        _localStorage = localStorage;
        _sessionStorage = sessionStorage;
    }

    public async Task<string?> GetAccessTokenAsync()
    {
        var token = await _localStorage.GetItemAsStringAsync("authToken");
        if (!string.IsNullOrWhiteSpace(token)) return token;
        return await _sessionStorage.GetItemAsStringAsync("authToken");
    }

    public async Task<string?> GetRefreshTokenAsync()
    {
        var token = await _localStorage.GetItemAsStringAsync("refreshToken");
        if (!string.IsNullOrWhiteSpace(token)) return token;
        return await _sessionStorage.GetItemAsStringAsync("refreshToken");
    }

    public async Task SaveTokensAsync(string accessToken, string refreshToken, bool rememberMe)
    {
        if (rememberMe)
        {
            await _localStorage.SetItemAsStringAsync("authToken", accessToken);
            await _localStorage.SetItemAsStringAsync("refreshToken", refreshToken);
            await _sessionStorage.RemoveItemAsync("authToken");
            await _sessionStorage.RemoveItemAsync("refreshToken");
        }
        else
        {
            await _sessionStorage.SetItemAsStringAsync("authToken", accessToken);
            await _sessionStorage.SetItemAsStringAsync("refreshToken", refreshToken);
            await _localStorage.RemoveItemAsync("authToken");
            await _localStorage.RemoveItemAsync("refreshToken");
        }
    }

    /// <summary>Saves tokens back into whichever storage currently holds them (used on refresh).</summary>
    public async Task SaveTokensKeepingStorageAsync(string accessToken, string refreshToken)
    {
        var inSession = await _sessionStorage.GetItemAsStringAsync("refreshToken") is not null;
        var inLocal = await _localStorage.GetItemAsStringAsync("refreshToken") is not null;
        await SaveTokensAsync(accessToken, refreshToken, rememberMe: !inSession || inLocal);
    }

    public async Task SaveAccessTokenAsync(string accessToken)
    {
        var sessionRefresh = await _sessionStorage.GetItemAsStringAsync("refreshToken") is not null;
        var localRefresh = await _localStorage.GetItemAsStringAsync("refreshToken") is not null;
        if (sessionRefresh && !localRefresh)
            await _sessionStorage.SetItemAsStringAsync("authToken", accessToken);
        else
            await _localStorage.SetItemAsStringAsync("authToken", accessToken);
    }

    public async Task RemoveAccessTokenAsync()
    {
        await _localStorage.RemoveItemAsync("authToken");
        await _sessionStorage.RemoveItemAsync("authToken");
    }

    public async Task ClearAsync()
    {
        await _localStorage.RemoveItemAsync("authToken");
        await _localStorage.RemoveItemAsync("refreshToken");
        await _sessionStorage.RemoveItemAsync("authToken");
        await _sessionStorage.RemoveItemAsync("refreshToken");
    }
}