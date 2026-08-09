using ELearningManagementSystem.Application.Features.AdminDashboard.Services;
using ELearningManagementSystem.Application.Features.AdminDashboard.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ELearningManagementSystem.Api.Controllers;

[ApiController]
[Route("api/admin/dashboard")]
[Authorize]
public class AdminDashboardController : ControllerBase
{
    private readonly IAdminDashboardService _adminDashboardService;

    public AdminDashboardController(IAdminDashboardService adminDashboardService)
    {
        _adminDashboardService = adminDashboardService;
    }

    [HttpGet]
    public async Task<IActionResult> GetDashboard()
    {
        var result = await _adminDashboardService.GetDashboardAsync();
        
        if (result.IsSuccess)
        {
            return Ok(result.Value);
        }

        if (result.Error != null && result.Error.StartsWith("PermissionDenied"))
        {
            return StatusCode(403, result);
        }

        if (result.Error != null && result.Error.StartsWith("AccountNotFound"))
        {
            return Unauthorized(result);
        }

        return BadRequest(result);
    }
}
