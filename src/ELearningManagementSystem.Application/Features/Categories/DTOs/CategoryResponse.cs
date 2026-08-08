namespace ELearningManagementSystem.Application.Features.Categories.DTOs;

public class CategoryResponse
{
    public int CategoryId { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public string? Description { get; set; }
    public System.DateTime CreatedAt { get; set; }
    public bool DeleteFlag { get; set; }
}
