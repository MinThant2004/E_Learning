using System.Collections.Generic;

namespace ELearningManagementSystem.Application.Features.Reports.DTOs;

public class AuditActivityFilterOptionsDto
{
    public List<ReportUserDto> Users { get; set; } = new();
    public List<string> Actions { get; set; } = new();
    public List<string> TableNames { get; set; } = new();
}

public class ReportUserDto
{
    public int UserId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
}
