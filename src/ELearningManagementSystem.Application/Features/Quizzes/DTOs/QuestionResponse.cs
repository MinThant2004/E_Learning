namespace ELearningManagementSystem.Application.Features.Quizzes.DTOs;

public class QuestionResponse
{
    public int QuestionId { get; set; }
    public int QuizId { get; set; }
    public string QuestionText { get; set; } = string.Empty;
    public int DisplayOrder { get; set; }
    public List<QuestionOptionResponse> Options { get; set; } = new();
}
