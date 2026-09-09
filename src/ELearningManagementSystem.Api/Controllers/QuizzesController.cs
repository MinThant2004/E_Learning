using System.Threading;
using System.Threading.Tasks;
using ELearningManagementSystem.Application.Features.Quizzes.DTOs;
using ELearningManagementSystem.Application.Features.Quizzes.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ELearningManagementSystem.Api.Controllers;

[ApiController]
[Route("api")]
public class QuizzesController : ControllerBase
{
    private readonly IQuizService _quizService;

    public QuizzesController(IQuizService quizService)
    {
        _quizService = quizService;
    }

    // ─── Quiz Endpoints ──────────────────────────────────────────

    /// <summary>GET /api/courses/{courseId}/quizzes — List quizzes for a course</summary>
    [HttpGet("courses/{courseId}/quizzes")]
    [Authorize(Policy = "Permission:Quiz.Read")]
    public async Task<IActionResult> GetQuizzes(int courseId, [FromQuery] bool? isArchived = null, CancellationToken cancellationToken = default)
    {
        var result = await _quizService.GetQuizzesByCourseAsync(courseId, isArchived, cancellationToken);
        if (result.IsFailure)
        {
            if (result.Error == "CourseNotFound") return NotFound(new { Error = result.Error });
            return BadRequest(new { Error = result.Error });
        }
        return Ok(result.Value);
    }

    /// <summary>POST /api/courses/{courseId}/quizzes — Create a final quiz for a course</summary>
    [HttpPost("courses/{courseId}/quizzes")]
    [Authorize(Policy = "Permission:Quiz.Create")]
    public async Task<IActionResult> CreateQuiz(int courseId, [FromBody] CreateQuizRequest request, CancellationToken cancellationToken)
    {
        if (request.CourseId != courseId)
            return BadRequest(new { Error = "CourseIdMismatch" });

        var result = await _quizService.CreateQuizAsync(request, cancellationToken);
        if (result.IsFailure)
        {
            if (result.Error == "CourseNotFound") return NotFound(new { Error = result.Error });
            if (result.Error == "QuizAlreadyExistsForCourse") return Conflict(new { Error = result.Error });
            return BadRequest(new { Error = result.Error });
        }
        return Created($"/api/quizzes/{result.Value!.QuizId}", result.Value);
    }

    /// <summary>GET /api/quizzes/{quizId} — Get quiz detail with questions and options</summary>
    [HttpGet("quizzes/{quizId}")]
    [Authorize(Policy = "Permission:Quiz.Read")]
    public async Task<IActionResult> GetQuiz(int quizId, CancellationToken cancellationToken)
    {
        var result = await _quizService.GetQuizByIdAsync(quizId, cancellationToken);
        if (result.IsFailure) return NotFound(new { Error = result.Error });
        return Ok(result.Value);
    }

    /// <summary>PUT /api/quizzes/{quizId} — Update quiz title and passing score</summary>
    [HttpPut("quizzes/{quizId}")]
    [Authorize(Policy = "Permission:Quiz.Update")]
    public async Task<IActionResult> UpdateQuiz(int quizId, [FromBody] UpdateQuizRequest request, CancellationToken cancellationToken)
    {
        var result = await _quizService.UpdateQuizAsync(quizId, request, cancellationToken);
        if (result.IsFailure)
        {
            if (result.Error == "QuizNotFound") return NotFound(new { Error = result.Error });
            if (result.Error == "ConcurrencyConflict") return Conflict(new { Error = "This quiz was modified by another user. Reload the latest version and try again." });
            return BadRequest(new { Error = result.Error });
        }
        return Ok(result.Value);
    }

    /// <summary>POST /api/quizzes/{quizId}/archive — Archive a quiz</summary>
    [HttpPost("quizzes/{quizId}/archive")]
    [Authorize(Policy = "Permission:Quiz.Delete")]
    public async Task<IActionResult> ArchiveQuiz(int quizId, CancellationToken cancellationToken)
    {
        var result = await _quizService.ArchiveQuizAsync(quizId, cancellationToken);
        if (result.IsFailure)
        {
            if (result.Error == "QuizNotFound") return NotFound(new { Error = result.Error });
            return BadRequest(new { Error = result.Error });
        }
        return Ok(new { Message = "Quiz archived successfully." });
    }

    /// <summary>POST /api/quizzes/{quizId}/restore — Restore a quiz</summary>
    [HttpPost("quizzes/{quizId}/restore")]
    [Authorize(Policy = "Permission:Quiz.Update")]
    public async Task<IActionResult> RestoreQuiz(int quizId, CancellationToken cancellationToken)
    {
        var result = await _quizService.RestoreQuizAsync(quizId, cancellationToken);
        if (result.IsFailure)
        {
            if (result.Error == "QuizNotFound") return NotFound(new { Error = result.Error });
            if (result.Error == "CannotRestoreArchivedCourse") return BadRequest(new { Error = result.Error });
            if (result.Error == "QuizAlreadyExistsForCourse") return Conflict(new { Error = result.Error });
            return BadRequest(new { Error = result.Error });
        }
        return Ok(new { Message = "Quiz restored successfully." });
    }

    // ─── Question Endpoints ──────────────────────────────────────

    /// <summary>POST /api/quizzes/{quizId}/questions — Add a question to a quiz</summary>
    [HttpPost("quizzes/{quizId}/questions")]
    [Authorize(Policy = "Permission:Quiz.Create")]
    public async Task<IActionResult> CreateQuestion(int quizId, [FromBody] CreateQuestionRequest request, CancellationToken cancellationToken)
    {
        if (request.QuizId != quizId)
            return BadRequest(new { Error = "QuizIdMismatch" });

        var result = await _quizService.CreateQuestionAsync(request, cancellationToken);
        if (result.IsFailure)
        {
            if (result.Error == "QuizNotFound") return NotFound(new { Error = result.Error });
            return BadRequest(new { Error = result.Error });
        }
        return Created($"/api/questions/{result.Value!.QuestionId}", result.Value);
    }

    /// <summary>PUT /api/questions/{questionId} — Update a question</summary>
    [HttpPut("questions/{questionId}")]
    [Authorize(Policy = "Permission:Quiz.Update")]
    public async Task<IActionResult> UpdateQuestion(int questionId, [FromBody] UpdateQuestionRequest request, CancellationToken cancellationToken)
    {
        var result = await _quizService.UpdateQuestionAsync(questionId, request, cancellationToken);
        if (result.IsFailure)
        {
            if (result.Error == "QuestionNotFound") return NotFound(new { Error = result.Error });
            if (result.Error == "ConcurrencyConflict") return Conflict(new { Error = "This question was modified by another user. Reload the latest version and try again." });
            return BadRequest(new { Error = result.Error });
        }
        return Ok(result.Value);
    }

    /// <summary>DELETE /api/questions/{questionId} — Remove a question and its options</summary>
    [HttpDelete("questions/{questionId}")]
    [Authorize(Policy = "Permission:Quiz.Delete")]
    public async Task<IActionResult> DeleteQuestion(int questionId, CancellationToken cancellationToken)
    {
        var result = await _quizService.DeleteQuestionAsync(questionId, cancellationToken);
        if (result.IsFailure)
        {
            if (result.Error == "QuestionNotFound") return NotFound(new { Error = result.Error });
            return BadRequest(new { Error = result.Error });
        }
        return Ok(new { Message = "Question deleted successfully." });
    }

    // ─── Option Endpoints ────────────────────────────────────────

    /// <summary>GET /api/questions/{questionId}/options — Get options for a question</summary>
    [HttpGet("questions/{questionId}/options")]
    [Authorize(Policy = "Permission:Quiz.Read")]
    public async Task<IActionResult> GetOptions(int questionId, CancellationToken cancellationToken)
    {
        var result = await _quizService.GetOptionsByQuestionAsync(questionId, cancellationToken);
        if (result.IsFailure)
        {
            if (result.Error == "QuestionNotFound") return NotFound(new { Error = result.Error });
            return BadRequest(new { Error = result.Error });
        }
        return Ok(result.Value);
    }

    /// <summary>POST /api/questions/{questionId}/options — Add an option to a question</summary>
    [HttpPost("questions/{questionId}/options")]
    [Authorize(Policy = "Permission:Quiz.Update")]
    public async Task<IActionResult> CreateOption(int questionId, [FromBody] CreateQuestionOptionRequest request, CancellationToken cancellationToken)
    {
        var result = await _quizService.CreateOptionAsync(questionId, request, cancellationToken);
        if (result.IsFailure)
        {
            if (result.Error == "QuestionNotFound") return NotFound(new { Error = result.Error });
            return BadRequest(new { Error = result.Error });
        }
        return Created($"/api/options/{result.Value!.OptionId}", result.Value);
    }

    /// <summary>PUT /api/options/{optionId} — Update an option</summary>
    [HttpPut("options/{optionId}")]
    [Authorize(Policy = "Permission:Quiz.Update")]
    public async Task<IActionResult> UpdateOption(int optionId, [FromBody] UpdateQuestionOptionRequest request, CancellationToken cancellationToken)
    {
        var result = await _quizService.UpdateOptionAsync(optionId, request, cancellationToken);
        if (result.IsFailure)
        {
            if (result.Error == "OptionNotFound") return NotFound(new { Error = result.Error });
            if (result.Error == "ConcurrencyConflict") return Conflict(new { Error = "This answer option was modified by another user. Reload the latest version and try again." });
            if (result.Error == "OptionNotInQuestion") return BadRequest(new { Error = result.Error });
            if (result.Error == "QuizNotFound") return NotFound(new { Error = result.Error });
            if (result.Error == "InvalidCorrectAnswer") return BadRequest(new { Error = result.Error });
            return BadRequest(new { Error = result.Error });
        }
        return Ok(result.Value);
    }

    /// <summary>DELETE /api/options/{optionId} — Remove an option</summary>
    [HttpDelete("options/{optionId}")]
    [Authorize(Policy = "Permission:Quiz.Update")]
    public async Task<IActionResult> DeleteOption(int optionId, CancellationToken cancellationToken)
    {
        var result = await _quizService.DeleteOptionAsync(optionId, cancellationToken);
        if (result.IsFailure)
        {
            if (result.Error == "OptionNotFound") return NotFound(new { Error = result.Error });
            if (result.Error == "InvalidQuestionConfiguration") return BadRequest(new { Error = "Cannot delete: at least 2 options are required per question." });
            if (result.Error == "InvalidCorrectAnswer") return BadRequest(new { Error = "Cannot delete: at least one correct answer must remain." });
            return BadRequest(new { Error = result.Error });
        }
        return Ok(new { Message = "Option deleted successfully." });
    }

    /// <summary>POST /api/options/{optionId}/archive — Archive an option</summary>
    [HttpPost("options/{optionId}/archive")]
    [Authorize(Policy = "Permission:Quiz.Update")]
    public async Task<IActionResult> ArchiveOption(int optionId, CancellationToken cancellationToken)
    {
        var result = await _quizService.ArchiveOptionAsync(optionId, cancellationToken);
        if (result.IsFailure)
        {
            if (result.Error == "OptionNotFound") return NotFound(new { Error = result.Error });
            return BadRequest(new { Error = result.Error });
        }
        return Ok(new { Message = "Option archived successfully." });
    }

    /// <summary>POST /api/options/{optionId}/restore — Restore an option</summary>
    [HttpPost("options/{optionId}/restore")]
    [Authorize(Policy = "Permission:Quiz.Update")]
    public async Task<IActionResult> RestoreOption(int optionId, CancellationToken cancellationToken)
    {
        var result = await _quizService.RestoreOptionAsync(optionId, cancellationToken);
        if (result.IsFailure)
        {
            if (result.Error == "OptionNotFound") return NotFound(new { Error = result.Error });
            return BadRequest(new { Error = result.Error });
        }
        return Ok(new { Message = "Option restored successfully." });
    }
}
