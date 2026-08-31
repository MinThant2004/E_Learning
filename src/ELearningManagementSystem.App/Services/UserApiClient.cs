using System.Net.Http.Json;
using System.Text.Json;



namespace ELearningManagementSystem.App.Services;

public class UserApiClient
{
    private readonly HttpClient _httpClient;

    public UserApiClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<PagedResult<AdminUserSummaryResponse>?> GetUsersAsync(UserListQuery query)
    {
        var queryString = $"?page={query.Page}&pageSize={query.PageSize}&archiveFilter={query.ArchiveFilter}";
        if (!string.IsNullOrEmpty(query.SearchTerm))
        {
            queryString += $"&searchTerm={Uri.EscapeDataString(query.SearchTerm)}";
        }
        if (!string.IsNullOrEmpty(query.RoleFilter))
        {
            queryString += $"&roleFilter={Uri.EscapeDataString(query.RoleFilter)}";
        }
        
        return await _httpClient.GetFromJsonAsync<PagedResult<AdminUserSummaryResponse>>($"api/users{queryString}");
    }

    public async Task<List<string>?> GetAvailableRolesAsync()
    {
        return await _httpClient.GetFromJsonAsync<List<string>>("api/users/roles");
    }

    public async Task<AdminUserDetailResponse?> GetUserByIdAsync(int id)
    {
        var response = await _httpClient.GetAsync($"api/users/{id}");
        if (!response.IsSuccessStatusCode)
            return null;

        return await response.Content.ReadFromJsonAsync<AdminUserDetailResponse>();
    }

    public async Task<(bool Success, string? Error)> CreateUserAsync(CreateUserRequest request)
    {
        var response = await _httpClient.PostAsJsonAsync("api/users", request);
        if (response.IsSuccessStatusCode)
            return (true, null);

        return (false, await ReadErrorAsync(response));
    }

    public async Task<(bool Success, string? Error)> UpdateUserAsync(int id, UpdateUserRequest request)
    {
        var response = await _httpClient.PutAsJsonAsync($"api/users/{id}", request);
        if (response.IsSuccessStatusCode)
            return (true, null);

        return (false, await ReadErrorAsync(response));
    }

    public async Task<(bool Success, string? Error)> ArchiveUserAsync(int id)
    {
        var response = await _httpClient.PostAsync($"api/users/{id}/archive", null);
        if (response.IsSuccessStatusCode)
            return (true, null);

        return (false, await ReadErrorAsync(response));
    }

    public async Task<(bool Success, string? Error)> RestoreUserAsync(int id)
    {
        var response = await _httpClient.PostAsync($"api/users/{id}/restore", null);
        if (response.IsSuccessStatusCode)
            return (true, null);

        return (false, await ReadErrorAsync(response));
    }

    private static async Task<string?> ReadErrorAsync(HttpResponseMessage response)
    {
        var content = await response.Content.ReadAsStringAsync();
        if (string.IsNullOrWhiteSpace(content))
            return null;

        try
        {
            using var document = JsonDocument.Parse(content);
            if (document.RootElement.TryGetProperty("error", out var error))
                return error.GetString();
            if (document.RootElement.TryGetProperty("errors", out var errors))
            {
                if (errors.ValueKind == JsonValueKind.Array)
                    return string.Join(" ", errors.EnumerateArray().Select(x => x.GetString()));
                return errors.GetString();
            }
            if (document.RootElement.TryGetProperty("Errors", out var errorsUpper))
            {
                if (errorsUpper.ValueKind == JsonValueKind.Array)
                    return string.Join(" ", errorsUpper.EnumerateArray().Select(x => x.GetString()));
                return errorsUpper.GetString();
            }
        }
        catch (JsonException)
        {
            // Keep compatibility with any non-JSON error response.
        }

        return content;
    }
}

public class UserListQuery
{
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public string? SearchTerm { get; set; }
    public string? RoleFilter { get; set; }
    public UserArchiveFilter ArchiveFilter { get; set; } = UserArchiveFilter.Active;
}

public enum UserArchiveFilter
{
    Active,
    Archived,
    All
}

public class AdminUserSummaryResponse
{
    public int UserId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public bool Status { get; set; }
    public bool IsArchived { get; set; }
    public DateTime CreatedAt { get; set; }
    public List<string> Roles { get; set; } = new();
}

public class AdminUserDetailResponse
{
    public int UserId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public bool Status { get; set; }
    public bool IsArchived { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public List<UserRoleSummaryResponse> Roles { get; set; } = new();
}

public class UserRoleSummaryResponse
{
    public string RoleName { get; set; } = string.Empty;
    public DateTime AssignedAt { get; set; }
    public string? AssignedBy { get; set; }
}

public class UpdateUserRequest
{
    public string FullName { get; set; } = string.Empty;
    public bool Status { get; set; }
    public List<string> Roles { get; set; } = new();
}

public class CreateUserRequest
{
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public List<string> Roles { get; set; } = new();
    public string Password { get; set; } = string.Empty;
    public string ConfirmPassword { get; set; } = string.Empty;
}
