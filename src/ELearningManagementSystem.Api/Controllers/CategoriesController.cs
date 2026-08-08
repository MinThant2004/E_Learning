using System.Threading;
using System.Threading.Tasks;
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
    [AllowAnonymous]
    public async Task<IActionResult> GetCategories(CancellationToken cancellationToken)
    {
        var result = await _categoryService.GetAllActiveAsync(cancellationToken);
        if (result.IsFailure) return BadRequest(new { Error = result.Error });
        return Ok(result.Value);
    }
}
