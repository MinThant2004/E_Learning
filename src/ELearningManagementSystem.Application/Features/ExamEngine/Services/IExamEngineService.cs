using ELearningManagementSystem.Application.Common;
using ELearningManagementSystem.Application.Features.ExamEngine.DTOs;

namespace ELearningManagementSystem.Application.Features.ExamEngine.Services;

public interface IExamEngineService
{
    Task<Result<ExamAttemptSessionResponse>> StartExamAttemptAsync(int examId, int userId, CancellationToken cancellationToken = default);
    Task<Result<ExamAttemptSessionResponse>> GetActiveAttemptSessionAsync(int attemptId, int userId, CancellationToken cancellationToken = default);
    Task<Result<bool>> SaveAnswerAsync(int attemptId, SaveAnswerRequest request, int userId, CancellationToken cancellationToken = default);
    Task<Result<ExamResultResponse>> SubmitExamAttemptAsync(int attemptId, SubmitExamAttemptRequest request, int userId, CancellationToken cancellationToken = default);
    Task<Result<ExamResultResponse>> GetAttemptResultAsync(int attemptId, int userId, CancellationToken cancellationToken = default);
    Task<Result<PublicCertificateResponse>> GetPublicCertificateAsync(int attemptId, CancellationToken cancellationToken = default);
    Task<Result<CertificateFileDownloadResponse>> DownloadCertificateFileAsync(int attemptId, CancellationToken cancellationToken = default);
    Task<Result<List<PublicCertificateResponse>>> GetMyCertificatesAsync(int userId, CancellationToken cancellationToken = default);
}
