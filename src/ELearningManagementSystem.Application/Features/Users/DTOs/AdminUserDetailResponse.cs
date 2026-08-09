using System;
using System.Collections.Generic;

namespace ELearningManagementSystem.Application.Features.Users.DTOs;

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
