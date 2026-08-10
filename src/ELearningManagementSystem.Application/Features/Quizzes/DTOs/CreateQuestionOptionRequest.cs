using System.ComponentModel.DataAnnotations;

namespace ELearningManagementSystem.Application.Features.Quizzes.DTOs;

public class CreateQuestionOptionRequest
{
    [Required(ErrorMessage = "Option Text is required.")]
    public string OptionText { get; set; } = string.Empty;

    public bool IsCorrect { get; set; }
}
