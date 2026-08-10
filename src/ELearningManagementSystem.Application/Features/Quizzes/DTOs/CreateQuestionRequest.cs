using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;

namespace ELearningManagementSystem.Application.Features.Quizzes.DTOs;

public class CreateQuestionRequest
{
    [Range(1, int.MaxValue, ErrorMessage = "Valid QuizId is required.")]
    public int QuizId { get; set; }

    [Required(ErrorMessage = "Question Text is required.")]
    public string QuestionText { get; set; } = string.Empty;

    [Range(0, int.MaxValue, ErrorMessage = "Display order must be non-negative.")]
    public int DisplayOrder { get; set; }

    [Required(ErrorMessage = "Options are required.")]
    public List<CreateQuestionOptionRequest> Options { get; set; } = new();
}
