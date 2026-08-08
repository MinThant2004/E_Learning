using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ELearningManagementSystem.Application.Common;
using ELearningManagementSystem.Application.Features.Lessons.DTOs;

namespace ELearningManagementSystem.Application.Features.Lessons.Services;

public interface ILessonService
{
    Task<Result<IEnumerable<LessonSummaryResponse>>> GetLessonsByCourseIdAsync(int courseId, bool? isArchived = false, CancellationToken cancellationToken = default);
    Task<Result<LessonDetailResponse>> GetLessonByIdAsync(int courseId, int lessonId, CancellationToken cancellationToken = default);
    Task<Result<LessonDetailResponse>> CreateAsync(CreateLessonRequest request, CancellationToken cancellationToken = default);
    Task<Result<LessonDetailResponse>> UpdateAsync(int lessonId, UpdateLessonRequest request, CancellationToken cancellationToken = default);
    Task<Result<bool>> ArchiveLessonAsync(int lessonId, CancellationToken cancellationToken = default);
    Task<Result<bool>> RestoreLessonAsync(int lessonId, CancellationToken cancellationToken = default);
}
