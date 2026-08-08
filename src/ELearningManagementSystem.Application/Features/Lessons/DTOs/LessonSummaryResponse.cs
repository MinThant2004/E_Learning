namespace ELearningManagementSystem.Application.Features.Lessons.DTOs;

public class LessonSummaryResponse
{
    public int LessonId { get; set; }
    public int CourseId { get; set; }
    public string Title { get; set; } = string.Empty;
    public int DisplayOrder { get; set; }
    public bool DeleteFlag { get; set; }
}
