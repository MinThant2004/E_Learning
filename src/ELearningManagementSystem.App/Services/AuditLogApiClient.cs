using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace ELearningManagementSystem.App.Services;

public class AuditLogApiClient
{
    private readonly HttpClient _httpClient;

    public AuditLogApiClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<PagedResult<ClientAuditLogResponse>?> GetAuditLogsAsync(
        int page = 1, 
        int pageSize = 10, 
        string? searchTerm = null, 
        string? actionFilter = null, 
        string? tableNameFilter = null, 
        int? userIdFilter = null, 
        DateTime? startDate = null, 
        DateTime? endDate = null)
    {
        var url = $"api/audit-logs?page={page}&pageSize={pageSize}";
        if (!string.IsNullOrEmpty(searchTerm)) url += $"&searchTerm={Uri.EscapeDataString(searchTerm)}";
        if (!string.IsNullOrEmpty(actionFilter)) url += $"&actionFilter={Uri.EscapeDataString(actionFilter)}";
        if (!string.IsNullOrEmpty(tableNameFilter)) url += $"&tableNameFilter={Uri.EscapeDataString(tableNameFilter)}";
        if (userIdFilter.HasValue) url += $"&userIdFilter={userIdFilter.Value}";
        if (startDate.HasValue) url += $"&startDate={Uri.EscapeDataString(startDate.Value.ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ssZ"))}";
        if (endDate.HasValue) url += $"&endDate={Uri.EscapeDataString(endDate.Value.ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ssZ"))}";

        return await _httpClient.GetFromJsonAsync<PagedResult<ClientAuditLogResponse>>(url);
    }

    public async Task<ClientAuditLogResponse?> GetAuditLogAsync(int id)
    {
        return await _httpClient.GetFromJsonAsync<ClientAuditLogResponse>($"api/audit-logs/{id}");
    }
}

public class ClientAuditLogResponse
{
    public int AuditLogId { get; set; }
    public int UserId { get; set; }
    public string UserEmail { get; set; } = string.Empty;
    public string UserFullName { get; set; } = string.Empty;
    public string Action { get; set; } = string.Empty;
    public string TableName { get; set; } = string.Empty;
    public int RecordId { get; set; }
    public DateTime CreatedAt { get; set; }
    public string? RecordName { get; set; }
    public List<ClientAuditLogChange>? Changes { get; set; }
}

public class ClientAuditLogChange
{
    public string Field { get; set; } = string.Empty;
    public string? OldValue { get; set; }
    public string? NewValue { get; set; }
}
