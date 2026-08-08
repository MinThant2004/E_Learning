using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components.Authorization;

namespace ELearningManagementSystem.App.Services;

public class CustomAuthStateProvider : AuthenticationStateProvider
{
    private readonly ILocalStorageService _localStorage;
    private readonly ClaimsPrincipal _anonymous = new(new ClaimsIdentity());

    public CustomAuthStateProvider(ILocalStorageService localStorage)
    {
        _localStorage = localStorage;
    }

    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        try
        {
            var token = await _localStorage.GetItemAsStringAsync("authToken");

            if (string.IsNullOrWhiteSpace(token))
                return new AuthenticationState(_anonymous);

            var handler = new JwtSecurityTokenHandler();

            if (!handler.CanReadToken(token))
            {
                await ClearAuthAsync();
                return new AuthenticationState(_anonymous);
            }

            var jwtToken = handler.ReadJwtToken(token);

            if (jwtToken.ValidTo < DateTime.UtcNow)
            {
                // Token is expired — caller (AuthHttpHandler) will attempt refresh
                await ClearAuthAsync();
                return new AuthenticationState(_anonymous);
            }

            var identity = new ClaimsIdentity(jwtToken.Claims, "jwt");
            var user = new ClaimsPrincipal(identity);

            return new AuthenticationState(user);
        }
        catch
        {
            return new AuthenticationState(_anonymous);
        }
    }

    public async Task MarkAsAuthenticatedAsync(string accessToken)
    {
        await _localStorage.SetItemAsStringAsync("authToken", accessToken);
        NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
    }

    public async Task MarkAsLoggedOutAsync()
    {
        await ClearAuthAsync();
        NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(_anonymous)));
    }

    private async Task ClearAuthAsync()
    {
        await _localStorage.RemoveItemAsync("authToken");
    }

    // Keep backward-compat shim
    public void NotifyAuthStateChanged() =>
        NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
}
