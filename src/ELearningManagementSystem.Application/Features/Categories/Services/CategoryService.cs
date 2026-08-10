using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ELearningManagementSystem.Application.Common;
using ELearningManagementSystem.Application.Features.Categories.DTOs;
using ELearningManagementSystem.Application.Features.AuditLogs.DTOs;
using ELearningManagementSystem.Application.Features.AuditLogs.Services;
using ELearningManagementSystem.Application.Interfaces;
using ELearningManagementSystem.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Caching.Memory;

namespace ELearningManagementSystem.Application.Features.Categories.Services;

public class CategoryService : ICategoryService
{
    private readonly IAppDbContext _context;
    private readonly IAuditLogService _auditLogService;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<CategoryService> _logger;
    private readonly IMemoryCache _cache;
    private const string CategoriesCacheKey = "all-categories";

    public CategoryService(IAppDbContext context, IAuditLogService auditLogService, ICurrentUserService currentUserService, ILogger<CategoryService> logger, IMemoryCache cache)
    {
        _context = context;
        _auditLogService = auditLogService;
        _currentUserService = currentUserService;
        _logger = logger;
        _cache = cache;
    }

    public async Task<Result<IEnumerable<CategoryResponse>>> GetCategoriesAsync(bool? isArchived = false, CancellationToken cancellationToken = default)
    {
        if (!_cache.TryGetValue(CategoriesCacheKey, out List<CategoryResponse>? allCategories) || allCategories == null)
        {
            allCategories = await _context.Categories
                .AsNoTracking()
                .OrderByDescending(c => c.CreatedAt)
                .Select(c => new CategoryResponse
                {
                    CategoryId = c.CategoryId,
                    CategoryName = c.CategoryName,
                    Description = c.Description,
                    CreatedAt = c.CreatedAt,
                    DeleteFlag = c.DeleteFlag
                })
                .ToListAsync(cancellationToken);

            _cache.Set(CategoriesCacheKey, allCategories, TimeSpan.FromMinutes(10));
        }

        var result = allCategories.AsEnumerable();

        if (isArchived.HasValue)
        {
            result = result.Where(c => c.DeleteFlag == isArchived.Value);
        }

        return Result.Success(result);
    }

    public async Task<Result<PagedResult<CategoryResponse>>> GetPagedListAsync(CategoryListQuery queryDto, CancellationToken cancellationToken = default)
    {
        IQueryable<Category> query = _context.Categories.AsNoTracking();

        if (queryDto.IsArchived.HasValue)
        {
            query = query.Where(c => c.DeleteFlag == queryDto.IsArchived.Value);
        }

        if (!string.IsNullOrWhiteSpace(queryDto.SearchTerm))
        {
            var search = queryDto.SearchTerm.ToLower();
            query = query.Where(c => c.CategoryName.ToLower().Contains(search) || (c.Description != null && c.Description.ToLower().Contains(search)));
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var categories = await query
            .OrderByDescending(c => c.CreatedAt)
            .Skip((queryDto.Page - 1) * queryDto.PageSize)
            .Take(queryDto.PageSize)
            .Select(c => new CategoryResponse
            {
                CategoryId = c.CategoryId,
                CategoryName = c.CategoryName,
                Description = c.Description,
                CreatedAt = c.CreatedAt,
                DeleteFlag = c.DeleteFlag
            })
            .ToListAsync(cancellationToken);

        return Result.Success(new PagedResult<CategoryResponse>(categories, totalCount, queryDto.Page, queryDto.PageSize));
    }

    public async Task<Result<CategoryResponse>> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var category = await _context.Categories
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.CategoryId == id, cancellationToken);

        if (category == null)
            return Result.Failure<CategoryResponse>("CategoryNotFound");

        var response = new CategoryResponse
        {
            CategoryId = category.CategoryId,
            CategoryName = category.CategoryName,
            Description = category.Description,
            CreatedAt = category.CreatedAt,
            DeleteFlag = category.DeleteFlag
        };

        return Result.Success(response);
    }

    public async Task<Result<CategoryResponse>> CreateAsync(CreateCategoryRequest request, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.CategoryName))
            return Result.Failure<CategoryResponse>("CategoryNameRequired");

        var normalizedName = request.CategoryName.Trim();

        // Check for duplicate category names (excluding soft-deleted ones)
        var exists = await _context.Categories
            .AnyAsync(c => c.CategoryName.ToLower() == normalizedName.ToLower() && !c.DeleteFlag, cancellationToken);

        if (exists)
            return Result.Failure<CategoryResponse>("CategoryAlreadyExists");

        var category = new Category
        {
            CategoryName = normalizedName,
            Description = request.Description?.Trim(),
            CreatedAt = DateTime.UtcNow,
            DeleteFlag = false
        };

        _context.Categories.Add(category);
        await _context.SaveChangesAsync(cancellationToken);
        _cache.Remove(CategoriesCacheKey);

        if (_currentUserService.UserId.HasValue)
        {
            await _auditLogService.CreateAuditLogAsync(new CreateAuditLogRequest
            {
                UserId = _currentUserService.UserId.Value,
                Action = "Create",
                TableName = "Categories",
                RecordId = category.CategoryId
            }, cancellationToken);
            
            _logger.LogInformation("User {UserId} created Category {CategoryId} (Name={CategoryName})", _currentUserService.UserId.Value, category.CategoryId, category.CategoryName);
        }

        return await GetByIdAsync(category.CategoryId, cancellationToken);
    }

    public async Task<Result<CategoryResponse>> UpdateAsync(int id, UpdateCategoryRequest request, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.CategoryName))
            return Result.Failure<CategoryResponse>("CategoryNameRequired");

        var category = await _context.Categories
            .FirstOrDefaultAsync(c => c.CategoryId == id && !c.DeleteFlag, cancellationToken);

        if (category == null)
            return Result.Failure<CategoryResponse>("CategoryNotFound");

        var normalizedName = request.CategoryName.Trim();

        // Check for duplicate names on other categories
        var nameExistsOnOther = await _context.Categories
            .AnyAsync(c => c.CategoryId != id && c.CategoryName.ToLower() == normalizedName.ToLower() && !c.DeleteFlag, cancellationToken);

        if (nameExistsOnOther)
            return Result.Failure<CategoryResponse>("CategoryAlreadyExists");

        category.CategoryName = normalizedName;
        category.Description = request.Description?.Trim();
        category.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);
        _cache.Remove(CategoriesCacheKey);

        if (_currentUserService.UserId.HasValue)
        {
            await _auditLogService.CreateAuditLogAsync(new CreateAuditLogRequest
            {
                UserId = _currentUserService.UserId.Value,
                Action = "Update",
                TableName = "Categories",
                RecordId = category.CategoryId
            }, cancellationToken);
            
            _logger.LogInformation("User {UserId} updated Category {CategoryId} (Name={CategoryName})", _currentUserService.UserId.Value, category.CategoryId, category.CategoryName);
        }

        return await GetByIdAsync(category.CategoryId, cancellationToken);
    }

    public async Task<Result<bool>> ArchiveCategoryAsync(int id, CancellationToken cancellationToken = default)
    {
        var category = await _context.Categories
            .FirstOrDefaultAsync(c => c.CategoryId == id && !c.DeleteFlag, cancellationToken);

        if (category == null)
            return Result.Failure<bool>("CategoryNotFound");

        // Check if any course is using this category (active courses)
        var isUsed = await _context.Courses
            .AnyAsync(c => c.CategoryId == id && !c.DeleteFlag, cancellationToken);

        if (isUsed)
            return Result.Failure<bool>("CategoryInUse");

        category.DeleteFlag = true;
        category.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);
        _cache.Remove(CategoriesCacheKey);

        if (_currentUserService.UserId.HasValue)
        {
            await _auditLogService.CreateAuditLogAsync(new CreateAuditLogRequest
            {
                UserId = _currentUserService.UserId.Value,
                Action = "Archive",
                TableName = "Categories",
                RecordId = category.CategoryId
            }, cancellationToken);
            
            _logger.LogInformation("User {UserId} archived Category {CategoryId}", _currentUserService.UserId.Value, category.CategoryId);
        }

        return Result.Success(true);
    }

    public async Task<Result<bool>> RestoreCategoryAsync(int id, CancellationToken cancellationToken = default)
    {
        var category = await _context.Categories
            .FirstOrDefaultAsync(c => c.CategoryId == id && c.DeleteFlag, cancellationToken);

        if (category == null)
            return Result.Failure<bool>("CategoryNotFound");

        category.DeleteFlag = false;
        category.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);
        _cache.Remove(CategoriesCacheKey);

        if (_currentUserService.UserId.HasValue)
        {
            await _auditLogService.CreateAuditLogAsync(new CreateAuditLogRequest
            {
                UserId = _currentUserService.UserId.Value,
                Action = "Restore",
                TableName = "Categories",
                RecordId = category.CategoryId
            }, cancellationToken);
            
            _logger.LogInformation("User {UserId} restored Category {CategoryId}", _currentUserService.UserId.Value, category.CategoryId);
        }

        return Result.Success(true);
    }
}
