namespace ELearningManagementSystem.Application.Features.LessonProgress.DTOs;

public class CourseProgressResponse
{
    public int CompletedCount { get; set; }
    public int TotalActiveLessons { get; set; }
    public double Percentage => TotalActiveLessons == 0 ? 0 : Math.Round((double)CompletedCount / TotalActiveLessons * 100, 2);
    public bool IsCourseComplete => TotalActiveLessons > 0 && CompletedCount >= TotalActiveLessons;
    
    public IEnumerable<LessonProgressResponse> Lessons { get; set; } = new List<LessonProgressResponse>();
}
