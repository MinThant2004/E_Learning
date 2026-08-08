namespace ELearningManagementSystem.Application.Features.QuizAttempts.DTOs;

public class QuizAttemptResultResponse
{
    public int AttemptId { get; set; }
    public int QuizId { get; set; }
    public decimal Score { get; set; }
    public int CorrectAnswers { get; set; }
    public int TotalQuestions { get; set; }
    public bool Passed { get; set; }
    public DateTime SubmittedAt { get; set; }
}

public class QuizAttemptHistoryResponse
{
    public int AttemptId { get; set; }
    public int QuizId { get; set; }
    public string QuizTitle { get; set; } = string.Empty;
    public decimal Score { get; set; }
    public int CorrectAnswers { get; set; }
    public int TotalQuestions { get; set; }
    public bool Passed { get; set; }
    public DateTime SubmittedAt { get; set; }
}
