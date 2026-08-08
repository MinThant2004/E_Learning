namespace ELearningManagementSystem.Application.Features.Quizzes.DTOs;

public class CreateQuestionRequest
{
    public int QuizId { get; set; }
    public string QuestionText { get; set; } = string.Empty;
    public int DisplayOrder { get; set; }
    public List<CreateQuestionOptionRequest> Options { get; set; } = new();
}
