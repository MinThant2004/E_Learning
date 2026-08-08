namespace ELearningManagementSystem.Domain.Entities;

public class QuizOption
{
    public int Id { get; set; }
    public int QuestionId { get; set; }
    public string OptionText { get; set; } = string.Empty;
    public bool IsCorrect { get; set; }

    // Navigation properties
    public QuizQuestion Question { get; set; } = null!;
}
