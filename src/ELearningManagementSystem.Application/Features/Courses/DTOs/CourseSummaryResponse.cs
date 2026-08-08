namespace ELearningManagementSystem.Application.Features.Courses.DTOs;

public class CourseSummaryResponse
{
    public int CourseId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int CategoryId { get; set; }
    public bool Status { get; set; }
}
