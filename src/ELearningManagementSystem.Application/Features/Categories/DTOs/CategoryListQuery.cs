using System;
using System.ComponentModel.DataAnnotations;

namespace ELearningManagementSystem.Application.Features.Categories.DTOs;

public class CategoryListQuery
{
    [Range(1, int.MaxValue, ErrorMessage = "Page must be at least 1.")]
    public int Page { get; set; } = 1;

    [Range(1, 100, ErrorMessage = "PageSize must be between 1 and 100.")]
    public int PageSize { get; set; } = 10;

    [MaxLength(200, ErrorMessage = "Search term is too long.")]
    public string? SearchTerm { get; set; }

    public bool? IsArchived { get; set; }
}
