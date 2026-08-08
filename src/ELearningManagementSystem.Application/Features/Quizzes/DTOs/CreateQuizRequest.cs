namespace ELearningManagementSystem.Application.Features.Quizzes.DTOs;

public class CreateQuizRequest
{
    public int CourseId { get; set; }
    public string Title { get; set; } = string.Empty;
    public int PassingScore { get; set; }
}
