using ELearningManagementSystem.Application.Common;
using ELearningManagementSystem.Application.Features.ExamPayments.DTOs;
using ELearningManagementSystem.Application.Features.AuditLogs.DTOs;
using ELearningManagementSystem.Application.Features.AuditLogs.Services;
using ELearningManagementSystem.Application.Interfaces;
using ELearningManagementSystem.Domain.Entities;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

using ELearningManagementSystem.Application.Features.Notifications.DTOs;
using ELearningManagementSystem.Application.Features.Notifications.Services;

namespace ELearningManagementSystem.Application.Features.ExamPayments.Services;

public class ExamPaymentService : IExamPaymentService
{
    private readonly IAppDbContext _context;
    private readonly ILogger<ExamPaymentService> _logger;
    private readonly IValidator<SubmitExamPaymentRequest> _validator;
    private readonly INotificationService _notificationService;
    private readonly IAuditLogService _auditLogService;

    public ExamPaymentService(
        IAppDbContext context,
        ILogger<ExamPaymentService> logger,
        IValidator<SubmitExamPaymentRequest> validator,
        INotificationService notificationService,
        IAuditLogService auditLogService)
    {
        _context = context;
        _logger = logger;
        _validator = validator;
        _notificationService = notificationService;
        _auditLogService = auditLogService;
    }

    public async Task<Result<List<StudentCourseExamStatusResponse>>> GetStudentAvailableExamsAsync(int userId, CancellationToken cancellationToken = default)
    {
        var completedCourseIds = await _context.Enrollments
            .Where(e => e.UserId == userId && e.Completed)
            .Select(e => e.CourseId)
            .ToListAsync(cancellationToken);

        var exams = await _context.CourseExams
            .Include(e => e.Course)
            .Include(e => e.ExamQuestions.Where(q => !q.DeleteFlag))
            .Where(e => !e.DeleteFlag && e.Status && e.Course != null && !e.Course.DeleteFlag)
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        var examIds = exams.Select(e => e.ExamId).ToList();

        var userPayments = await _context.ExamPayments
            .Where(p => p.UserId == userId && examIds.Contains(p.ExamId) && !p.DeleteFlag)
            .OrderByDescending(p => p.CreatedAt)
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        var nowUtc = DateTime.UtcNow;
        var userAttempts = await _context.CourseExamAttempts
            .Where(a => a.UserId == userId && examIds.Contains(a.ExamId) && !a.DeleteFlag)
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        var result = new List<StudentCourseExamStatusResponse>();

        foreach (var exam in exams)
        {
            var isCourseCompleted = completedCourseIds.Contains(exam.CourseId);
            var latestPayment = userPayments.FirstOrDefault(p => p.ExamId == exam.ExamId);
            var attempts = userAttempts.Where(a => a.ExamId == exam.ExamId).ToList();
            var activeAttempt = attempts.FirstOrDefault(a => a.Status == "InProgress" && a.ExpiresAt > nowUtc);
            var latestSubmittedAttempt = attempts
                .Where(a => a.Status == "Submitted")
                .OrderByDescending(a => a.CreatedAt)
                .FirstOrDefault();

            string paymentStatus = isCourseCompleted ? "NotPaid" : "PrerequisiteRequired";
            int? activePaymentId = null;
            int? activeAttemptId = null;
            string? rejectionReason = null;
            bool canTakeExam = false;

            if (latestPayment != null)
            {
                activePaymentId = latestPayment.ExamPaymentId;

                if (activeAttempt != null)
                {
                    paymentStatus = "InProgress";
                    activeAttemptId = activeAttempt.AttemptId;
                    canTakeExam = true;
                }
                else if (!latestPayment.IsUsed)
                {
                    if (latestPayment.Status == "Approved")
                    {
                        paymentStatus = "Approved";
                        canTakeExam = true;
                    }
                    else if (latestPayment.Status == "Pending")
                    {
                        paymentStatus = "Pending";
                    }
                    else if (latestPayment.Status == "Rejected")
                    {
                        paymentStatus = "Rejected";
                        rejectionReason = latestPayment.RejectionReason;
                    }
                }
                else if (latestSubmittedAttempt != null)
                {
                    paymentStatus = "Submitted";
                    activeAttemptId = latestSubmittedAttempt.AttemptId;
                }
                else if (attempts.Any(a => a.Status == "InProgress" && a.ExpiresAt <= nowUtc))
                {
                    var expiredAttempt = attempts.First(a => a.Status == "InProgress" && a.ExpiresAt <= nowUtc);
                    paymentStatus = "Expired";
                    activeAttemptId = expiredAttempt.AttemptId;
                    canTakeExam = true;
                }
            }

            result.Add(new StudentCourseExamStatusResponse
            {
                ExamId = exam.ExamId,
                CourseId = exam.CourseId,
                CourseTitle = exam.Course.Title,
                ExamTitle = exam.Title,
                Description = exam.Description,
                ExamFee = exam.ExamFee,
                QuestionCount = exam.QuestionCount,
                DurationMinutes = exam.DurationMinutes,
                PassingScore = exam.PassingScore,
                MaxAttempts = exam.MaxAttempts,
                PoolQuestionCount = exam.ExamQuestions.Count,
                PaymentStatus = paymentStatus,
                ActivePaymentId = activePaymentId,
                ActiveAttemptId = activeAttemptId,
                RejectionReason = rejectionReason,
                CanTakeExam = canTakeExam,
                LastAttemptPassed = latestSubmittedAttempt?.Passed
            });
        }

        return Result.Success(result);
    }

    public async Task<Result<StudentCourseExamStatusResponse>> GetStudentExamStatusAsync(int examId, int userId, CancellationToken cancellationToken = default)
    {
        var availableExams = await GetStudentAvailableExamsAsync(userId, cancellationToken);
        if (!availableExams.IsSuccess)
            return Result.Failure<StudentCourseExamStatusResponse>(availableExams.Error!);

        var examStatus = availableExams.Value!.FirstOrDefault(e => e.ExamId == examId);
        if (examStatus == null)
        {
            var examExists = await _context.CourseExams.AnyAsync(e => e.ExamId == examId && !e.DeleteFlag, cancellationToken);
            if (!examExists)
                return Result.Failure<StudentCourseExamStatusResponse>("ExamNotFound: The specified exam does not exist.");

            return Result.Failure<StudentCourseExamStatusResponse>("CourseNotCompleted: You must complete all lessons and the course quiz before accessing this exam.");
        }

        return Result.Success(examStatus);
    }

    public async Task<Result<ExamPaymentResponse>> SubmitPaymentAsync(int examId, int userId, SubmitExamPaymentRequest request, string screenshotUrl, CancellationToken cancellationToken = default)
    {
        var validation = await _validator.ValidateAsync(request, cancellationToken);
        if (!validation.IsValid)
            return Result.Failure<ExamPaymentResponse>($"ValidationError: {validation.Errors.First().ErrorMessage}");

        var activeMethod = await _context.PaymentMethods
            .FirstOrDefaultAsync(m => !m.DeleteFlag && m.IsActive
                && m.Name.ToLower() == request.PaymentMethod.Trim().ToLower(), cancellationToken);

        if (activeMethod == null)
            return Result.Failure<ExamPaymentResponse>("InvalidPaymentMethod: The selected payment method is not available. Please choose an active gateway and try again.");

        var statusResult = await GetStudentExamStatusAsync(examId, userId, cancellationToken);
        if (!statusResult.IsSuccess)
            return Result.Failure<ExamPaymentResponse>(statusResult.Error!);

        var statusInfo = statusResult.Value!;
        if (statusInfo.PaymentStatus == "Pending")
            return Result.Failure<ExamPaymentResponse>("PaymentPending: You already have a payment under review. Please wait for admin approval.");

        if (statusInfo.PaymentStatus == "Approved" && statusInfo.CanTakeExam)
            return Result.Failure<ExamPaymentResponse>("AlreadyApproved: Your payment has already been approved. You can take the exam now.");

        var payment = new ExamPayment
        {
            ExamId = examId,
            UserId = userId,
            Amount = statusInfo.ExamFee,
            PaymentMethod = request.PaymentMethod.Trim(),
            TransactionId = request.TransactionId.Trim(),
            ScreenshotUrl = screenshotUrl,
            Status = "Pending",
            IsUsed = false,
            CreatedAt = DateTime.UtcNow,
            DeleteFlag = false
        };

        _context.ExamPayments.Add(payment);
        await _context.SaveChangesAsync(cancellationToken);

        // Notify Admin of new payment submission (TargetUrl: /admin/payments)
        var studentUser = await _context.Users.FindAsync(new object[] { userId }, cancellationToken);
        var studentName = studentUser?.FullName ?? "Student";

        await _notificationService.CreateNotificationAsync(new CreateNotificationRequest
        {
            UserId = null, // Admin broadcast notification
            Title = "New Exam Fee Payment Submitted",
            Message = $"{studentName} submitted {payment.Amount:N0} MMK payment for '{statusInfo.ExamTitle}' via {request.PaymentMethod}.",
            Type = "PaymentSubmitted",
            TargetUrl = "/admin/payments"
        }, cancellationToken);

        await _auditLogService.CreateAuditLogAsync(new CreateAuditLogRequest
        {
            UserId = userId,
            Action = "Submit",
            TableName = "ExamPayments",
            RecordId = payment.ExamPaymentId,
            Changes = new List<AuditLogChangeDto>
            {
                new() { Field = "ExamId", NewValue = examId.ToString() },
                new() { Field = "Amount", NewValue = payment.Amount.ToString("N0") },
                new() { Field = "PaymentMethod", NewValue = request.PaymentMethod.Trim() },
                new() { Field = "TransactionId", NewValue = request.TransactionId.Trim() }
            }
        }, cancellationToken);

        _logger.LogInformation("Student {UserId} submitted ExamPayment {PaymentId} for Exam {ExamId} via {Method} (Txn: {TxnId})",
            userId, payment.ExamPaymentId, examId, request.PaymentMethod, request.TransactionId);

        return await GetPaymentResponseByIdAsync(payment.ExamPaymentId, cancellationToken);
    }

    public async Task<Result<PagedResult<ExamPaymentResponse>>> GetPagedPaymentsAsync(PaymentListQuery query, CancellationToken cancellationToken = default)
    {
        var dbQuery = _context.ExamPayments
            .Include(p => p.Exam)
            .ThenInclude(e => e.Course)
            .Include(p => p.User)
            .Include(p => p.ReviewedByNavigation)
            .Where(p => !p.DeleteFlag)
            .AsNoTracking();

        if (!string.IsNullOrWhiteSpace(query.Status) && !query.Status.Equals("All", StringComparison.OrdinalIgnoreCase))
        {
            dbQuery = dbQuery.Where(p => p.Status.ToLower() == query.Status.ToLower());
        }

        if (!string.IsNullOrWhiteSpace(query.SearchTerm))
        {
            var term = query.SearchTerm.Trim().ToLower();
            dbQuery = dbQuery.Where(p =>
                p.User.FullName.ToLower().Contains(term) ||
                p.User.Email.ToLower().Contains(term) ||
                p.Exam.Title.ToLower().Contains(term) ||
                p.TransactionId.ToLower().Contains(term));
        }

        var totalCount = await dbQuery.CountAsync(cancellationToken);

        var items = await dbQuery
            .OrderByDescending(p => p.CreatedAt)
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .Select(p => new ExamPaymentResponse
            {
                ExamPaymentId = p.ExamPaymentId,
                ExamId = p.ExamId,
                ExamTitle = p.Exam.Title,
                CourseId = p.Exam.CourseId,
                CourseTitle = p.Exam.Course.Title,
                UserId = p.UserId,
                StudentName = p.User.FullName,
                StudentEmail = p.User.Email,
                Amount = p.Amount,
                PaymentMethod = p.PaymentMethod,
                TransactionId = p.TransactionId,
                ScreenshotUrl = p.ScreenshotUrl,
                Status = p.Status,
                IsUsed = p.IsUsed,
                RejectionReason = p.RejectionReason,
                ReviewedByName = p.ReviewedByNavigation != null ? p.ReviewedByNavigation.FullName : null,
                ReviewedAt = p.ReviewedAt,
                CreatedAt = p.CreatedAt
            })
            .ToListAsync(cancellationToken);

        return Result.Success(new PagedResult<ExamPaymentResponse>(items, totalCount, query.Page, query.PageSize));
    }

    public async Task<Result<ExamPaymentResponse>> ReviewPaymentAsync(int paymentId, ReviewExamPaymentRequest request, int adminId, CancellationToken cancellationToken = default)
    {
        var payment = await _context.ExamPayments
            .FirstOrDefaultAsync(p => p.ExamPaymentId == paymentId && !p.DeleteFlag, cancellationToken);

        if (payment == null)
            return Result.Failure<ExamPaymentResponse>("PaymentNotFound: The specified payment request was not found.");

        if (request.Approve)
        {
            payment.Status = "Approved";
            payment.RejectionReason = null;
        }
        else
        {
            payment.Status = "Rejected";
            payment.RejectionReason = string.IsNullOrWhiteSpace(request.RejectionReason)
                ? "Payment request was rejected by admin."
                : request.RejectionReason.Trim();
        }

        payment.ReviewedBy = adminId;
        payment.ReviewedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        // Notify Student of Payment Decision (TargetUrl: /course-exams)
        var exam = await _context.CourseExams.FindAsync(new object[] { payment.ExamId }, cancellationToken);
        var examTitle = exam?.Title ?? "Course Exam";

        await _auditLogService.CreateAuditLogAsync(new CreateAuditLogRequest
        {
            UserId = adminId,
            Action = request.Approve ? "Approve" : "Reject",
            TableName = "ExamPayments",
            RecordId = payment.ExamPaymentId,
            Changes = new List<AuditLogChangeDto>
            {
                new() { Field = "Status", OldValue = "Pending", NewValue = payment.Status },
                new() { Field = "RejectionReason", NewValue = payment.RejectionReason }
            }
        }, cancellationToken);

        if (request.Approve)
        {
            await _notificationService.CreateNotificationAsync(new CreateNotificationRequest
            {
                UserId = payment.UserId,
                Title = "Exam Payment Approved! 🎉",
                Message = $"Your payment for '{examTitle}' has been verified and approved. You can start your exam attempt now!",
                Type = "PaymentApproved",
                TargetUrl = "/course-exams"
            }, cancellationToken);
        }
        else
        {
            await _notificationService.CreateNotificationAsync(new CreateNotificationRequest
            {
                UserId = payment.UserId,
                Title = "Exam Payment Action Required ⚠️",
                Message = $"Your payment for '{examTitle}' was rejected: {payment.RejectionReason}. Please re-submit payment.",
                Type = "PaymentRejected",
                TargetUrl = "/course-exams"
            }, cancellationToken);
        }

        _logger.LogInformation("Admin {AdminId} reviewed ExamPayment {PaymentId}: Approved={Approve}, Status={Status}",
            adminId, paymentId, request.Approve, payment.Status);

        return await GetPaymentResponseByIdAsync(paymentId, cancellationToken);
    }

    private async Task<Result<ExamPaymentResponse>> GetPaymentResponseByIdAsync(int paymentId, CancellationToken cancellationToken)
    {
        var p = await _context.ExamPayments
            .Include(p => p.Exam)
            .ThenInclude(e => e.Course)
            .Include(p => p.User)
            .Include(p => p.ReviewedByNavigation)
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.ExamPaymentId == paymentId, cancellationToken);

        if (p == null)
            return Result.Failure<ExamPaymentResponse>("PaymentNotFound: Payment not found.");

        var response = new ExamPaymentResponse
        {
            ExamPaymentId = p.ExamPaymentId,
            ExamId = p.ExamId,
            ExamTitle = p.Exam.Title,
            CourseId = p.Exam.CourseId,
            CourseTitle = p.Exam.Course.Title,
            UserId = p.UserId,
            StudentName = p.User.FullName,
            StudentEmail = p.User.Email,
            Amount = p.Amount,
            PaymentMethod = p.PaymentMethod,
            TransactionId = p.TransactionId,
            ScreenshotUrl = p.ScreenshotUrl,
            Status = p.Status,
            IsUsed = p.IsUsed,
            RejectionReason = p.RejectionReason,
            ReviewedByName = p.ReviewedByNavigation != null ? p.ReviewedByNavigation.FullName : null,
            ReviewedAt = p.ReviewedAt,
            CreatedAt = p.CreatedAt
        };

        return Result.Success(response);
    }
}
