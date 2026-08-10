using System.ComponentModel.DataAnnotations;

namespace ELearningManagementSystem.Application.Features.Lessons.DTOs;

public class UpdateLessonRequest
{
    [Required(ErrorMessage = "Title is required.")]
    [MaxLength(200, ErrorMessage = "Title must not exceed 200 characters.")]
    public string Title { get; set; } = string.Empty;

    [Required(ErrorMessage = "Content is required.")]
    public string Content { get; set; } = string.Empty;

    [Range(0, int.MaxValue, ErrorMessage = "Display order must be non-negative.")]
    public int DisplayOrder { get; set; }
}
