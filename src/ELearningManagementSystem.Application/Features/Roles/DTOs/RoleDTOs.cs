using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ELearningManagementSystem.Application.Features.Roles.DTOs;

public class RoleListQuery
{
    [Range(1, int.MaxValue)]
    public int Page { get; set; } = 1;

    [Range(1, 100)]
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
    [Required(ErrorMessage = "Role name is required.")]
    [MaxLength(100, ErrorMessage = "Role name must not exceed 100 characters.")]
    public string RoleName { get; set; } = string.Empty;

    [MaxLength(255, ErrorMessage = "Description must not exceed 255 characters.")]
    public string? Description { get; set; }

    public List<int> PermissionIds { get; set; } = new();
}

public class UpdateRoleRequest
{
    [Required(ErrorMessage = "Role name is required.")]
    [MaxLength(100, ErrorMessage = "Role name must not exceed 100 characters.")]
    public string RoleName { get; set; } = string.Empty;

    [MaxLength(255, ErrorMessage = "Description must not exceed 255 characters.")]
    public string? Description { get; set; }
}

public class AssignPermissionsRequest
{
    [Required(ErrorMessage = "PermissionIds list is required.")]
    public List<int> PermissionIds { get; set; } = new();
}
