using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ELearningManagementSystem.Application.Common;
using ELearningManagementSystem.Application.Features.Enrollments.DTOs;

namespace ELearningManagementSystem.Application.Features.Enrollments.Services;

public interface IEnrollmentService
{
    Task<Result<EnrollmentSummaryResponse>> EnrollAsync(int courseId, CancellationToken cancellationToken = default);
    Task<Result<IEnumerable<MyCourseResponse>>> GetMyEnrollmentsAsync(CancellationToken cancellationToken = default);
    Task<Result<bool>> CheckEnrollmentStatusAsync(int courseId, CancellationToken cancellationToken = default);
}
