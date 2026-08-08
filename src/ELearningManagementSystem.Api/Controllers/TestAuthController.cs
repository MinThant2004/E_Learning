using ELearningManagementSystem.Infrastructure.Security.Attributes;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ELearningManagementSystem.Api.Controllers;

[ApiController]
[Route("api/test-auth")]
[Authorize] // All endpoints require authentication by default
public class TestAuthController : ControllerBase
{
    [HttpGet("authenticated")]
    public IActionResult AuthenticatedOnly()
    {
        return Ok(new { Message = "Access granted to authenticated user." });
    }

    [HttpGet("course-read")]
    [HasPermission("Course.Read")]
    public IActionResult CourseRead()
    {
        return Ok(new { Message = "Access granted: You have Course.Read permission." });
    }

    [HttpGet("course-create")]
    [HasPermission("Course.Create")]
    public IActionResult CourseCreate()
    {
        return Ok(new { Message = "Access granted: You have Course.Create permission." });
    }

    [HttpGet("role-delete")]
    [HasPermission("Role.Delete")]
    public IActionResult RoleDelete()
    {
        return Ok(new { Message = "Access granted: You have Role.Delete permission." });
    }
}
