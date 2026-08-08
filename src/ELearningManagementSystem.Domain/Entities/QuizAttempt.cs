namespace ELearningManagementSystem.Domain.Entities;

public partial class QuizAttempt
{
    public int AttemptId { get; set; }
    public int QuizId { get; set; }
    public int UserId { get; set; }
    public decimal Score { get; set; }
    public int CorrectAnswers { get; set; }
    public int TotalQuestions { get; set; }
    public bool Passed { get; set; }
    public DateTime SubmittedAt { get; set; }

    public virtual Quiz Quiz { get; set; } = null!;
    public virtual User User { get; set; } = null!;
}
