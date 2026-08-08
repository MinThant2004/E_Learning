namespace ELearningManagementSystem.Domain.Entities;

public partial class Enrollment
{
    public int EnrollmentId { get; set; }
    public int UserId { get; set; }
    public int CourseId { get; set; }
    public DateTime EnrollDate { get; set; }
    public bool Completed { get; set; }
    public DateTime? CompletedDate { get; set; }

    public virtual Course Course { get; set; } = null!;
    public virtual ICollection<LessonProgress> LessonProgresses { get; set; } = new List<LessonProgress>();
    public virtual User User { get; set; } = null!;
}
