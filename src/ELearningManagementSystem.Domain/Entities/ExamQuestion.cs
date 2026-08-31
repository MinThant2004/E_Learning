namespace ELearningManagementSystem.Domain.Entities;

public partial class ExamQuestion
{
    public int ExamQuestionId { get; set; }
    public int ExamId { get; set; }
    public string QuestionText { get; set; } = null!;
    public int DisplayOrder { get; set; }
    public string DifficultyLevel { get; set; } = "Medium";
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public bool DeleteFlag { get; set; }

    public virtual CourseExam Exam { get; set; } = null!;
    public virtual ICollection<ExamQuestionOption> ExamQuestionOptions { get; set; } = new List<ExamQuestionOption>();
}
