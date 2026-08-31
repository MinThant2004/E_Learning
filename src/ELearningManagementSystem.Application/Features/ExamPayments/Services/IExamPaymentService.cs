using ELearningManagementSystem.Application.Common;
using ELearningManagementSystem.Application.Features.ExamPayments.DTOs;

namespace ELearningManagementSystem.Application.Features.ExamPayments.Services;

public interface IExamPaymentService
{
    Task<Result<List<StudentCourseExamStatusResponse>>> GetStudentAvailableExamsAsync(int userId, CancellationToken cancellationToken = default);
    Task<Result<StudentCourseExamStatusResponse>> GetStudentExamStatusAsync(int examId, int userId, CancellationToken cancellationToken = default);
    Task<Result<ExamPaymentResponse>> SubmitPaymentAsync(int examId, int userId, SubmitExamPaymentRequest request, string screenshotUrl, CancellationToken cancellationToken = default);
    Task<Result<PagedResult<ExamPaymentResponse>>> GetPagedPaymentsAsync(PaymentListQuery query, CancellationToken cancellationToken = default);
    Task<Result<ExamPaymentResponse>> ReviewPaymentAsync(int paymentId, ReviewExamPaymentRequest request, int adminId, CancellationToken cancellationToken = default);
}
