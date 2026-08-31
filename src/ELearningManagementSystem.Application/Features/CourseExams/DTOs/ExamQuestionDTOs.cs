namespace ELearningManagementSystem.Application.Features.CourseExams.DTOs;

public class ExamQuestionResponse
{
    public int ExamQuestionId { get; set; }
    public int ExamId { get; set; }
    public string QuestionText { get; set; } = string.Empty;
    public int DisplayOrder { get; set; }
    public string DifficultyLevel { get; set; } = "Medium";
    public List<ExamQuestionOptionResponse> Options { get; set; } = new();
}

public class ExamQuestionOptionResponse
{
    public int OptionId { get; set; }
    public int ExamQuestionId { get; set; }
    public string OptionText { get; set; } = string.Empty;
    public bool IsCorrect { get; set; }
}

public class CreateExamQuestionRequest
{
    public string QuestionText { get; set; } = string.Empty;
    public string DifficultyLevel { get; set; } = "Medium";
    public List<CreateExamOptionRequest> Options { get; set; } = new();
}

public class CreateExamOptionRequest
{
    public string OptionText { get; set; } = string.Empty;
    public bool IsCorrect { get; set; }
}

public class UpdateExamQuestionRequest
{
    public string QuestionText { get; set; } = string.Empty;
    public string DifficultyLevel { get; set; } = "Medium";
    public List<CreateExamOptionRequest> Options { get; set; } = new();
}
