namespace ELearningManagementSystem.Domain.Entities;

public partial class Quiz
{
    public int QuizId { get; set; }
    public int CourseId { get; set; }
    public string Title { get; set; } = null!;
    public int PassingScore { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public bool DeleteFlag { get; set; }

    public virtual Course Course { get; set; } = null!;
    public virtual ICollection<Question> Questions { get; set; } = new List<Question>();
    public virtual ICollection<QuizAttempt> QuizAttempts { get; set; } = new List<QuizAttempt>();
}
