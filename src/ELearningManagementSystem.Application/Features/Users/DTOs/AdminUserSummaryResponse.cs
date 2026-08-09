using System;
using System.Collections.Generic;

namespace ELearningManagementSystem.Application.Features.Users.DTOs;

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
