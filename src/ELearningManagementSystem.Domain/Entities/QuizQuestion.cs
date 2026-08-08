namespace ELearningManagementSystem.Domain.Entities;

public class QuizQuestion
{
    public int Id { get; set; }
    public int QuizId { get; set; }
    public string QuestionText { get; set; } = string.Empty;
    public int OrderIndex { get; set; }

    // Navigation properties
    public Quiz Quiz { get; set; } = null!;
    public ICollection<QuizOption> Options { get; set; } = new List<QuizOption>();
}
