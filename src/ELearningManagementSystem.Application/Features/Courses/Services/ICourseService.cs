using System.Threading;
using System.Threading.Tasks;
using ELearningManagementSystem.Application.Common;
using ELearningManagementSystem.Application.Features.Courses.DTOs;

namespace ELearningManagementSystem.Application.Features.Courses.Services;

public interface ICourseService
{
    Task<Result<PagedResult<CourseSummaryResponse>>> GetPagedListAsync(CourseListQuery query, CancellationToken cancellationToken = default);
    Task<Result<CourseDetailResponse>> GetByIdAsync(int courseId, CancellationToken cancellationToken = default);
    Task<Result<CourseDetailResponse>> CreateAsync(CreateCourseRequest request, CancellationToken cancellationToken = default);
    Task<Result<CourseDetailResponse>> UpdateAsync(int courseId, UpdateCourseRequest request, CancellationToken cancellationToken = default);
    Task<Result<bool>> ArchiveCourseAsync(int courseId, CancellationToken cancellationToken = default);
    Task<Result<bool>> RestoreCourseAsync(int courseId, CancellationToken cancellationToken = default);
}
