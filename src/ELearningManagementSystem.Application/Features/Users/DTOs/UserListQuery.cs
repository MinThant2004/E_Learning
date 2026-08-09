using System;

namespace ELearningManagementSystem.Application.Features.Users.DTOs;

public class UserListQuery
{
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public string? SearchTerm { get; set; }
    public string? RoleFilter { get; set; }
    public UserArchiveFilter ArchiveFilter { get; set; } = UserArchiveFilter.Active;
}

public enum UserArchiveFilter
{
    Active,
    Archived,
    All
}
