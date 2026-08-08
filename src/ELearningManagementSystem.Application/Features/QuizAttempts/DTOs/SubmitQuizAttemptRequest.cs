namespace ELearningManagementSystem.Application.Features.QuizAttempts.DTOs;

public class SubmitQuizAttemptRequest
{
    public List<SubmitQuizAnswerRequest> Answers { get; set; } = new();
}

public class SubmitQuizAnswerRequest
{
    public int QuestionId { get; set; }
    public int SelectedOptionId { get; set; }
}
