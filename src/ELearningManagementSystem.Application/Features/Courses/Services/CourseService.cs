using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ELearningManagementSystem.Application.Common;
using ELearningManagementSystem.Application.Features.Courses.DTOs;
using ELearningManagementSystem.Application.Features.AuditLogs.DTOs;
using ELearningManagementSystem.Application.Features.AuditLogs.Services;
using ELearningManagementSystem.Application.Interfaces;
using ELearningManagementSystem.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ELearningManagementSystem.Application.Features.Courses.Services;

public class CourseService : ICourseService
{
    private readonly IAppDbContext _dbContext;
    private readonly ICurrentUserService _currentUserService;
    private readonly IPermissionService _permissionService;
    private readonly IAuditLogService _auditLogService;
    private readonly ILogger<CourseService> _logger;

    public CourseService(IAppDbContext dbContext, ICurrentUserService currentUserService, IPermissionService permissionService, IAuditLogService auditLogService, ILogger<CourseService> logger)
    {
        _dbContext = dbContext;
        _currentUserService = currentUserService;
        _permissionService = permissionService;
        _auditLogService = auditLogService;
        _logger = logger;
    }

    public async Task<Result<PagedResult<CourseSummaryResponse>>> GetPagedListAsync(CourseListQuery query, CancellationToken cancellationToken = default)
    {
        var userId = _currentUserService.UserId;
        if (!userId.HasValue || !await _permissionService.HasPermissionAsync(userId.Value, "Course.Read", cancellationToken))
        {
            return Result.Failure<PagedResult<CourseSummaryResponse>>("PermissionDenied");
        }
        var dbQuery = _dbContext.Courses
            .AsNoTracking()
            .AsQueryable();

        if (query.IsArchived.HasValue)
        {
            dbQuery = dbQuery.Where(c => c.DeleteFlag == query.IsArchived.Value);
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
                CategoryName = c.Category.CategoryName,
                Status = c.Status,
                ThumbnailUrl = c.ThumbnailUrl,
                DeleteFlag = c.DeleteFlag
            })
            .ToListAsync(cancellationToken);

        return Result.Success(new PagedResult<CourseSummaryResponse>(items, totalCount, query.Page, query.PageSize));
    }

    public async Task<Result<CourseDetailResponse>> GetByIdAsync(int courseId, CancellationToken cancellationToken = default)
    {
        var userId = _currentUserService.UserId;
        if (!userId.HasValue || !await _permissionService.HasPermissionAsync(userId.Value, "Course.Read", cancellationToken))
        {
            return Result.Failure<CourseDetailResponse>("PermissionDenied");
        }

        var course = await _dbContext.Courses
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.CourseId == courseId, cancellationToken);

        if (course == null)
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
            ThumbnailUrl = course.ThumbnailUrl,
            DeleteFlag = course.DeleteFlag
        };

        return Result.Success(response);
    }

    public async Task<Result<CourseDetailResponse>> CreateAsync(CreateCourseRequest request, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.Title))
            return Result.Failure<CourseDetailResponse>("InvalidCourseTitle");

        var userId = _currentUserService.UserId ?? throw new UnauthorizedAccessException();
        if (!await _permissionService.HasPermissionAsync(userId, "Course.Create", cancellationToken))
        {
            return Result.Failure<CourseDetailResponse>("PermissionDenied");
        }

        // Optional check: cannot create course with archived category
        var category = await _dbContext.Categories
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.CategoryId == request.CategoryId, cancellationToken);

        if (category == null || category.DeleteFlag)
            return Result.Failure<CourseDetailResponse>("CategoryNotFound");

        var course = new Course
        {
            Title = request.Title.Trim(),
            Description = request.Description?.Trim(),
            CategoryId = request.CategoryId,
            ThumbnailUrl = request.ThumbnailUrl,
            Status = true,
            DeleteFlag = false,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = userId
        };

        _dbContext.Courses.Add(course);
        await _dbContext.SaveChangesAsync(cancellationToken);

        await _auditLogService.CreateAuditLogAsync(new CreateAuditLogRequest
        {
            UserId = userId,
            Action = "Create",
            TableName = "Courses",
            RecordId = course.CourseId
        }, cancellationToken);
        
        _logger.LogInformation("User {UserId} created Course {CourseId} (Title={Title})", userId, course.CourseId, course.Title);

        return await GetByIdAsync(course.CourseId, cancellationToken);
    }

    public async Task<Result<CourseDetailResponse>> UpdateAsync(int courseId, UpdateCourseRequest request, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.Title))
            return Result.Failure<CourseDetailResponse>("InvalidCourseTitle");

        var userId = _currentUserService.UserId ?? throw new UnauthorizedAccessException();
        if (!await _permissionService.HasPermissionAsync(userId, "Course.Update", cancellationToken))
        {
            return Result.Failure<CourseDetailResponse>("PermissionDenied");
        }

        var course = await _dbContext.Courses.FirstOrDefaultAsync(c => c.CourseId == courseId, cancellationToken);
        if (course == null || course.DeleteFlag)
            return Result.Failure<CourseDetailResponse>("CourseNotFound");

        // Optional check: cannot update course with archived category
        var category = await _dbContext.Categories
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.CategoryId == request.CategoryId, cancellationToken);

        if (category == null || category.DeleteFlag)
            return Result.Failure<CourseDetailResponse>("CategoryNotFound");

        course.Title = request.Title.Trim();
        course.Description = request.Description?.Trim();
        course.CategoryId = request.CategoryId;
        course.UpdatedAt = DateTime.UtcNow;

        if (request.RemoveThumbnail)
        {
            course.ThumbnailUrl = null;
        }
        else if (request.ThumbnailUrl != null)
        {
            course.ThumbnailUrl = request.ThumbnailUrl;
        }

        await _dbContext.SaveChangesAsync(cancellationToken);

        await _auditLogService.CreateAuditLogAsync(new CreateAuditLogRequest
        {
            UserId = userId,
            Action = "Update",
            TableName = "Courses",
            RecordId = course.CourseId
        }, cancellationToken);

        _logger.LogInformation("User {UserId} updated Course {CourseId} (Title={Title})", userId, course.CourseId, course.Title);

        return await GetByIdAsync(course.CourseId, cancellationToken);
    }

    public async Task<Result<bool>> ArchiveCourseAsync(int courseId, CancellationToken cancellationToken = default)
    {
        var userId = _currentUserService.UserId;
        if (!userId.HasValue || !await _permissionService.HasPermissionAsync(userId.Value, "Course.Delete", cancellationToken))
        {
            return Result.Failure<bool>("PermissionDenied");
        }

        var course = await _dbContext.Courses.FirstOrDefaultAsync(c => c.CourseId == courseId && !c.DeleteFlag, cancellationToken);
        if (course == null)
            return Result.Failure<bool>("CourseNotFound");

        course.DeleteFlag = true;
        course.UpdatedAt = DateTime.UtcNow;

        await _dbContext.SaveChangesAsync(cancellationToken);

        if (userId.HasValue)
        {
            await _auditLogService.CreateAuditLogAsync(new CreateAuditLogRequest
            {
                UserId = userId.Value,
                Action = "Archive",
                TableName = "Courses",
                RecordId = course.CourseId
            }, cancellationToken);
            
            _logger.LogInformation("User {UserId} archived Course {CourseId}", userId.Value, courseId);
        }

        return Result.Success(true);
    }

    public async Task<Result<bool>> RestoreCourseAsync(int courseId, CancellationToken cancellationToken = default)
    {
        var userId = _currentUserService.UserId;
        if (!userId.HasValue || !await _permissionService.HasPermissionAsync(userId.Value, "Course.Update", cancellationToken))
        {
            return Result.Failure<bool>("PermissionDenied");
        }

        var course = await _dbContext.Courses
            .Include(c => c.Category)
            .FirstOrDefaultAsync(c => c.CourseId == courseId && c.DeleteFlag, cancellationToken);

        if (course == null)
            return Result.Failure<bool>("CourseNotFound");

        // Business Rule: If restoring a Course references an archived Category, do not silently restore
        if (course.Category.DeleteFlag)
            return Result.Failure<bool>("CannotRestoreArchivedCategory");

        course.DeleteFlag = false;
        course.UpdatedAt = DateTime.UtcNow;

        await _dbContext.SaveChangesAsync(cancellationToken);

        if (userId.HasValue)
        {
            await _auditLogService.CreateAuditLogAsync(new CreateAuditLogRequest
            {
                UserId = userId.Value,
                Action = "Restore",
                TableName = "Courses",
                RecordId = course.CourseId
            }, cancellationToken);

            _logger.LogInformation("User {UserId} restored Course {CourseId}", userId.Value, courseId);
        }

        return Result.Success(true);
    }
}
