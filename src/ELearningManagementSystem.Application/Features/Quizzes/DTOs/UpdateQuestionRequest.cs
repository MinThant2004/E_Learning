using System.ComponentModel.DataAnnotations;

namespace ELearningManagementSystem.Application.Features.Quizzes.DTOs;

public class UpdateQuestionRequest
{
    [Required(ErrorMessage = "Question Text is required.")]
    public string QuestionText { get; set; } = string.Empty;

    [Range(0, int.MaxValue, ErrorMessage = "Display order must be non-negative.")]
    public int DisplayOrder { get; set; }
}
