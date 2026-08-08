using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ELearningManagementSystem.Application.Common;
using ELearningManagementSystem.Application.Features.Lessons.DTOs;
using ELearningManagementSystem.Application.Interfaces;
using ELearningManagementSystem.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace ELearningManagementSystem.Application.Features.Lessons.Services;

public class LessonService : ILessonService
{
    private readonly IAppDbContext _context;

    public LessonService(IAppDbContext context)
    {
        _context = context;
    }

    public async Task<Result<PagedResult<LessonSummaryResponse>>> GetPagedListAsync(LessonListQuery query, CancellationToken cancellationToken = default)
    {
        var courseExists = await _context.Courses
            .AnyAsync(c => c.CourseId == query.CourseId, cancellationToken);

        if (!courseExists)
            return Result.Failure<PagedResult<LessonSummaryResponse>>("CourseNotFound");

        IQueryable<Lesson> dbQuery = _context.Lessons
            .AsNoTracking()
            .Where(l => l.CourseId == query.CourseId);

        if (query.IsArchived.HasValue)
        {
            dbQuery = dbQuery.Where(l => l.DeleteFlag == query.IsArchived.Value);
        }

        if (!string.IsNullOrWhiteSpace(query.SearchTerm))
        {
            var search = query.SearchTerm.Trim().ToLower();
            dbQuery = dbQuery.Where(l => l.Title.ToLower().Contains(search) || l.Content.ToLower().Contains(search));
        }

        var totalCount = await dbQuery.CountAsync(cancellationToken);

        var lessons = await dbQuery
            .OrderBy(l => l.DisplayOrder)
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .Select(l => new LessonSummaryResponse
            {
                LessonId = l.LessonId,
                CourseId = l.CourseId,
                Title = l.Title,
                DisplayOrder = l.DisplayOrder,
                DeleteFlag = l.DeleteFlag
            })
            .ToListAsync(cancellationToken);

        var result = new PagedResult<LessonSummaryResponse>(lessons, totalCount, query.Page, query.PageSize);

        return Result.Success(result);
    }

    public async Task<Result<IEnumerable<LessonSummaryResponse>>> GetLessonsByCourseIdAsync(int courseId, bool? isArchived = false, CancellationToken cancellationToken = default)
    {
        // 1. Verify parent course exists
        var course = await _context.Courses
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.CourseId == courseId, cancellationToken);

        if (course == null)
            return Result.Failure<IEnumerable<LessonSummaryResponse>>("CourseNotFound");

        IQueryable<Lesson> query = _context.Lessons
            .AsNoTracking()
            .Where(l => l.CourseId == courseId);

        if (isArchived.HasValue)
        {
            query = query.Where(l => l.DeleteFlag == isArchived.Value);
        }

        var lessons = await query
            .OrderBy(l => l.DisplayOrder)
            .Select(l => new LessonSummaryResponse
            {
                LessonId = l.LessonId,
                CourseId = l.CourseId,
                Title = l.Title,
                DisplayOrder = l.DisplayOrder,
                DeleteFlag = l.DeleteFlag
            })
            .ToListAsync(cancellationToken);

        return Result.Success<IEnumerable<LessonSummaryResponse>>(lessons);
    }

    public async Task<Result<LessonDetailResponse>> GetLessonByIdAsync(int courseId, int lessonId, CancellationToken cancellationToken = default)
    {
        var courseExists = await _context.Courses
            .AnyAsync(c => c.CourseId == courseId, cancellationToken);

        if (!courseExists)
            return Result.Failure<LessonDetailResponse>("CourseNotFound");

        var lesson = await _context.Lessons
            .AsNoTracking()
            .FirstOrDefaultAsync(l => l.LessonId == lessonId && l.CourseId == courseId, cancellationToken);

        if (lesson == null)
            return Result.Failure<LessonDetailResponse>("LessonNotFound");

        var response = new LessonDetailResponse
        {
            LessonId = lesson.LessonId,
            CourseId = lesson.CourseId,
            Title = lesson.Title,
            Content = lesson.Content,
            DisplayOrder = lesson.DisplayOrder,
            CreatedAt = lesson.CreatedAt,
            UpdatedAt = lesson.UpdatedAt,
            DeleteFlag = lesson.DeleteFlag
        };

        return Result.Success(response);
    }

    public async Task<Result<LessonDetailResponse>> CreateAsync(CreateLessonRequest request, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.Title))
            return Result.Failure<LessonDetailResponse>("InvalidLessonTitle");

        if (string.IsNullOrWhiteSpace(request.Content))
            return Result.Failure<LessonDetailResponse>("InvalidLessonContent");

        var course = await _context.Courses
            .FirstOrDefaultAsync(c => c.CourseId == request.CourseId && !c.DeleteFlag, cancellationToken);

        if (course == null)
            return Result.Failure<LessonDetailResponse>("CourseNotFound");

        // Calculate final DisplayOrder and handle shifting occupied positions
        int finalOrder = request.DisplayOrder;
        if (finalOrder < 1)
        {
            var maxOrder = await _context.Lessons
                .Where(l => l.CourseId == request.CourseId && !l.DeleteFlag)
                .Select(l => (int?)l.DisplayOrder)
                .MaxAsync(cancellationToken) ?? 0;
            finalOrder = maxOrder + 1;
        }
        else
        {
            // Shift existing lessons
            var occupied = await _context.Lessons
                .AnyAsync(l => l.CourseId == request.CourseId && l.DisplayOrder == finalOrder && !l.DeleteFlag, cancellationToken);
            if (occupied)
            {
                var lessonsToShift = await _context.Lessons
                    .Where(l => l.CourseId == request.CourseId && l.DisplayOrder >= finalOrder && !l.DeleteFlag)
                    .ToListAsync(cancellationToken);
                foreach (var l in lessonsToShift)
                {
                    l.DisplayOrder += 1;
                }
            }
        }

        var lesson = new Lesson
        {
            CourseId = request.CourseId,
            Title = request.Title.Trim(),
            Content = request.Content.Trim(),
            DisplayOrder = finalOrder,
            CreatedAt = DateTime.UtcNow,
            DeleteFlag = false
        };

        _context.Lessons.Add(lesson);
        await _context.SaveChangesAsync(cancellationToken);

        return await GetLessonByIdAsync(lesson.CourseId, lesson.LessonId, cancellationToken);
    }

    public async Task<Result<LessonDetailResponse>> UpdateAsync(int lessonId, UpdateLessonRequest request, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.Title))
            return Result.Failure<LessonDetailResponse>("InvalidLessonTitle");

        if (string.IsNullOrWhiteSpace(request.Content))
            return Result.Failure<LessonDetailResponse>("InvalidLessonContent");

        var lesson = await _context.Lessons
            .FirstOrDefaultAsync(l => l.LessonId == lessonId && !l.DeleteFlag, cancellationToken);

        if (lesson == null)
            return Result.Failure<LessonDetailResponse>("LessonNotFound");

        // Check if display order is changed and shift other lessons
        if (lesson.DisplayOrder != request.DisplayOrder && request.DisplayOrder >= 1)
        {
            int oldOrder = lesson.DisplayOrder;
            int newOrder = request.DisplayOrder;

            var maxOrder = await _context.Lessons
                .Where(l => l.CourseId == lesson.CourseId && !l.DeleteFlag)
                .Select(l => (int?)l.DisplayOrder)
                .MaxAsync(cancellationToken) ?? 1;

            if (newOrder > maxOrder)
            {
                newOrder = maxOrder;
            }

            if (newOrder > oldOrder)
            {
                var lessonsToShift = await _context.Lessons
                    .Where(l => l.CourseId == lesson.CourseId && l.DisplayOrder > oldOrder && l.DisplayOrder <= newOrder && !l.DeleteFlag)
                    .ToListAsync(cancellationToken);
                foreach (var l in lessonsToShift)
                {
                    l.DisplayOrder -= 1;
                }
            }
            else
            {
                var lessonsToShift = await _context.Lessons
                    .Where(l => l.CourseId == lesson.CourseId && l.DisplayOrder >= newOrder && l.DisplayOrder < oldOrder && !l.DeleteFlag)
                    .ToListAsync(cancellationToken);
                foreach (var l in lessonsToShift)
                {
                    l.DisplayOrder += 1;
                }
            }
            lesson.DisplayOrder = newOrder;
        }

        lesson.Title = request.Title.Trim();
        lesson.Content = request.Content.Trim();
        lesson.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        return await GetLessonByIdAsync(lesson.CourseId, lesson.LessonId, cancellationToken);
    }

    public async Task<Result<bool>> ArchiveLessonAsync(int lessonId, CancellationToken cancellationToken = default)
    {
        var lesson = await _context.Lessons
            .FirstOrDefaultAsync(l => l.LessonId == lessonId && !l.DeleteFlag, cancellationToken);

        if (lesson == null)
            return Result.Failure<bool>("LessonNotFound");

        lesson.DeleteFlag = true;
        lesson.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);
        return Result.Success(true);
    }

    public async Task<Result<bool>> RestoreLessonAsync(int lessonId, CancellationToken cancellationToken = default)
    {
        var lesson = await _context.Lessons
            .Include(l => l.Course)
            .FirstOrDefaultAsync(l => l.LessonId == lessonId && l.DeleteFlag, cancellationToken);

        if (lesson == null)
            return Result.Failure<bool>("LessonNotFound");

        // Business Rule: If restoring references an archived Course
        if (lesson.Course.DeleteFlag)
            return Result.Failure<bool>("CannotRestoreArchivedCourse");

        lesson.DeleteFlag = false;
        lesson.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);
        return Result.Success(true);
    }
}
