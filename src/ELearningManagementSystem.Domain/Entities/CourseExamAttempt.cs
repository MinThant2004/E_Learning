namespace ELearningManagementSystem.Domain.Entities;

public partial class CourseExamAttempt
{
    public int AttemptId { get; set; }
    public int ExamId { get; set; }
    public int UserId { get; set; }
    public int ExamPaymentId { get; set; }
    public DateTime StartedAt { get; set; }
    public DateTime ExpiresAt { get; set; }
    public DateTime? SubmittedAt { get; set; }
    public decimal? Score { get; set; }
    public bool? Passed { get; set; }
    public string Status { get; set; } = "InProgress"; // InProgress, Submitted, Expired
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public bool DeleteFlag { get; set; } = false;

    public virtual CourseExam CourseExam { get; set; } = null!;
    public virtual User User { get; set; } = null!;
    public virtual ExamPayment ExamPayment { get; set; } = null!;
    public virtual ICollection<CourseExamAttemptAnswer> AttemptAnswers { get; set; } = new List<CourseExamAttemptAnswer>();
}
