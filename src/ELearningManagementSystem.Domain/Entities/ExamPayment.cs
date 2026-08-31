namespace ELearningManagementSystem.Domain.Entities;

public partial class ExamPayment
{
    public int ExamPaymentId { get; set; }
    public int ExamId { get; set; }
    public int UserId { get; set; }
    public decimal Amount { get; set; }
    public string PaymentMethod { get; set; } = null!; // KPay, AyaPay, CBPay, WavePay
    public string TransactionId { get; set; } = null!;
    public string ScreenshotUrl { get; set; } = null!;
    public string Status { get; set; } = "Pending"; // Pending, Approved, Rejected
    public bool IsUsed { get; set; } = false; // Set to true when exam attempt is started
    public string? RejectionReason { get; set; }
    public int? ReviewedBy { get; set; }
    public DateTime? ReviewedAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public bool DeleteFlag { get; set; }

    public virtual CourseExam Exam { get; set; } = null!;
    public virtual User User { get; set; } = null!;
    public virtual User? ReviewedByNavigation { get; set; }
    public virtual ICollection<CourseExamAttempt> ExamAttempts { get; set; } = new List<CourseExamAttempt>();
}
