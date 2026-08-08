namespace ELearningManagementSystem.Application.Features.Quizzes.DTOs;

public class QuizDetailResponse
{
    public int QuizId { get; set; }
    public int CourseId { get; set; }
    public string Title { get; set; } = string.Empty;
    public int PassingScore { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public bool DeleteFlag { get; set; }
    public List<QuestionResponse> Questions { get; set; } = new();
}
