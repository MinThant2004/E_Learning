using System.ComponentModel.DataAnnotations;

namespace ELearningManagementSystem.Application.Features.Courses.DTOs;

public class UpdateCourseRequest
{
    [Required(ErrorMessage = "Title is required.")]
    [MaxLength(200, ErrorMessage = "Title must not exceed 200 characters.")]
    public string Title { get; set; } = string.Empty;

    public string? Description { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Please select a category.")]
    public int CategoryId { get; set; }

    [MaxLength(500, ErrorMessage = "Thumbnail URL must not exceed 500 characters.")]
    public string? ThumbnailUrl { get; set; }

    public bool RemoveThumbnail { get; set; }

    /// <summary>Base64-encoded RowVersion for optimistic concurrency.</summary>
    public string? RowVersion { get; set; }
}
