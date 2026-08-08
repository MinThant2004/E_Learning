namespace ELearningManagementSystem.Application.Features.Lessons.DTOs;

public class UpdateLessonRequest
{
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public int DisplayOrder { get; set; }
}
