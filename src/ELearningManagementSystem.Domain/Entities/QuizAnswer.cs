namespace ELearningManagementSystem.Domain.Entities;

public class QuizAnswer
{
    public int Id { get; set; }
    public int AttemptId { get; set; }
    public int QuestionId { get; set; }
    public int SelectedOptionId { get; set; }

    // Navigation properties
    public QuizAttempt Attempt { get; set; } = null!;
    public QuizQuestion Question { get; set; } = null!;
    public QuizOption SelectedOption { get; set; } = null!;
}
