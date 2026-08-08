using System;

namespace ELearningManagementSystem.Application.Features.Lessons.DTOs;

public class LessonDetailResponse
{
    public int LessonId { get; set; }
    public int CourseId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public int DisplayOrder { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public bool DeleteFlag { get; set; }
}
