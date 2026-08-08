using ELearningManagementSystem.Application.Common;
using ELearningManagementSystem.Application.Features.QuizAttempts.DTOs;

namespace ELearningManagementSystem.Application.Features.QuizAttempts.Services;

public interface IQuizAttemptService
{
    Task<Result<AvailableQuizResponse>> GetAvailableQuizAsync(int courseId, int userId, CancellationToken cancellationToken = default);
    Task<Result<QuizAttemptResultResponse>> SubmitQuizAttemptAsync(int courseId, int userId, SubmitQuizAttemptRequest request, CancellationToken cancellationToken = default);
    Task<Result<List<QuizAttemptHistoryResponse>>> GetStudentAttemptHistoryAsync(int courseId, int userId, CancellationToken cancellationToken = default);
    Task<Result<QuizAttemptResultResponse>> GetAttemptDetailsAsync(int attemptId, int userId, CancellationToken cancellationToken = default);
}
