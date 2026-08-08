using ELearningManagementSystem.Application.Features.QuizAttempts.DTOs;
using ELearningManagementSystem.Application.Features.QuizAttempts.Services;
using ELearningManagementSystem.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ELearningManagementSystem.Api.Controllers;

[ApiController]
[Authorize]
public class QuizAttemptsController : ControllerBase
{
    private readonly IQuizAttemptService _quizAttemptService;
    private readonly ICurrentUserService _currentUserService;

    public QuizAttemptsController(IQuizAttemptService quizAttemptService, ICurrentUserService currentUserService)
    {
        _quizAttemptService = quizAttemptService;
        _currentUserService = currentUserService;
    }

    [HttpGet("api/courses/{courseId}/final-quiz")]
    public async Task<IActionResult> GetAvailableQuiz(int courseId, CancellationToken cancellationToken)
    {
        if (!_currentUserService.UserId.HasValue) return Unauthorized();
        var userId = _currentUserService.UserId.Value;
        
        var result = await _quizAttemptService.GetAvailableQuizAsync(courseId, userId, cancellationToken);
        if (!result.IsSuccess)
        {
            if (result.Error?.StartsWith("CourseNotFound") == true || result.Error?.StartsWith("QuizNotFound") == true)
                return NotFound(result);
            if (result.Error?.StartsWith("EnrollmentNotFound") == true)
                return Forbid();
            
            return BadRequest(result);
        }
        return Ok(result.Value);
    }

    [HttpPost("api/courses/{courseId}/final-quiz/attempts")]
    public async Task<IActionResult> SubmitQuizAttempt(int courseId, [FromBody] SubmitQuizAttemptRequest request, CancellationToken cancellationToken)
    {
        if (!_currentUserService.UserId.HasValue) return Unauthorized();
        var userId = _currentUserService.UserId.Value;

        var result = await _quizAttemptService.SubmitQuizAttemptAsync(courseId, userId, request, cancellationToken);
        if (!result.IsSuccess)
        {
            if (result.Error?.StartsWith("CourseNotFound") == true || result.Error?.StartsWith("QuizNotFound") == true)
                return NotFound(result);
            if (result.Error?.StartsWith("EnrollmentNotFound") == true)
                return Forbid();
                
            return BadRequest(result);
        }
        return Ok(result.Value);
    }

    [HttpGet("api/courses/{courseId}/final-quiz/attempts")]
    public async Task<IActionResult> GetStudentAttemptHistory(int courseId, CancellationToken cancellationToken)
    {
        if (!_currentUserService.UserId.HasValue) return Unauthorized();
        var userId = _currentUserService.UserId.Value;

        var result = await _quizAttemptService.GetStudentAttemptHistoryAsync(courseId, userId, cancellationToken);
        return Ok(result.Value);
    }

    [HttpGet("api/quiz-attempts/{attemptId}")]
    public async Task<IActionResult> GetAttemptDetails(int attemptId, CancellationToken cancellationToken)
    {
        if (!_currentUserService.UserId.HasValue) return Unauthorized();
        var userId = _currentUserService.UserId.Value;

        var result = await _quizAttemptService.GetAttemptDetailsAsync(attemptId, userId, cancellationToken);
        if (!result.IsSuccess)
        {
            if (result.Error?.StartsWith("AttemptNotFound") == true)
                return NotFound(result);
                
            return BadRequest(result);
        }
        return Ok(result.Value);
    }
}
