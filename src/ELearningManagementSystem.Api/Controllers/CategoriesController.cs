using System.Threading;
using System.Threading.Tasks;
using ELearningManagementSystem.Application.Features.Categories.DTOs;
using ELearningManagementSystem.Application.Features.Categories.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ELearningManagementSystem.Api.Controllers;

[ApiController]
[Route("api/categories")]
public class CategoriesController : ControllerBase
{
    private readonly ICategoryService _categoryService;

    public CategoriesController(ICategoryService categoryService)
    {
        _categoryService = categoryService;
    }

    /// <summary>GET /api/categories — Fetch categories with optional status filtering</summary>
    [HttpGet]
    [Authorize(Policy = "Permission:Category.Read")]
    public async Task<IActionResult> GetCategories([FromQuery] CategoryListQuery query, CancellationToken cancellationToken)
    {
        var result = await _categoryService.GetPagedListAsync(query, cancellationToken);
        if (result.IsFailure) return BadRequest(new { Error = result.Error });
        return Ok(result.Value);
    }

    /// <summary>GET /api/categories/{id} — Get a category by ID</summary>
    [HttpGet("{id}")]
    [Authorize(Policy = "Permission:Category.Read")]
    public async Task<IActionResult> GetCategory(int id, CancellationToken cancellationToken)
    {
        var result = await _categoryService.GetByIdAsync(id, cancellationToken);
        if (result.IsFailure) return NotFound(new { Error = result.Error });
        return Ok(result.Value);
    }

    /// <summary>POST /api/categories — Create a category</summary>
    [HttpPost]
    [Authorize(Policy = "Permission:Category.Create")]
    public async Task<IActionResult> CreateCategory([FromBody] CreateCategoryRequest request, CancellationToken cancellationToken)
    {
        var result = await _categoryService.CreateAsync(request, cancellationToken);
        if (result.IsFailure) return BadRequest(new { Error = result.Error });
        return Created($"/api/categories/{result.Value!.CategoryId}", result.Value);
    }

    /// <summary>PUT /api/categories/{id} — Update a category</summary>
    [HttpPut("{id}")]
    [Authorize(Policy = "Permission:Category.Update")]
    public async Task<IActionResult> UpdateCategory(int id, [FromBody] UpdateCategoryRequest request, CancellationToken cancellationToken)
    {
        var result = await _categoryService.UpdateAsync(id, request, cancellationToken);
        if (result.IsFailure)
        {
            if (result.Error == "CategoryNotFound") return NotFound(new { Error = result.Error });
            return BadRequest(new { Error = result.Error });
        }
        return Ok(result.Value);
    }

    /// <summary>POST /api/categories/{id}/archive — Archive a category (sets DeleteFlag = 1)</summary>
    [HttpPost("{id}/archive")]
    [Authorize(Policy = "Permission:Category.Delete")]
    public async Task<IActionResult> ArchiveCategory(int id, CancellationToken cancellationToken)
    {
        var result = await _categoryService.ArchiveCategoryAsync(id, cancellationToken);
        if (result.IsFailure)
        {
            if (result.Error == "CategoryNotFound") return NotFound(new { Error = result.Error });
            return BadRequest(new { Error = result.Error });
        }
        return Ok(new { Message = "Category archived successfully." });
    }

    /// <summary>POST /api/categories/{id}/restore — Restore a category (sets DeleteFlag = 0)</summary>
    [HttpPost("{id}/restore")]
    [Authorize(Policy = "Permission:Category.Update")]
    public async Task<IActionResult> RestoreCategory(int id, CancellationToken cancellationToken)
    {
        var result = await _categoryService.RestoreCategoryAsync(id, cancellationToken);
        if (result.IsFailure)
        {
            if (result.Error == "CategoryNotFound") return NotFound(new { Error = result.Error });
            return BadRequest(new { Error = result.Error });
        }
        return Ok(new { Message = "Category restored successfully." });
    }
}
