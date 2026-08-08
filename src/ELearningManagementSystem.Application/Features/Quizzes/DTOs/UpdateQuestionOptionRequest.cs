namespace ELearningManagementSystem.Application.Features.Quizzes.DTOs;

public class UpdateQuestionOptionRequest
{
    public int QuestionId { get; set; }
    public string OptionText { get; set; } = string.Empty;
    public bool IsCorrect { get; set; }
}
