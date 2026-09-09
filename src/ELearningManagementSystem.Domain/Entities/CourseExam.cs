namespace ELearningManagementSystem.Domain.Entities;

public partial class CourseExam
{
    public int ExamId { get; set; }
    public int CourseId { get; set; }
    public string Title { get; set; } = null!;
    public string? Description { get; set; }
    public decimal ExamFee { get; set; }
    public int QuestionCount { get; set; }
    public int DurationMinutes { get; set; }
    public int PassingScore { get; set; }
    public int MaxAttempts { get; set; } = 3;
    public bool Status { get; set; } = true;
    public int CreatedBy { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public bool DeleteFlag { get; set; }
    public byte[] RowVersion { get; set; } = null!;

    public virtual Course Course { get; set; } = null!;
    public virtual User CreatedByNavigation { get; set; } = null!;
    public virtual ICollection<ExamQuestion> ExamQuestions { get; set; } = new List<ExamQuestion>();
    public virtual ICollection<ExamPayment> ExamPayments { get; set; } = new List<ExamPayment>();
    public virtual ICollection<CourseExamAttempt> ExamAttempts { get; set; } = new List<CourseExamAttempt>();
}
