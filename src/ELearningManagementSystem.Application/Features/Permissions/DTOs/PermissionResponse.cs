using System.Collections.Generic;

namespace ELearningManagementSystem.Application.Features.Permissions.DTOs;

public class PermissionResponse
{
    public int PermissionId { get; set; }
    public string PermissionCode { get; set; } = string.Empty;
    public string PermissionName { get; set; } = string.Empty;
    public string Module { get; set; } = string.Empty;
}
