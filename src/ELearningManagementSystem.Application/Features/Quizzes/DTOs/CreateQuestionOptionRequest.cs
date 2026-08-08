namespace ELearningManagementSystem.Application.Features.Quizzes.DTOs;

public class CreateQuestionOptionRequest
{
    public string OptionText { get; set; } = string.Empty;
    public bool IsCorrect { get; set; }
}
