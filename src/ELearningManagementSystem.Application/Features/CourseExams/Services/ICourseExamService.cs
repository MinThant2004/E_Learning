using ELearningManagementSystem.Application.Common;
using ELearningManagementSystem.Application.Features.CourseExams.DTOs;

namespace ELearningManagementSystem.Application.Features.CourseExams.Services;

public interface ICourseExamService
{
    Task<Result<PagedResult<CourseExamResponse>>> GetPagedExamsAsync(ExamListQuery query, CancellationToken cancellationToken = default);
    Task<Result<CourseExamResponse>> GetExamByIdAsync(int examId, CancellationToken cancellationToken = default);
    Task<Result<CourseExamResponse>> CreateExamAsync(CreateCourseExamRequest request, int userId, CancellationToken cancellationToken = default);
    Task<Result<CourseExamResponse>> UpdateExamAsync(int examId, UpdateCourseExamRequest request, int userId, CancellationToken cancellationToken = default);
    Task<Result> ArchiveExamAsync(int examId, int userId, CancellationToken cancellationToken = default);
    Task<Result> RestoreExamAsync(int examId, int userId, CancellationToken cancellationToken = default);

    Task<Result<List<ExamQuestionResponse>>> GetQuestionPoolAsync(int examId, CancellationToken cancellationToken = default);
    Task<Result<ExamQuestionResponse>> AddQuestionToPoolAsync(int examId, CreateExamQuestionRequest request, int userId, CancellationToken cancellationToken = default);
    Task<Result<ExamQuestionResponse>> UpdateQuestionInPoolAsync(int questionId, UpdateExamQuestionRequest request, int userId, CancellationToken cancellationToken = default);
    Task<Result> DeleteQuestionFromPoolAsync(int questionId, int userId, CancellationToken cancellationToken = default);
}
