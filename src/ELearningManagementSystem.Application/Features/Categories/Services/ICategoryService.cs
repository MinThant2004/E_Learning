using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using ELearningManagementSystem.Application.Common;
using ELearningManagementSystem.Application.Features.Categories.DTOs;

namespace ELearningManagementSystem.Application.Features.Categories.Services;

public interface ICategoryService
{
    Task<Result<IEnumerable<CategoryResponse>>> GetCategoriesAsync(bool? isArchived = false, CancellationToken cancellationToken = default);
    Task<Result<CategoryResponse>> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<Result<CategoryResponse>> CreateAsync(CreateCategoryRequest request, CancellationToken cancellationToken = default);
    Task<Result<CategoryResponse>> UpdateAsync(int id, UpdateCategoryRequest request, CancellationToken cancellationToken = default);
    Task<Result<bool>> ArchiveCategoryAsync(int id, CancellationToken cancellationToken = default);
    Task<Result<bool>> RestoreCategoryAsync(int id, CancellationToken cancellationToken = default);
}
