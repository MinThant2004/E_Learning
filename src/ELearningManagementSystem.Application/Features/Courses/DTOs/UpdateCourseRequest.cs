namespace ELearningManagementSystem.Application.Features.Courses.DTOs;

public class UpdateCourseRequest
{
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int CategoryId { get; set; }
    public string? ThumbnailUrl { get; set; }
    public bool RemoveThumbnail { get; set; }
}
