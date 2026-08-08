using ELearningManagementSystem.Application.Features.Courses.DTOs;
using ELearningManagementSystem.Application.Features.Courses.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ELearningManagementSystem.Api.Controllers;

[ApiController]
[Route("api/courses")]
public class CoursesController : ControllerBase
{
    private readonly ICourseService _courseService;

    public CoursesController(ICourseService courseService)
    {
        _courseService = courseService;
    }

    /// <summary>GET /api/courses — Browse active courses</summary>
    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> GetCourses([FromQuery] CourseListQuery query, CancellationToken cancellationToken)
    {
        var result = await _courseService.GetPagedListAsync(query, cancellationToken);
        if (result.IsFailure) return BadRequest(new { Error = result.Error });
        return Ok(result.Value);
    }

    /// <summary>GET /api/courses/{id} — View one active course</summary>
    [HttpGet("{id}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetCourse(int id, CancellationToken cancellationToken)
    {
        var result = await _courseService.GetByIdAsync(id, cancellationToken);
        if (result.IsFailure) return NotFound(new { Error = result.Error });
        return Ok(result.Value);
    }

    /// <summary>POST /api/courses — Create a course</summary>
    [HttpPost]
    [Authorize(Policy = "Permission:Course.Create")]
    public async Task<IActionResult> CreateCourse([FromBody] CreateCourseRequest request, CancellationToken cancellationToken)
    {
        var result = await _courseService.CreateAsync(request, cancellationToken);
        if (result.IsFailure) return BadRequest(new { Error = result.Error });
        return CreatedAtAction(nameof(GetCourse), new { id = result.Value!.CourseId }, result.Value);
    }

    /// <summary>PUT /api/courses/{id} — Update a course</summary>
    [HttpPut("{id}")]
    [Authorize(Policy = "Permission:Course.Update")]
    public async Task<IActionResult> UpdateCourse(int id, [FromBody] UpdateCourseRequest request, CancellationToken cancellationToken)
    {
        var result = await _courseService.UpdateAsync(id, request, cancellationToken);
        if (result.IsFailure)
        {
            if (result.Error == "CourseNotFound") return NotFound(new { Error = result.Error });
            return BadRequest(new { Error = result.Error });
        }
        return Ok(result.Value);
    }

    /// <summary>DELETE /api/courses/{id} — Soft-delete a course</summary>
    [HttpDelete("{id}")]
    [Authorize(Policy = "Permission:Course.Delete")]
    public async Task<IActionResult> DeleteCourse(int id, CancellationToken cancellationToken)
    {
        var result = await _courseService.SoftDeleteAsync(id, cancellationToken);
        if (result.IsFailure)
        {
            if (result.Error == "CourseNotFound") return NotFound(new { Error = result.Error });
            return BadRequest(new { Error = result.Error });
        }
        return Ok(new { Message = "Course deleted successfully." });
    }
}
