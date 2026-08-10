using System.ComponentModel.DataAnnotations;

namespace ELearningManagementSystem.Application.Features.Quizzes.DTOs;

public class UpdateQuizRequest
{
    [Required(ErrorMessage = "Title is required.")]
    [MaxLength(200, ErrorMessage = "Title must not exceed 200 characters.")]
    public string Title { get; set; } = string.Empty;

    [Range(0, 100, ErrorMessage = "Passing score must be between 0 and 100.")]
    public int PassingScore { get; set; }
}
