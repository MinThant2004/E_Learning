using ELearningManagementSystem.Application.Common;
using ELearningManagementSystem.Application.Features.LessonProgress.DTOs;

namespace ELearningManagementSystem.Application.Features.LessonProgress.Services;

public interface ILessonProgressService
{
    Task<Result<CourseProgressResponse>> GetCourseProgressAsync(int courseId, int userId, CancellationToken cancellationToken = default);
    Task<Result<LessonProgressResponse>> CompleteLessonAsync(int courseId, int lessonId, int userId, CancellationToken cancellationToken = default);
}
