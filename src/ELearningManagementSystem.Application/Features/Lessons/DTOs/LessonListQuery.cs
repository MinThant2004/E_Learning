using System;

namespace ELearningManagementSystem.Application.Features.Lessons.DTOs;

public class LessonListQuery
{
    public int CourseId { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public string? SearchTerm { get; set; }
    public bool? IsArchived { get; set; }
}
