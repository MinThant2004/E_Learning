namespace ELearningManagementSystem.Domain.Entities;

public partial class Category
{
    public int CategoryId { get; set; }
    public string CategoryName { get; set; } = null!;
    public string? Description { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public bool DeleteFlag { get; set; }

    public virtual ICollection<Course> Courses { get; set; } = new List<Course>();
}
