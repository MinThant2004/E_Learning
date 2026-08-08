using ELearningManagementSystem.Application.Common;
using ELearningManagementSystem.Application.Features.Courses.DTOs;
using ELearningManagementSystem.Application.Interfaces;
using ELearningManagementSystem.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace ELearningManagementSystem.Application.Features.Courses.Services;

public class CourseService : ICourseService
{
    private readonly IAppDbContext _dbContext;
    private readonly ICurrentUserService _currentUserService;

    public CourseService(IAppDbContext dbContext, ICurrentUserService currentUserService)
    {
        _dbContext = dbContext;
        _currentUserService = currentUserService;
    }

    public async Task<Result<PagedResult<CourseSummaryResponse>>> GetPagedListAsync(CourseListQuery query, CancellationToken cancellationToken = default)
    {
        var dbQuery = _dbContext.Courses
            .Include(c => c.Category)
            .AsNoTracking()
            .AsQueryable();

        if (!query.IncludeDeleted)
        {
            dbQuery = dbQuery.Where(c => !c.DeleteFlag);
        }

        if (!string.IsNullOrWhiteSpace(query.SearchTerm))
        {
            dbQuery = dbQuery.Where(c => c.Title.Contains(query.SearchTerm) || (c.Description != null && c.Description.Contains(query.SearchTerm)));
        }

        var totalCount = await dbQuery.CountAsync(cancellationToken);

        var items = await dbQuery
            .OrderByDescending(c => c.CreatedAt)
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .Select(c => new CourseSummaryResponse
            {
                CourseId = c.CourseId,
                Title = c.Title,
                Description = c.Description,
                CategoryId = c.CategoryId,
                Status = c.Status
            })
            .ToListAsync(cancellationToken);

        return Result.Success(new PagedResult<CourseSummaryResponse>(items, totalCount, query.Page, query.PageSize));
    }

    public async Task<Result<CourseDetailResponse>> GetByIdAsync(int courseId, CancellationToken cancellationToken = default)
    {
        var course = await _dbContext.Courses
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.CourseId == courseId, cancellationToken);

        if (course == null || course.DeleteFlag)
            return Result.Failure<CourseDetailResponse>("CourseNotFound");

        var response = new CourseDetailResponse
        {
            CourseId = course.CourseId,
            Title = course.Title,
            Description = course.Description,
            CategoryId = course.CategoryId,
            Status = course.Status,
            CreatedAt = course.CreatedAt,
            UpdatedAt = course.UpdatedAt,
            CreatedBy = course.CreatedBy,
            ThumbnailUrl = course.ThumbnailUrl
        };

        return Result.Success(response);
    }

    public async Task<Result<CourseDetailResponse>> CreateAsync(CreateCourseRequest request, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.Title))
            return Result.Failure<CourseDetailResponse>("InvalidCourseTitle");

        var userId = _currentUserService.UserId ?? throw new UnauthorizedAccessException();

        var course = new Course
        {
            Title = request.Title.Trim(),
            Description = request.Description?.Trim(),
            CategoryId = request.CategoryId,
            Status = true,
            DeleteFlag = false,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = userId
        };

        _dbContext.Courses.Add(course);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return await GetByIdAsync(course.CourseId, cancellationToken);
    }

    public async Task<Result<CourseDetailResponse>> UpdateAsync(int courseId, UpdateCourseRequest request, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.Title))
            return Result.Failure<CourseDetailResponse>("InvalidCourseTitle");

        var course = await _dbContext.Courses.FirstOrDefaultAsync(c => c.CourseId == courseId, cancellationToken);
        if (course == null || course.DeleteFlag)
            return Result.Failure<CourseDetailResponse>("CourseNotFound");

        course.Title = request.Title.Trim();
        course.Description = request.Description?.Trim();
        course.CategoryId = request.CategoryId;
        course.UpdatedAt = DateTime.UtcNow;

        await _dbContext.SaveChangesAsync(cancellationToken);

        return await GetByIdAsync(course.CourseId, cancellationToken);
    }

    public async Task<Result<bool>> SoftDeleteAsync(int courseId, CancellationToken cancellationToken = default)
    {
        var course = await _dbContext.Courses.FirstOrDefaultAsync(c => c.CourseId == courseId, cancellationToken);
        if (course == null || course.DeleteFlag)
            return Result.Failure<bool>("CourseNotFound");

        course.DeleteFlag = true;
        course.UpdatedAt = DateTime.UtcNow;

        await _dbContext.SaveChangesAsync(cancellationToken);

        return Result.Success(true);
    }
}
