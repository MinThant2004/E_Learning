namespace ELearningManagementSystem.Domain.Entities;

public partial class LessonProgress
{
    public int LessonProgressId { get; set; }
    public int EnrollmentId { get; set; }
    public int LessonId { get; set; }
    public bool Completed { get; set; }
    public DateTime? CompletedDate { get; set; }

    public virtual Enrollment Enrollment { get; set; } = null!;
    public virtual Lesson Lesson { get; set; } = null!;
}
