namespace ELearningManagementSystem.Domain.Entities;

public partial class ExamQuestionOption
{
    public int OptionId { get; set; }
    public int ExamQuestionId { get; set; }
    public string OptionText { get; set; } = null!;
    public bool IsCorrect { get; set; }
    public bool DeleteFlag { get; set; }
    public byte[] RowVersion { get; set; } = null!;

    public virtual ExamQuestion Question { get; set; } = null!;
}
