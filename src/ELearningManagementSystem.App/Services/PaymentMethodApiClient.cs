using System.IO;
using System.Net.Http.Json;
using ELearningManagementSystem.Application.Features.PaymentMethods.DTOs;

namespace ELearningManagementSystem.App.Services;

public class PaymentMethodApiClient
{
    private readonly HttpClient _httpClient;

    public PaymentMethodApiClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<List<PaymentMethodResponse>?> GetActivePaymentMethodsAsync()
    {
        return await _httpClient.GetFromJsonAsync<List<PaymentMethodResponse>>("api/payment-methods/active");
    }

    public async Task<List<PaymentMethodResponse>?> GetAllPaymentMethodsAdminAsync()
    {
        return await _httpClient.GetFromJsonAsync<List<PaymentMethodResponse>>("api/payment-methods/admin");
    }

    public async Task<HttpResponseMessage> CreatePaymentMethodAsync(CreatePaymentMethodRequest request)
    {
        return await _httpClient.PostAsJsonAsync("api/payment-methods/admin", request);
    }

    public async Task<HttpResponseMessage> UpdatePaymentMethodAsync(int id, UpdatePaymentMethodRequest request)
    {
        return await _httpClient.PutAsJsonAsync($"api/payment-methods/admin/{id}", request);
    }

    public async Task<HttpResponseMessage> DeletePaymentMethodAsync(int id)
    {
        return await _httpClient.DeleteAsync($"api/payment-methods/admin/{id}");
    }

    public async Task<string?> UploadPaymentAssetAsync(Stream fileStream, string fileName)
    {
        using var content = new MultipartFormDataContent();
        using var streamContent = new StreamContent(fileStream);
        content.Add(streamContent, "file", fileName);

        var response = await _httpClient.PostAsync("api/payment-methods/upload-asset", content);
        if (response.IsSuccessStatusCode)
        {
            var res = await response.Content.ReadFromJsonAsync<UploadAssetResponse>();
            return res?.Url;
        }
        return null;
    }

    private class UploadAssetResponse
    {
        public string Url { get; set; } = string.Empty;
    }
}
