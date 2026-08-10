using System.ComponentModel.DataAnnotations;

namespace ELearningManagementSystem.Application.Features.Categories.DTOs;

public class UpdateCategoryRequest
{
    [Required(ErrorMessage = "Category Name is required.")]
    [MaxLength(100, ErrorMessage = "Category Name must not exceed 100 characters.")]
    public string CategoryName { get; set; } = string.Empty;

    [MaxLength(255, ErrorMessage = "Description must not exceed 255 characters.")]
    public string? Description { get; set; }
}
