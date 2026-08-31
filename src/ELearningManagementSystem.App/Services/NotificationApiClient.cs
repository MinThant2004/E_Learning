using System.Net.Http.Json;
using ELearningManagementSystem.Application.Features.Notifications.DTOs;

namespace ELearningManagementSystem.App.Services;

public class NotificationApiClient
{
    private readonly HttpClient _httpClient;

    public NotificationApiClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<List<UserNotificationResponse>?> GetMyNotificationsAsync()
    {
        return await _httpClient.GetFromJsonAsync<List<UserNotificationResponse>>("api/notifications");
    }

    public async Task<bool> MarkAsReadAsync(int id)
    {
        var response = await _httpClient.PostAsync($"api/notifications/{id}/read", null);
        return response.IsSuccessStatusCode;
    }
}
