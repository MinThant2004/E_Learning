using ELearningManagementSystem.Application.Common;
using ELearningManagementSystem.Application.Features.LessonProgress.DTOs;
using ELearningManagementSystem.Application.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace ELearningManagementSystem.Application.Features.LessonProgress.Services;

public class LessonProgressService : ILessonProgressService
{
    private readonly IAppDbContext _context;

    public LessonProgressService(IAppDbContext context)
    {
        _context = context;
    }

    public async Task<Result<CourseProgressResponse>> GetCourseProgressAsync(int courseId, int userId, CancellationToken cancellationToken = default)
    {
        var enrollment = await _context.Enrollments
            .FirstOrDefaultAsync(e => e.CourseId == courseId && e.UserId == userId, cancellationToken);

        if (enrollment == null)
            return Result.Failure<CourseProgressResponse>("LessonProgress.EnrollmentNotFound");

        var activeLessonsCount = await _context.Lessons
            .CountAsync(l => l.CourseId == courseId && !l.DeleteFlag, cancellationToken);

        var progressRecords = await _context.LessonProgresses
            .Where(lp => lp.EnrollmentId == enrollment.EnrollmentId && !lp.Lesson.DeleteFlag)
            .ToListAsync(cancellationToken);

        var response = new CourseProgressResponse
        {
            TotalActiveLessons = activeLessonsCount,
            CompletedCount = progressRecords.Count(p => p.Completed),
            Lessons = progressRecords.Select(p => new LessonProgressResponse
            {
                LessonId = p.LessonId,
                Completed = p.Completed,
                CompletedDate = p.CompletedDate
            }).ToList()
        };

        return Result.Success(response);
    }

    public async Task<Result<LessonProgressResponse>> CompleteLessonAsync(int courseId, int lessonId, int userId, CancellationToken cancellationToken = default)
    {
        var enrollment = await _context.Enrollments
            .FirstOrDefaultAsync(e => e.CourseId == courseId && e.UserId == userId, cancellationToken);

        if (enrollment == null)
            return Result.Failure<LessonProgressResponse>("LessonProgress.EnrollmentNotFound");

        var lesson = await _context.Lessons
            .FirstOrDefaultAsync(l => l.LessonId == lessonId && l.CourseId == courseId, cancellationToken);

        if (lesson == null)
            return Result.Failure<LessonProgressResponse>("LessonProgress.LessonNotFound");

        if (lesson.DeleteFlag)
            return Result.Failure<LessonProgressResponse>("LessonProgress.LessonArchived");

        var progress = await _context.LessonProgresses
            .FirstOrDefaultAsync(lp => lp.EnrollmentId == enrollment.EnrollmentId && lp.LessonId == lessonId, cancellationToken);

        if (progress != null)
        {
            if (progress.Completed)
            {
                return Result.Success(new LessonProgressResponse
                {
                    LessonId = progress.LessonId,
                    Completed = progress.Completed,
                    CompletedDate = progress.CompletedDate
                });
            }
            else
            {
                progress.Completed = true;
                progress.CompletedDate = DateTime.UtcNow;
                await _context.SaveChangesAsync(cancellationToken);
                
                return Result.Success(new LessonProgressResponse
                {
                    LessonId = progress.LessonId,
                    Completed = progress.Completed,
                    CompletedDate = progress.CompletedDate
                });
            }
        }

        progress = new Domain.Entities.LessonProgress
        {
            EnrollmentId = enrollment.EnrollmentId,
            LessonId = lessonId,
            Completed = true,
            CompletedDate = DateTime.UtcNow
        };

        _context.LessonProgresses.Add(progress);
        await _context.SaveChangesAsync(cancellationToken);

        return Result.Success(new LessonProgressResponse
        {
            LessonId = progress.LessonId,
            Completed = progress.Completed,
            CompletedDate = progress.CompletedDate
        });
    }
}
