using ELearningManagementSystem.Application.Features.ExamEngine.DTOs;
using ELearningManagementSystem.Application.Features.ExamEngine.Services;
using ELearningManagementSystem.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ELearningManagementSystem.Api.Controllers;

[ApiController]
[Route("api/course-exams")]
public class ExamEngineController : ControllerBase
{
    private readonly IExamEngineService _examEngineService;
    private readonly ICurrentUserService _currentUser;

    public ExamEngineController(IExamEngineService examEngineService, ICurrentUserService currentUser)
    {
        _examEngineService = examEngineService;
        _currentUser = currentUser;
    }

    /// <summary>POST /api/course-exams/{examId:int}/attempts/start — Start new exam attempt session (consumes 1 approved payment)</summary>
    [HttpPost("{examId:int}/attempts/start")]
    [Authorize]
    public async Task<IActionResult> StartExamAttempt(int examId, CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId;
        if (userId is null) return Unauthorized(new { Error = "AuthenticationRequired" });

        var result = await _examEngineService.StartExamAttemptAsync(examId, userId.Value, cancellationToken);
        return result.ToActionResult();
    }

    /// <summary>GET /api/course-exams/attempts/{attemptId:int} — Get active attempt session & remaining time</summary>
    [HttpGet("attempts/{attemptId:int}")]
    [Authorize]
    public async Task<IActionResult> GetActiveAttemptSession(int attemptId, CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId;
        if (userId is null) return Unauthorized(new { Error = "AuthenticationRequired" });

        var result = await _examEngineService.GetActiveAttemptSessionAsync(attemptId, userId.Value, cancellationToken);
        return result.ToActionResult();
    }

    /// <summary>POST /api/course-exams/attempts/{attemptId:int}/answers — Save single answer in real-time</summary>
    [HttpPost("attempts/{attemptId:int}/answers")]
    [Authorize]
    public async Task<IActionResult> SaveAnswer(int attemptId, [FromBody] SaveAnswerRequest request, CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId;
        if (userId is null) return Unauthorized(new { Error = "AuthenticationRequired" });

        var result = await _examEngineService.SaveAnswerAsync(attemptId, request, userId.Value, cancellationToken);
        return result.ToActionResult();
    }

    /// <summary>POST /api/course-exams/attempts/{attemptId:int}/submit — Submit exam & grade attempt</summary>
    [HttpPost("attempts/{attemptId:int}/submit")]
    [Authorize]
    public async Task<IActionResult> SubmitExamAttempt(int attemptId, [FromBody] SubmitExamAttemptRequest request, CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId;
        if (userId is null) return Unauthorized(new { Error = "AuthenticationRequired" });

        var result = await _examEngineService.SubmitExamAttemptAsync(attemptId, request, userId.Value, cancellationToken);
        return result.ToActionResult();
    }

    /// <summary>GET /api/course-exams/attempts/{attemptId:int}/result — Get exam result & question breakdown</summary>
    [HttpGet("attempts/{attemptId:int}/result")]
    [Authorize]
    public async Task<IActionResult> GetAttemptResult(int attemptId, CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId;
        if (userId is null) return Unauthorized(new { Error = "AuthenticationRequired" });

        var result = await _examEngineService.GetAttemptResultAsync(attemptId, userId.Value, cancellationToken);
        return result.ToActionResult();
    }

    /// <summary>GET /api/course-exams/certificates/verify/{attemptId:int} — Public certificate verification (AllowAnonymous)</summary>
    [HttpGet("certificates/verify/{attemptId:int}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetPublicCertificate(int attemptId, CancellationToken cancellationToken)
    {
        var result = await _examEngineService.GetPublicCertificateAsync(attemptId, cancellationToken);
        return result.ToActionResult();
    }

    /// <summary>GET /api/course-exams/certificates/download/{attemptId:int} — Download certificate file directly (AllowAnonymous)</summary>
    [HttpGet("certificates/download/{attemptId:int}")]
    [AllowAnonymous]
    public async Task<IActionResult> DownloadCertificateFile(int attemptId, CancellationToken cancellationToken)
    {
        var result = await _examEngineService.DownloadCertificateFileAsync(attemptId, cancellationToken);
        if (result.IsSuccess && result.Value is not null)
        {
            var file = result.Value;
            return File(file.Content, file.ContentType, file.FileName);
        }
        return result.ToActionResult();
    }

    /// <summary>GET /api/course-exams/my-certificates — Get current student's earned certificates</summary>
    [HttpGet("my-certificates")]
    [Authorize]
    public async Task<IActionResult> GetMyCertificates(CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId;
        if (userId is null) return Unauthorized(new { Error = "AuthenticationRequired" });

        var result = await _examEngineService.GetMyCertificatesAsync(userId.Value, cancellationToken);
        return result.ToActionResult();
    }
}
