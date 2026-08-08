namespace ELearningManagementSystem.Application.Features.Courses.DTOs;

public class CreateCourseRequest
{
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int CategoryId { get; set; }
}
