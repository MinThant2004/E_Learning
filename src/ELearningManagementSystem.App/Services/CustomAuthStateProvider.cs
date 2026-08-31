using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Components.Authorization;

namespace ELearningManagementSystem.App.Services;

public class CustomAuthStateProvider : AuthenticationStateProvider
{
    private readonly TokenStorageService _tokenStorage;
    private readonly ClaimsPrincipal _anonymous = new(new ClaimsIdentity());

    public CustomAuthStateProvider(TokenStorageService tokenStorage)
    {
        _tokenStorage = tokenStorage;
    }

    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        try
        {
            var token = await _tokenStorage.GetAccessTokenAsync();

            if (string.IsNullOrWhiteSpace(token))
                return new AuthenticationState(_anonymous);

            var handler = new JwtSecurityTokenHandler();

            if (!handler.CanReadToken(token))
            {
                await _tokenStorage.RemoveAccessTokenAsync();
                return new AuthenticationState(_anonymous);
            }

            var jwtToken = handler.ReadJwtToken(token);

            if (jwtToken.ValidTo < DateTime.UtcNow)
            {
                // Token is expired — caller (AuthHttpHandler) will attempt refresh
                await _tokenStorage.RemoveAccessTokenAsync();
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
        await _tokenStorage.SaveAccessTokenAsync(accessToken);
        NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
    }

    public async Task MarkAsLoggedOutAsync()
    {
        await _tokenStorage.ClearAsync();
        NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(_anonymous)));
    }

    // Keep backward-compat shim
    public void NotifyAuthStateChanged() =>
        NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
}