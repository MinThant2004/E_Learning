namespace ELearningManagementSystem.Application.Features.Categories.DTOs;

public class CreateCategoryRequest
{
    public string CategoryName { get; set; } = string.Empty;
    public string? Description { get; set; }
}
