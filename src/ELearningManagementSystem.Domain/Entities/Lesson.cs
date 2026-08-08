namespace ELearningManagementSystem.Domain.Entities;

public partial class Lesson
{
    public int LessonId { get; set; }
    public int CourseId { get; set; }
    public string Title { get; set; } = null!;
    public string Content { get; set; } = null!;
    public int DisplayOrder { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public bool DeleteFlag { get; set; }

    public virtual Course Course { get; set; } = null!;
    public virtual ICollection<LessonProgress> LessonProgresses { get; set; } = new List<LessonProgress>();
}
