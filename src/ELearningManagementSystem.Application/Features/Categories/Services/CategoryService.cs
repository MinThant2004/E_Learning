using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ELearningManagementSystem.Application.Common;
using ELearningManagementSystem.Application.Features.Categories.DTOs;
using ELearningManagementSystem.Application.Interfaces;
using ELearningManagementSystem.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace ELearningManagementSystem.Application.Features.Categories.Services;

public class CategoryService : ICategoryService
{
    private readonly IAppDbContext _context;

    public CategoryService(IAppDbContext context)
    {
        _context = context;
    }

    public async Task<Result<IEnumerable<CategoryResponse>>> GetAllActiveAsync(CancellationToken cancellationToken = default)
    {
        var categories = await _context.Categories
            .AsNoTracking()
            .Where(c => !c.DeleteFlag)
            .OrderByDescending(c => c.CreatedAt)
            .Select(c => new CategoryResponse
            {
                CategoryId = c.CategoryId,
                CategoryName = c.CategoryName,
                Description = c.Description,
                CreatedAt = c.CreatedAt
            })
            .ToListAsync(cancellationToken);

        return Result.Success<IEnumerable<CategoryResponse>>(categories);
    }

    public async Task<Result<CategoryResponse>> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var category = await _context.Categories
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.CategoryId == id && !c.DeleteFlag, cancellationToken);

        if (category == null)
            return Result.Failure<CategoryResponse>("CategoryNotFound");

        var response = new CategoryResponse
        {
            CategoryId = category.CategoryId,
            CategoryName = category.CategoryName,
            Description = category.Description,
            CreatedAt = category.CreatedAt
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

        return await GetByIdAsync(category.CategoryId, cancellationToken);
    }

    public async Task<Result<bool>> SoftDeleteAsync(int id, CancellationToken cancellationToken = default)
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

        return Result.Success(true);
    }
}
