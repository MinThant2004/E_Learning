using System;
using System.ComponentModel.DataAnnotations;

namespace ELearningManagementSystem.Application.Features.Users.DTOs;

public class UserListQuery
{
    [Range(1, int.MaxValue, ErrorMessage = "Page must be at least 1.")]
    public int Page { get; set; } = 1;

    [Range(1, 100, ErrorMessage = "PageSize must be between 1 and 100.")]
    public int PageSize { get; set; } = 10;

    [MaxLength(200, ErrorMessage = "Search term is too long.")]
    public string? SearchTerm { get; set; }

    [MaxLength(100, ErrorMessage = "Role filter is too long.")]
    public string? RoleFilter { get; set; }

    public UserArchiveFilter ArchiveFilter { get; set; } = UserArchiveFilter.Active;
}

public enum UserArchiveFilter
{
    Active,
    Archived,
    All
}
