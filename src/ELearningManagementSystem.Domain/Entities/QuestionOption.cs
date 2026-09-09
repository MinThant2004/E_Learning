namespace ELearningManagementSystem.Domain.Entities;

public partial class QuestionOption
{
    public int OptionId { get; set; }
    public int QuestionId { get; set; }
    public string OptionText { get; set; } = null!;
    public bool IsCorrect { get; set; }
    public byte[] RowVersion { get; set; } = null!;

    public virtual Question Question { get; set; } = null!;
}
