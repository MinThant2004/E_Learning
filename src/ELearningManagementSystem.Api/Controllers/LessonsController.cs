using System.Threading;
using System.Threading.Tasks;
using ELearningManagementSystem.Application.Features.Lessons.DTOs;
using ELearningManagementSystem.Application.Features.Lessons.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ELearningManagementSystem.Api.Controllers;

[ApiController]
[Route("api/courses/{courseId}/lessons")]
public class LessonsController : ControllerBase
{
    private readonly ILessonService _lessonService;

    public LessonsController(ILessonService lessonService)
    {
        _lessonService = lessonService;
    }

    /// <summary>GET /api/courses/{courseId}/lessons — Get lessons for a course with optional status filter</summary>
    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> GetLessons(int courseId, [FromQuery] int page = 1, [FromQuery] int pageSize = 10, [FromQuery] string? searchTerm = null, [FromQuery] bool? isArchived = null, CancellationToken cancellationToken = default)
    {
        var hasLessonReadPermission = User.Claims.Any(c => c.Type == "permission" && c.Value == "Lesson.Read");
        if (!hasLessonReadPermission)
        {
            isArchived = false;
        }

        var query = new LessonListQuery
        {
            CourseId = courseId,
            Page = page,
            PageSize = pageSize,
            SearchTerm = searchTerm,
            IsArchived = isArchived
        };

        var result = await _lessonService.GetPagedListAsync(query, cancellationToken);
        if (result.IsFailure)
        {
            if (result.Error == "CourseNotFound") return NotFound(new { Error = result.Error });
            return BadRequest(new { Error = result.Error });
        }
        return Ok(result.Value);
    }

    /// <summary>GET /api/courses/{courseId}/lessons/{lessonId} — Get lesson detail</summary>
    [HttpGet("{lessonId}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetLesson(int courseId, int lessonId, CancellationToken cancellationToken)
    {
        var result = await _lessonService.GetLessonByIdAsync(courseId, lessonId, cancellationToken);
        if (result.IsFailure) return NotFound(new { Error = result.Error });
        return Ok(result.Value);
    }

    /// <summary>POST /api/courses/{courseId}/lessons — Create a new lesson</summary>
    [HttpPost]
    [Authorize(Policy = "Permission:Lesson.Create")]
    public async Task<IActionResult> CreateLesson(int courseId, [FromBody] CreateLessonRequest request, CancellationToken cancellationToken)
    {
        if (request.CourseId != courseId)
            return BadRequest(new { Error = "CourseIdMismatch" });

        var result = await _lessonService.CreateAsync(request, cancellationToken);
        if (result.IsFailure) return BadRequest(new { Error = result.Error });
        return Created($"/api/courses/{courseId}/lessons/{result.Value!.LessonId}", result.Value);
    }

    /// <summary>PUT /api/courses/{courseId}/lessons/{lessonId} — Update a lesson and display order</summary>
    [HttpPut("{lessonId}")]
    [Authorize(Policy = "Permission:Lesson.Update")]
    public async Task<IActionResult> UpdateLesson(int courseId, int lessonId, [FromBody] UpdateLessonRequest request, CancellationToken cancellationToken)
    {
        var result = await _lessonService.UpdateAsync(lessonId, request, cancellationToken);
        if (result.IsFailure)
        {
            if (result.Error == "LessonNotFound") return NotFound(new { Error = result.Error });
            return BadRequest(new { Error = result.Error });
        }
        return Ok(result.Value);
    }

    /// <summary>POST /api/courses/{courseId}/lessons/{lessonId}/archive — Archive a lesson (sets DeleteFlag = 1)</summary>
    [HttpPost("{lessonId}/archive")]
    [Authorize(Policy = "Permission:Lesson.Delete")]
    public async Task<IActionResult> ArchiveLesson(int courseId, int lessonId, CancellationToken cancellationToken)
    {
        var result = await _lessonService.ArchiveLessonAsync(lessonId, cancellationToken);
        if (result.IsFailure)
        {
            if (result.Error == "LessonNotFound") return NotFound(new { Error = result.Error });
            return BadRequest(new { Error = result.Error });
        }
        return Ok(new { Message = "Lesson archived successfully." });
    }

    /// <summary>POST /api/courses/{courseId}/lessons/{lessonId}/restore — Restore a lesson (sets DeleteFlag = 0)</summary>
    [HttpPost("{lessonId}/restore")]
    [Authorize(Policy = "Permission:Lesson.Update")]
    public async Task<IActionResult> RestoreLesson(int courseId, int lessonId, CancellationToken cancellationToken)
    {
        var result = await _lessonService.RestoreLessonAsync(lessonId, cancellationToken);
        if (result.IsFailure)
        {
            if (result.Error == "LessonNotFound") return NotFound(new { Error = result.Error });
            return BadRequest(new { Error = result.Error });
        }
        return Ok(new { Message = "Lesson restored successfully." });
    }
}
