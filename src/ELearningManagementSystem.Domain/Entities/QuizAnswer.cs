namespace ELearningManagementSystem.Domain.Entities;

public partial class QuizAnswer
{
    public int QuizAnswerId { get; set; }
    public int AttemptId { get; set; }
    public int QuestionId { get; set; }
    public int SelectedOptionId { get; set; }

    public virtual QuizAttempt Attempt { get; set; } = null!;
    public virtual Question Question { get; set; } = null!;
    public virtual QuestionOption SelectedOption { get; set; } = null!;
}
