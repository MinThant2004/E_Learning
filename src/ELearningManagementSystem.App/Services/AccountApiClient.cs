using System.Net.Http.Json;
using System.Threading.Tasks;

namespace ELearningManagementSystem.App.Services;

public class AccountApiClient
{
    private readonly HttpClient _httpClient;

    public AccountApiClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<(bool Success, string? Error)> ChangePasswordAsync(ChangePasswordRequest request)
    {
        var response = await _httpClient.PostAsJsonAsync("api/auth/change-password", request);
        if (response.IsSuccessStatusCode)
            return (true, null);

        return (false, await ApiResponseHelper.GetErrorMessageAsync(response, "Unable to change your password."));
    }
}

public class ChangePasswordRequest
{
    public string CurrentPassword { get; set; } = string.Empty;
    public string NewPassword { get; set; } = string.Empty;
    public string ConfirmNewPassword { get; set; } = string.Empty;
}
