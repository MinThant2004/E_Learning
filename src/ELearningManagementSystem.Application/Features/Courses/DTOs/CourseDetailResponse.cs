using System;

namespace ELearningManagementSystem.Application.Features.Courses.DTOs;

public class CourseDetailResponse
{
    public int CourseId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int CategoryId { get; set; }
    public bool Status { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public int CreatedBy { get; set; }
    public string? ThumbnailUrl { get; set; }
    public bool DeleteFlag { get; set; }
}
