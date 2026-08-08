namespace ELearningManagementSystem.Application.Features.Courses.DTOs;

public class CourseListQuery
{
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public string? SearchTerm { get; set; }
    public bool IncludeDeleted { get; set; } = false;
}
