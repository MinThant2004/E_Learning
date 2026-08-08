namespace ELearningManagementSystem.Application.Features.LessonProgress.DTOs;

public class LessonProgressResponse
{
    public int LessonId { get; set; }
    public bool Completed { get; set; }
    public DateTime? CompletedDate { get; set; }
}
