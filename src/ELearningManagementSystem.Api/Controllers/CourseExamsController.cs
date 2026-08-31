using ELearningManagementSystem.Application.Features.CourseExams.DTOs;
using ELearningManagementSystem.Application.Features.CourseExams.Services;
using ELearningManagementSystem.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ELearningManagementSystem.Api.Controllers;

[ApiController]
[Route("api/course-exams")]
public class CourseExamsController : ControllerBase
{
    private readonly ICourseExamService _examService;
    private readonly ICurrentUserService _currentUser;

    public CourseExamsController(ICourseExamService examService, ICurrentUserService currentUser)
    {
        _examService = examService;
        _currentUser = currentUser;
    }

    /// <summary>GET /api/course-exams — Get paged course exams</summary>
    [HttpGet]
    [Authorize(Policy = "Permission:Course.Read")]
    public async Task<IActionResult> GetExams([FromQuery] ExamListQuery query, CancellationToken cancellationToken)
    {
        var result = await _examService.GetPagedExamsAsync(query, cancellationToken);
        return result.ToActionResult();
    }

    /// <summary>GET /api/course-exams/{id:int} — Get exam details</summary>
    [HttpGet("{id:int}")]
    [Authorize(Policy = "Permission:Course.Read")]
    public async Task<IActionResult> GetExamById(int id, CancellationToken cancellationToken)
    {
        var result = await _examService.GetExamByIdAsync(id, cancellationToken);
        return result.ToActionResult();
    }

    /// <summary>POST /api/course-exams — Create new course exam</summary>
    [HttpPost]
    [Authorize(Policy = "Permission:Course.Create")]
    public async Task<IActionResult> CreateExam([FromBody] CreateCourseExamRequest request, CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId;
        if (userId is null)
            return Unauthorized(new { Error = "AuthenticationRequired" });

        var result = await _examService.CreateExamAsync(request, userId.Value, cancellationToken);
        return result.ToActionResult();
    }

    /// <summary>PUT /api/course-exams/{id:int} — Update course exam</summary>
    [HttpPut("{id:int}")]
    [Authorize(Policy = "Permission:Course.Update")]
    public async Task<IActionResult> UpdateExam(int id, [FromBody] UpdateCourseExamRequest request, CancellationToken cancellationToken)
    {
        var result = await _examService.UpdateExamAsync(id, request, _currentUser.UserId ?? 0, cancellationToken);
        return result.ToActionResult();
    }

    /// <summary>POST /api/course-exams/{id:int}/archive — Archive exam</summary>
    [HttpPost("{id:int}/archive")]
    [Authorize(Policy = "Permission:Course.Delete")]
    public async Task<IActionResult> ArchiveExam(int id, CancellationToken cancellationToken)
    {
        var result = await _examService.ArchiveExamAsync(id, _currentUser.UserId ?? 0, cancellationToken);
        return result.ToActionResult();
    }

    /// <summary>POST /api/course-exams/{id:int}/restore — Restore exam</summary>
    [HttpPost("{id:int}/restore")]
    [Authorize(Policy = "Permission:Course.Update")]
    public async Task<IActionResult> RestoreExam(int id, CancellationToken cancellationToken)
    {
        var result = await _examService.RestoreExamAsync(id, _currentUser.UserId ?? 0, cancellationToken);
        return result.ToActionResult();
    }
}
