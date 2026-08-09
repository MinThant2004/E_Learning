using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace ELearningManagementSystem.App.Services;

public record ApiErrorResponse(string? Error, string[]? Errors);

public static class ApiResponseHelper
{
    public static async Task<string> GetErrorMessageAsync(HttpResponseMessage response, string fallback = "An error occurred.")
    {
        if (response == null) return fallback;

        try
        {
            var apiError = await response.Content.ReadFromJsonAsync<ApiErrorResponse>();
            if (apiError != null)
            {
                if (!string.IsNullOrWhiteSpace(apiError.Error))
                    return apiError.Error;
                if (apiError.Errors != null && apiError.Errors.Length > 0)
                    return string.Join(" ", apiError.Errors);
            }
        }
        catch
        {
            // Ignore parse errors and fallback
        }

        return $"Server error ({response.StatusCode})";
    }
}
