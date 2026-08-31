using System.Net;
using System.Net.Http.Headers;

namespace ELearningManagementSystem.App.Services;

/// <summary>
/// Delegating handler that:
/// 1. Attaches the stored access token as Authorization: Bearer header.
/// 2. On HTTP 401, attempts exactly ONE token refresh and retries the request once.
/// 3. If refresh fails, clears auth state and does NOT retry again (no infinite loop).
/// </summary>
public class AuthHttpHandler : DelegatingHandler
{
    private readonly TokenStorageService _tokenStorage;
    private readonly AuthApiService _authApiService;

    public AuthHttpHandler(TokenStorageService tokenStorage, AuthApiService authApiService)
    {
        _tokenStorage = tokenStorage;
        _authApiService = authApiService;
    }

    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        // Attach current access token
        var token = await _tokenStorage.GetAccessTokenAsync();
        if (!string.IsNullOrWhiteSpace(token))
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        var response = await base.SendAsync(request, cancellationToken);

        // One-shot refresh on 401
        if (response.StatusCode == HttpStatusCode.Unauthorized)
        {
            var refreshed = await _authApiService.RefreshAccessTokenAsync();
            if (refreshed)
            {
                // Retry original request once with new token
                var newToken = await _tokenStorage.GetAccessTokenAsync();
                var retryRequest = await CloneRequestAsync(request);
                if (!string.IsNullOrWhiteSpace(newToken))
                    retryRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", newToken);

                response = await base.SendAsync(retryRequest, cancellationToken);
            }
            // If refresh failed, _authApiService.RefreshAccessTokenAsync already called LogoutAsync
        }

        return response;
    }

    private static async Task<HttpRequestMessage> CloneRequestAsync(HttpRequestMessage original)
    {
        var clone = new HttpRequestMessage(original.Method, original.RequestUri);

        foreach (var header in original.Headers)
            clone.Headers.TryAddWithoutValidation(header.Key, header.Value);

        if (original.Content is not null)
        {
            var content = await original.Content.ReadAsByteArrayAsync();
            clone.Content = new ByteArrayContent(content);
            foreach (var header in original.Content.Headers)
                clone.Content.Headers.TryAddWithoutValidation(header.Key, header.Value);
        }

        return clone;
    }
}
