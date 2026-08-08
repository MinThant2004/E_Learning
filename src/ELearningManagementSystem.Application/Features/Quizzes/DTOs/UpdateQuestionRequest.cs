namespace ELearningManagementSystem.Application.Features.Quizzes.DTOs;

public class UpdateQuestionRequest
{
    public string QuestionText { get; set; } = string.Empty;
    public int DisplayOrder { get; set; }
}
