using System.IO;
using ELearningManagementSystem.Application.Features.ExamPayments.DTOs;
using ELearningManagementSystem.Application.Features.ExamPayments.Services;
using ELearningManagementSystem.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace ELearningManagementSystem.Api.Controllers;

[ApiController]
[Route("api")]
public class ExamPaymentsController : ControllerBase
{
    private readonly IExamPaymentService _paymentService;
    private readonly ICurrentUserService _currentUser;
    private readonly IWebHostEnvironment _env;

    public ExamPaymentsController(
        IExamPaymentService paymentService,
        ICurrentUserService currentUser,
        IWebHostEnvironment env)
    {
        _paymentService = paymentService;
        _currentUser = currentUser;
        _env = env;
    }

    /// <summary>GET /api/course-exams/my-available — Get exams for completed courses + payment status</summary>
    [HttpGet("course-exams/my-available")]
    [Authorize]
    public async Task<IActionResult> GetMyAvailableExams(CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId;
        if (userId is null) return Unauthorized(new { Error = "AuthenticationRequired" });

        var result = await _paymentService.GetStudentAvailableExamsAsync(userId.Value, cancellationToken);
        return result.ToActionResult();
    }

    /// <summary>GET /api/course-exams/{examId}/my-payment — Get student's payment status for an exam</summary>
    [HttpGet("course-exams/{examId}/my-payment")]
    [Authorize]
    public async Task<IActionResult> GetMyExamPaymentStatus(int examId, CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId;
        if (userId is null) return Unauthorized(new { Error = "AuthenticationRequired" });

        var result = await _paymentService.GetStudentExamStatusAsync(examId, userId.Value, cancellationToken);
        return result.ToActionResult();
    }

    /// <summary>POST /api/course-exams/{examId}/payments — Submit payment with screenshot upload</summary>
    [HttpPost("course-exams/{examId}/payments")]
    [Authorize]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> SubmitPayment(
        int examId,
        [FromForm] string paymentMethod,
        [FromForm] string transactionId,
        IFormFile screenshot,
        CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId;
        if (userId is null) return Unauthorized(new { Error = "AuthenticationRequired" });

        if (screenshot == null || screenshot.Length == 0)
            return BadRequest(new { Error = "ScreenshotRequired: Payment screenshot image is required." });

        var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".webp" };
        var ext = Path.GetExtension(screenshot.FileName).ToLowerInvariant();
        if (!allowedExtensions.Contains(ext))
            return BadRequest(new { Error = "InvalidFileType: Only JPG, PNG, or WEBP image files are allowed." });

        if (screenshot.Length > 5 * 1024 * 1024)
            return BadRequest(new { Error = "FileTooLarge: Screenshot file size must not exceed 5MB." });

        // Save screenshot to wwwroot/uploads/exam-payments/
        var uploadsDir = Path.Combine(_env.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot"), "uploads", "exam-payments");
        if (!Directory.Exists(uploadsDir))
            Directory.CreateDirectory(uploadsDir);

        var fileName = $"{Guid.NewGuid()}{ext}";
        var filePath = Path.Combine(uploadsDir, fileName);

        using (var stream = new FileStream(filePath, FileMode.Create))
        {
            await screenshot.CopyToAsync(stream, cancellationToken);
        }

        var relativeUrl = $"/uploads/exam-payments/{fileName}";

        var request = new SubmitExamPaymentRequest
        {
            PaymentMethod = paymentMethod,
            TransactionId = transactionId
        };

        var result = await _paymentService.SubmitPaymentAsync(examId, userId.Value, request, relativeUrl, cancellationToken);
        return result.ToActionResult();
    }

    /// <summary>GET /api/admin/exam-payments — List all submitted payments for admin review</summary>
    [HttpGet("admin/exam-payments")]
    [Authorize(Policy = "Permission:Course.Update")]
    public async Task<IActionResult> GetAdminPayments([FromQuery] PaymentListQuery query, CancellationToken cancellationToken)
    {
        var result = await _paymentService.GetPagedPaymentsAsync(query, cancellationToken);
        return result.ToActionResult();
    }

    /// <summary>POST /api/admin/exam-payments/{paymentId}/review — Approve or Reject a payment</summary>
    [HttpPost("admin/exam-payments/{paymentId}/review")]
    [Authorize(Policy = "Permission:Course.Update")]
    public async Task<IActionResult> ReviewPayment(int paymentId, [FromBody] ReviewExamPaymentRequest request, CancellationToken cancellationToken)
    {
        var adminId = _currentUser.UserId;
        if (adminId is null) return Unauthorized(new { Error = "AuthenticationRequired" });

        var result = await _paymentService.ReviewPaymentAsync(paymentId, request, adminId.Value, cancellationToken);
        return result.ToActionResult();
    }
}
