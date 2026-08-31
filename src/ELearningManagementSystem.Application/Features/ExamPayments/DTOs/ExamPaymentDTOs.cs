namespace ELearningManagementSystem.Application.Features.ExamPayments.DTOs;

public class SubmitExamPaymentRequest
{
    public string PaymentMethod { get; set; } = string.Empty; // KPay, AyaPay, CBPay, WavePay
    public string TransactionId { get; set; } = string.Empty;
}

public class ReviewExamPaymentRequest
{
    public bool Approve { get; set; }
    public string? RejectionReason { get; set; }
}

public class ExamPaymentResponse
{
    public int ExamPaymentId { get; set; }
    public int ExamId { get; set; }
    public string ExamTitle { get; set; } = string.Empty;
    public int CourseId { get; set; }
    public string CourseTitle { get; set; } = string.Empty;
    public int UserId { get; set; }
    public string StudentName { get; set; } = string.Empty;
    public string StudentEmail { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string PaymentMethod { get; set; } = string.Empty;
    public string TransactionId { get; set; } = string.Empty;
    public string ScreenshotUrl { get; set; } = string.Empty;
    public string Status { get; set; } = "Pending";
    public bool IsUsed { get; set; }
    public string? RejectionReason { get; set; }
    public string? ReviewedByName { get; set; }
    public DateTime? ReviewedAt { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class StudentCourseExamStatusResponse
{
    public int ExamId { get; set; }
    public int CourseId { get; set; }
    public string CourseTitle { get; set; } = string.Empty;
    public string ExamTitle { get; set; } = string.Empty;
    public string? Description { get; set; }
    public decimal ExamFee { get; set; }
    public int QuestionCount { get; set; }
    public int DurationMinutes { get; set; }
    public int PassingScore { get; set; }
    public int MaxAttempts { get; set; } = 1;
    public int PoolQuestionCount { get; set; }
    
    // Payment Status: NotPaid, Pending, Approved, Rejected, InProgress, Submitted
    public string PaymentStatus { get; set; } = "NotPaid";
    public int? ActivePaymentId { get; set; }
    public int? ActiveAttemptId { get; set; }
    public string? RejectionReason { get; set; }
    public bool CanTakeExam { get; set; }

    // Pass/fail of the student's most recent submitted attempt (null if none yet).
    // Drives retake eligibility: only failed students are offered a "Retake Exam" action.
    public bool? LastAttemptPassed { get; set; }
}

public class PaymentListQuery
{
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public string? Status { get; set; } = "Pending"; // Pending, Approved, Rejected, All
    public string? SearchTerm { get; set; }
}
