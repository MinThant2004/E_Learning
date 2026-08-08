namespace ELearningManagementSystem.Application.Features.QuizAttempts.DTOs;

public class AvailableQuizResponse
{
    public int QuizId { get; set; }
    public int CourseId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public List<StudentQuizQuestionResponse> Questions { get; set; } = new();
}

public class StudentQuizQuestionResponse
{
    public int QuestionId { get; set; }
    public string QuestionText { get; set; } = string.Empty;
    public int DisplayOrder { get; set; }
    public List<StudentQuizOptionResponse> Options { get; set; } = new();
}

public class StudentQuizOptionResponse
{
    public int OptionId { get; set; }
    public string OptionText { get; set; } = string.Empty;
}
