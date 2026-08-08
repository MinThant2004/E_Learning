using ELearningManagementSystem.Application.Features.StudentDashboard.Services;
using ELearningManagementSystem.Application.Features.StudentDashboard.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ELearningManagementSystem.Api.Controllers;

[ApiController]
[Route("api/student/dashboard")]
[Authorize]
public class StudentDashboardController : ControllerBase
{
    private readonly IStudentDashboardService _studentDashboardService;

    public StudentDashboardController(IStudentDashboardService studentDashboardService)
    {
        _studentDashboardService = studentDashboardService;
    }

    [HttpGet]
    public async Task<IActionResult> GetDashboard()
    {
        var result = await _studentDashboardService.GetDashboardSummaryAsync();
        
        if (result.IsSuccess)
        {
            return Ok(result.Value);
        }

        if (result.Error != null && result.Error.StartsWith("AccountNotFound"))
        {
            return Unauthorized(result);
        }

        return BadRequest(result);
    }
}
