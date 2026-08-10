using System.ComponentModel.DataAnnotations;

namespace ELearningManagementSystem.Application.Features.Quizzes.DTOs;

public class UpdateQuestionOptionRequest
{
    [Range(1, int.MaxValue, ErrorMessage = "Valid QuestionId is required.")]
    public int QuestionId { get; set; }

    [Required(ErrorMessage = "Option Text is required.")]
    public string OptionText { get; set; } = string.Empty;

    public bool IsCorrect { get; set; }
}
