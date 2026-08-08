namespace ELearningManagementSystem.Application.Features.Quizzes.DTOs;

public class UpdateQuizRequest
{
    public string Title { get; set; } = string.Empty;
    public int PassingScore { get; set; }
}
