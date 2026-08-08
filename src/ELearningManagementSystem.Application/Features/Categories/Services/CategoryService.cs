using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ELearningManagementSystem.Application.Common;
using ELearningManagementSystem.Application.Features.Categories.DTOs;
using ELearningManagementSystem.Application.Interfaces;
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
            .OrderBy(c => c.CategoryName)
            .Select(c => new CategoryResponse
            {
                CategoryId = c.CategoryId,
                CategoryName = c.CategoryName,
                Description = c.Description
            })
            .ToListAsync(cancellationToken);

        return Result.Success<IEnumerable<CategoryResponse>>(categories);
    }
}
