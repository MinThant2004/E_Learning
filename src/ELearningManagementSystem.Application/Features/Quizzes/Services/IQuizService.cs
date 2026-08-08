using ELearningManagementSystem.Application.Common;
using ELearningManagementSystem.Application.Features.Quizzes.DTOs;

namespace ELearningManagementSystem.Application.Features.Quizzes.Services;

public interface IQuizService
{
    // Quiz CRUD
    Task<Result<List<QuizResponse>>> GetQuizzesByCourseAsync(int courseId, bool? isArchived = null, CancellationToken cancellationToken = default);
    Task<Result<QuizDetailResponse>> GetQuizByIdAsync(int quizId, CancellationToken cancellationToken = default);
    Task<Result<QuizDetailResponse>> CreateQuizAsync(CreateQuizRequest request, CancellationToken cancellationToken = default);
    Task<Result<QuizDetailResponse>> UpdateQuizAsync(int quizId, UpdateQuizRequest request, CancellationToken cancellationToken = default);
    Task<Result<bool>> ArchiveQuizAsync(int quizId, CancellationToken cancellationToken = default);
    Task<Result<bool>> RestoreQuizAsync(int quizId, CancellationToken cancellationToken = default);

    // Question CRUD
    Task<Result<QuestionResponse>> CreateQuestionAsync(CreateQuestionRequest request, CancellationToken cancellationToken = default);
    Task<Result<QuestionResponse>> UpdateQuestionAsync(int questionId, UpdateQuestionRequest request, CancellationToken cancellationToken = default);
    Task<Result<bool>> DeleteQuestionAsync(int questionId, CancellationToken cancellationToken = default);

    // Option CRUD
    Task<Result<List<QuestionOptionResponse>>> GetOptionsByQuestionAsync(int questionId, CancellationToken cancellationToken = default);
    Task<Result<QuestionOptionResponse>> CreateOptionAsync(int questionId, CreateQuestionOptionRequest request, CancellationToken cancellationToken = default);
    Task<Result<QuestionOptionResponse>> UpdateOptionAsync(int optionId, UpdateQuestionOptionRequest request, CancellationToken cancellationToken = default);
    Task<Result<bool>> DeleteOptionAsync(int optionId, CancellationToken cancellationToken = default);
    Task<Result<bool>> ArchiveOptionAsync(int optionId, CancellationToken cancellationToken = default);
    Task<Result<bool>> RestoreOptionAsync(int optionId, CancellationToken cancellationToken = default);
}
