using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace ELearningManagementSystem.App.Services
{
    public class AdminDashboardApiClient
    {
        private readonly HttpClient _httpClient;

        public AdminDashboardApiClient(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<(bool Success, AdminDashboardResponse? Data, string? Error)> GetDashboardAsync()
        {
            var response = await _httpClient.GetAsync("api/admin/dashboard");
            if (response.IsSuccessStatusCode)
            {
                var data = await response.Content.ReadFromJsonAsync<AdminDashboardResponse>(new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                return (true, data, null);
            }
            
            var error = await ApiResponseHelper.GetErrorMessageAsync(response);
            return (false, null, error);
        }
    }

    public class AdminDashboardResponse
    {
        public Dictionary<string, ContentMetricResponse> Metrics { get; set; } = new();
        public List<AdminQuickActionResponse> QuickActions { get; set; } = new();
    }

    public class ContentMetricResponse
    {
        public string Title { get; set; } = string.Empty;
        public string Icon { get; set; } = string.Empty;
        public int ActiveCount { get; set; }
        public int ArchivedCount { get; set; }
        public int TotalCount => ActiveCount + ArchivedCount;
        public string Route { get; set; } = string.Empty;
    }

    public class AdminQuickActionResponse
    {
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Route { get; set; } = string.Empty;
        public string Icon { get; set; } = string.Empty;
    }
}
