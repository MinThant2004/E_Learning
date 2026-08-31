namespace ELearningManagementSystem.Domain.Entities;

public partial class CourseExamAttemptAnswer
{
    public int AttemptAnswerId { get; set; }
    public int AttemptId { get; set; }
    public int ExamQuestionId { get; set; }
    public int? SelectedOptionId { get; set; }
    public bool? IsCorrect { get; set; }
    public int QuestionOrder { get; set; }
    public string QuestionTextSnapshot { get; set; } = string.Empty;
    public string ShuffledOptionsJson { get; set; } = "[]";

    public virtual CourseExamAttempt Attempt { get; set; } = null!;
    public virtual ExamQuestion ExamQuestion { get; set; } = null!;
    public virtual ExamQuestionOption? SelectedOption { get; set; }
}
