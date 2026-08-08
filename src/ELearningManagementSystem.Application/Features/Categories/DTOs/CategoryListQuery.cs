using System;

namespace ELearningManagementSystem.Application.Features.Categories.DTOs;

public class CategoryListQuery
{
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public string? SearchTerm { get; set; }
    public bool? IsArchived { get; set; }
}
