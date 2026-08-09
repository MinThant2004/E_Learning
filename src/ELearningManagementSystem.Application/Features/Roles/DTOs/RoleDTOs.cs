using System;
using System.Collections.Generic;

namespace ELearningManagementSystem.Application.Features.Roles.DTOs;

public class RoleListQuery
{
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public string? SearchTerm { get; set; }
}

public class RoleResponse
{
    public int RoleId { get; set; }
    public string RoleName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class RoleDetailResponse : RoleResponse
{
    public List<int> PermissionIds { get; set; } = new();
}

public class CreateRoleRequest
{
    public string RoleName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public List<int> PermissionIds { get; set; } = new();
}

public class UpdateRoleRequest
{
    public string RoleName { get; set; } = string.Empty;
    public string? Description { get; set; }
}

public class AssignPermissionsRequest
{
    public List<int> PermissionIds { get; set; } = new();
}
