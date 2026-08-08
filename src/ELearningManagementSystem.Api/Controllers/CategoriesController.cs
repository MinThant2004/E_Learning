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

    /// <summary>GET /api/categories — Fetch active categories</summary>
    [HttpGet]
    [Authorize(Policy = "Permission:Category.Read")]
    public async Task<IActionResult> GetCategories(CancellationToken cancellationToken)
    {
        var result = await _categoryService.GetAllActiveAsync(cancellationToken);
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

    /// <summary>DELETE /api/categories/{id} — Soft-delete a category</summary>
    [HttpDelete("{id}")]
    [Authorize(Policy = "Permission:Category.Delete")]
    public async Task<IActionResult> DeleteCategory(int id, CancellationToken cancellationToken)
    {
        var result = await _categoryService.SoftDeleteAsync(id, cancellationToken);
        if (result.IsFailure)
        {
            if (result.Error == "CategoryNotFound") return NotFound(new { Error = result.Error });
            return BadRequest(new { Error = result.Error });
        }
        return Ok(new { Message = "Category deleted successfully." });
    }
}
