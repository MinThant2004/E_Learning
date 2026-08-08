using ELearningManagementSystem.Application.Features.LessonProgress.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace ELearningManagementSystem.Api.Controllers;

[ApiController]
[Route("api/courses/{courseId}")]
[Authorize]
public class LessonProgressController : ControllerBase
{
    private readonly ILessonProgressService _progressService;

    public LessonProgressController(ILessonProgressService progressService)
    {
        _progressService = progressService;
    }

    [HttpGet("progress")]
    public async Task<IActionResult> GetProgress(int courseId, CancellationToken cancellationToken)
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var result = await _progressService.GetCourseProgressAsync(courseId, userId, cancellationToken);
        if (result.IsSuccess)
        {
            return Ok(result.Value);
        }
        return BadRequest(new { Error = result.Error });
    }

    [HttpPost("lessons/{lessonId}/complete")]
    public async Task<IActionResult> CompleteLesson(int courseId, int lessonId, CancellationToken cancellationToken)
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var result = await _progressService.CompleteLessonAsync(courseId, lessonId, userId, cancellationToken);
        if (result.IsSuccess)
        {
            return Ok(result.Value);
        }
        return BadRequest(new { Error = result.Error });
    }
}
