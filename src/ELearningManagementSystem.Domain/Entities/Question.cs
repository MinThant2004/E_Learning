namespace ELearningManagementSystem.Domain.Entities;

public partial class Question
{
    public int QuestionId { get; set; }
    public int QuizId { get; set; }
    public string QuestionText { get; set; } = null!;
    public int DisplayOrder { get; set; }
    public byte[] RowVersion { get; set; } = null!;

    public virtual ICollection<QuestionOption> QuestionOptions { get; set; } = new List<QuestionOption>();
    public virtual Quiz Quiz { get; set; } = null!;
}
