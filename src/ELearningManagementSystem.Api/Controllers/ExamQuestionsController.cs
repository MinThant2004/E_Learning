using ELearningManagementSystem.Application.Features.CourseExams.DTOs;
using ELearningManagementSystem.Application.Features.CourseExams.Services;
using ELearningManagementSystem.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ELearningManagementSystem.Api.Controllers;

[ApiController]
[Route("api/course-exams/{examId}/questions")]
public class ExamQuestionsController : ControllerBase
{
    private readonly ICourseExamService _examService;
    private readonly ICurrentUserService _currentUser;

    public ExamQuestionsController(ICourseExamService examService, ICurrentUserService currentUser)
    {
        _examService = examService;
        _currentUser = currentUser;
    }

    /// <summary>GET /api/course-exams/{examId}/questions — Get pool questions for an exam</summary>
    [HttpGet]
    [Authorize(Policy = "Permission:Course.Read")]
    public async Task<IActionResult> GetQuestionPool(int examId, CancellationToken cancellationToken)
    {
        var result = await _examService.GetQuestionPoolAsync(examId, cancellationToken);
        return result.ToActionResult();
    }

    /// <summary>POST /api/course-exams/{examId}/questions — Add question & options to pool</summary>
    [HttpPost]
    [Authorize(Policy = "Permission:Course.Create")]
    public async Task<IActionResult> AddQuestionToPool(int examId, [FromBody] CreateExamQuestionRequest request, CancellationToken cancellationToken)
    {
        var result = await _examService.AddQuestionToPoolAsync(examId, request, _currentUser.UserId ?? 0, cancellationToken);
        return result.ToActionResult();
    }

    /// <summary>PUT /api/exam-questions/{questionId} — Update question & options in pool</summary>
    [HttpPut("/api/exam-questions/{questionId}")]
    [Authorize(Policy = "Permission:Course.Update")]
    public async Task<IActionResult> UpdateQuestionInPool(int questionId, [FromBody] UpdateExamQuestionRequest request, CancellationToken cancellationToken)
    {
        var result = await _examService.UpdateQuestionInPoolAsync(questionId, request, _currentUser.UserId ?? 0, cancellationToken);
        return result.ToActionResult();
    }

    /// <summary>DELETE /api/exam-questions/{questionId} — Archive question from pool</summary>
    [HttpDelete("/api/exam-questions/{questionId}")]
    [Authorize(Policy = "Permission:Course.Update")]
    public async Task<IActionResult> DeleteQuestionFromPool(int questionId, CancellationToken cancellationToken)
    {
        var result = await _examService.DeleteQuestionFromPoolAsync(questionId, _currentUser.UserId ?? 0, cancellationToken);
        return result.ToActionResult();
    }
}
