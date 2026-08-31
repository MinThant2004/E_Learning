namespace ELearningManagementSystem.Application.Features.CourseExams.DTOs;

public class CourseExamResponse
{
    public int ExamId { get; set; }
    public int CourseId { get; set; }
    public string CourseTitle { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal ExamFee { get; set; }
    public int QuestionCount { get; set; }
    public int DurationMinutes { get; set; }
    public int PassingScore { get; set; }
    public int MaxAttempts { get; set; }
    public int PoolQuestionCount { get; set; }
    public bool Status { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public bool DeleteFlag { get; set; }
}

public class CreateCourseExamRequest
{
    public int CourseId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal ExamFee { get; set; }
    public int QuestionCount { get; set; }
    public int DurationMinutes { get; set; }
    public int PassingScore { get; set; }
    public int MaxAttempts { get; set; } = 3;
}

public class UpdateCourseExamRequest
{
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal ExamFee { get; set; }
    public int QuestionCount { get; set; }
    public int DurationMinutes { get; set; }
    public int PassingScore { get; set; }
    public int MaxAttempts { get; set; } = 3;
    public bool Status { get; set; } = true;
}

public class ExamListQuery
{
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public string? SearchTerm { get; set; }
    public bool? IsArchived { get; set; } = false;
    public int? CourseId { get; set; }
}
