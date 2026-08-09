using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ELearningManagementSystem.Application.Common;
using ELearningManagementSystem.Application.Features.Enrollments.DTOs;
using ELearningManagementSystem.Application.Interfaces;
using ELearningManagementSystem.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ELearningManagementSystem.Application.Features.Enrollments.Services;

public class EnrollmentService : IEnrollmentService
{
    private readonly IAppDbContext _dbContext;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<EnrollmentService> _logger;

    public EnrollmentService(IAppDbContext dbContext, ICurrentUserService currentUserService, ILogger<EnrollmentService> logger)
    {
        _dbContext = dbContext;
        _currentUserService = currentUserService;
        _logger = logger;
    }

    public async Task<Result<EnrollmentSummaryResponse>> EnrollAsync(int courseId, CancellationToken cancellationToken = default)
    {
        if (!_currentUserService.UserId.HasValue)
            return Result.Failure<EnrollmentSummaryResponse>("AccountNotFound");

        var userId = _currentUserService.UserId.Value;

        var course = await _dbContext.Courses
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.CourseId == courseId, cancellationToken);

        if (course == null)
            return Result.Failure<EnrollmentSummaryResponse>("CourseNotFound");

        if (course.DeleteFlag)
            return Result.Failure<EnrollmentSummaryResponse>("CourseArchived");

        var existingEnrollment = await _dbContext.Enrollments
            .AsNoTracking()
            .FirstOrDefaultAsync(e => e.CourseId == courseId && e.UserId == userId, cancellationToken);

        if (existingEnrollment != null)
            return Result.Failure<EnrollmentSummaryResponse>("AlreadyEnrolled");

        var enrollment = new Enrollment
        {
            CourseId = courseId,
            UserId = userId,
            EnrollDate = DateTime.UtcNow,
            Completed = false
        };

        _dbContext.Enrollments.Add(enrollment);
        await _dbContext.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("User {UserId} enrolled in Course {CourseId} (EnrollmentId={EnrollmentId})", userId, courseId, enrollment.EnrollmentId);

        var response = new EnrollmentSummaryResponse
        {
            EnrollmentId = enrollment.EnrollmentId,
            CourseId = course.CourseId,
            CourseTitle = course.Title,
            CourseDescription = course.Description,
            ThumbnailUrl = course.ThumbnailUrl,
            EnrollDate = enrollment.EnrollDate,
            Completed = enrollment.Completed,
            CompletedDate = enrollment.CompletedDate
        };

        return Result.Success(response);
    }

    public async Task<Result<IEnumerable<MyCourseResponse>>> GetMyEnrollmentsAsync(CancellationToken cancellationToken = default)
    {
        if (!_currentUserService.UserId.HasValue)
            return Result.Failure<IEnumerable<MyCourseResponse>>("AccountNotFound");

        var userId = _currentUserService.UserId.Value;

        var myCourses = await _dbContext.Enrollments
            .Include(e => e.Course)
            .Where(e => e.UserId == userId && !e.Course.DeleteFlag)
            .OrderByDescending(e => e.EnrollDate)
            .Select(e => new MyCourseResponse
            {
                CourseId = e.Course.CourseId,
                Title = e.Course.Title,
                Description = e.Course.Description,
                ThumbnailUrl = e.Course.ThumbnailUrl,
                EnrollmentId = e.EnrollmentId,
                EnrollDate = e.EnrollDate,
                Completed = e.Completed,
                CompletedCount = e.LessonProgresses
                    .Where(lp => lp.Completed && !lp.Lesson.DeleteFlag)
                    .Select(lp => lp.LessonId)
                    .Distinct()
                    .Count(),
                TotalActiveLessons = e.Course.Lessons.Count(l => !l.DeleteFlag)
            })
            .ToListAsync(cancellationToken);

        return Result.Success<IEnumerable<MyCourseResponse>>(myCourses);
    }

    public async Task<Result<bool>> CheckEnrollmentStatusAsync(int courseId, CancellationToken cancellationToken = default)
    {
        if (!_currentUserService.UserId.HasValue)
            return Result.Success(false); // Return false instead of failing so UI can handle it gracefully

        var userId = _currentUserService.UserId.Value;

        var isEnrolled = await _dbContext.Enrollments
            .AsNoTracking()
            .AnyAsync(e => e.CourseId == courseId && e.UserId == userId, cancellationToken);

        return Result.Success(isEnrolled);
    }
}
