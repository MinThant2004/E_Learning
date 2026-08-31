using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using ELearningManagementSystem.Application.Common;
using ELearningManagementSystem.Application.Interfaces;
using ELearningManagementSystem.Application.Features.AuditLogs.DTOs;
using ELearningManagementSystem.Domain.Entities;

namespace ELearningManagementSystem.Application.Features.AuditLogs.Services;

public class AuditLogService : IAuditLogService
{
    private const int MaxChangeValueLength = 300;
    private const int MaxTrackedChanges = 25;

    private static readonly JsonSerializerOptions ChangesJsonOptions = new(JsonSerializerDefaults.Web);

    private readonly IAppDbContext _context;

    public AuditLogService(IAppDbContext context)
    {
        _context = context;
    }

    public async Task<Result<bool>> CreateAuditLogAsync(CreateAuditLogRequest request, CancellationToken cancellationToken = default)
    {
        if (request == null)
            return Result.Failure<bool>("InvalidAuditRequest");

        var auditLog = new AuditLog
        {
            UserId = request.UserId,
            Action = request.Action ?? string.Empty,
            TableName = request.TableName ?? string.Empty,
            RecordId = request.RecordId,
            CreatedAt = DateTime.UtcNow,
            Changes = SerializeChanges(request.Changes)
        };

        _context.AuditLogs.Add(auditLog);
        await _context.SaveChangesAsync(cancellationToken);
        return Result.Success(true);
    }

    public async Task<Result<PagedResult<AuditLogResponse>>> GetPagedListAsync(AuditLogListQuery query, CancellationToken cancellationToken = default)
    {
        IQueryable<AuditLog> dbQuery = _context.AuditLogs
            .Include(a => a.User)
            .AsNoTracking();

        if (!string.IsNullOrWhiteSpace(query.SearchTerm))
        {
            var search = query.SearchTerm.Trim().ToLower();
            dbQuery = dbQuery.Where(a =>
                a.Action.ToLower().Contains(search) ||
                a.TableName.ToLower().Contains(search) ||
                a.User.Email.ToLower().Contains(search) ||
                a.User.FullName.ToLower().Contains(search)
            );
        }

        if (!string.IsNullOrWhiteSpace(query.ActionFilter))
        {
            dbQuery = dbQuery.Where(a => a.Action == query.ActionFilter.Trim());
        }

        if (!string.IsNullOrWhiteSpace(query.TableNameFilter))
        {
            dbQuery = dbQuery.Where(a => a.TableName == query.TableNameFilter.Trim());
        }

        if (query.UserIdFilter.HasValue)
        {
            dbQuery = dbQuery.Where(a => a.UserId == query.UserIdFilter.Value);
        }

        if (query.StartDate.HasValue)
        {
            dbQuery = dbQuery.Where(a => a.CreatedAt >= query.StartDate.Value);
        }

        if (query.EndDate.HasValue)
        {
            dbQuery = dbQuery.Where(a => a.CreatedAt <= query.EndDate.Value);
        }

        var totalCount = await dbQuery.CountAsync(cancellationToken);

        var items = await dbQuery
            .OrderByDescending(a => a.CreatedAt)
            .Skip((query.Page - 1) * query.PageSize)
            .Take(query.PageSize)
            .Select(a => new AuditLogResponse
            {
                AuditLogId = a.AuditLogId,
                UserId = a.UserId,
                UserEmail = a.User.Email,
                UserFullName = a.User.FullName,
                Action = a.Action,
                TableName = a.TableName,
                RecordId = a.RecordId,
                CreatedAt = a.CreatedAt
            })
            .ToListAsync(cancellationToken);

        var pagedResult = new PagedResult<AuditLogResponse>(items, totalCount, query.Page, query.PageSize);
        return Result.Success(pagedResult);
    }

    public async Task<Result<AuditLogResponse>> GetByIdAsync(int auditLogId, CancellationToken cancellationToken = default)
    {
        var log = await _context.AuditLogs
            .Include(a => a.User)
            .AsNoTracking()
            .FirstOrDefaultAsync(a => a.AuditLogId == auditLogId, cancellationToken);

        if (log == null)
            return Result.Failure<AuditLogResponse>("AuditLogNotFound");

        var response = new AuditLogResponse
        {
            AuditLogId = log.AuditLogId,
            UserId = log.UserId,
            UserEmail = log.User.Email,
            UserFullName = log.User.FullName,
            Action = log.Action,
            TableName = log.TableName,
            RecordId = log.RecordId,
            CreatedAt = log.CreatedAt,
            RecordName = await ResolveRecordNameAsync(log.TableName, log.RecordId, cancellationToken),
            Changes = DeserializeChanges(log.Changes)
        };

        return Result.Success(response);
    }

    /// <summary>
    /// Resolves the display name of the audited record from its table.
    /// Archived (soft-deleted) rows are still resolved since they remain in the database.
    /// </summary>
    private async Task<string?> ResolveRecordNameAsync(string tableName, int recordId, CancellationToken cancellationToken)
    {
        switch (tableName)
        {
            case "Categories":
                return (await _context.Categories.AsNoTracking()
                    .FirstOrDefaultAsync(c => c.CategoryId == recordId, cancellationToken))?.CategoryName;
            case "Courses":
                return (await _context.Courses.AsNoTracking()
                    .FirstOrDefaultAsync(c => c.CourseId == recordId, cancellationToken))?.Title;
            case "Lessons":
                return (await _context.Lessons.AsNoTracking()
                    .FirstOrDefaultAsync(l => l.LessonId == recordId, cancellationToken))?.Title;
            case "Quizzes":
                return (await _context.Quizzes.AsNoTracking()
                    .FirstOrDefaultAsync(q => q.QuizId == recordId, cancellationToken))?.Title;
            case "Questions":
                return (await _context.Questions.AsNoTracking()
                    .FirstOrDefaultAsync(q => q.QuestionId == recordId, cancellationToken))?.QuestionText;
            case "ExamQuestions":
                return (await _context.ExamQuestions.AsNoTracking()
                    .FirstOrDefaultAsync(q => q.ExamQuestionId == recordId, cancellationToken))?.QuestionText;
            case "Users":
                return (await _context.Users.AsNoTracking()
                    .FirstOrDefaultAsync(u => u.UserId == recordId, cancellationToken))?.FullName;
            case "CourseExams":
                return (await _context.CourseExams.AsNoTracking()
                    .FirstOrDefaultAsync(e => e.ExamId == recordId, cancellationToken))?.Title;
            case "ExamPayments":
                var payment = await _context.ExamPayments.AsNoTracking()
                    .Include(p => p.Exam)
                    .Include(p => p.User)
                    .FirstOrDefaultAsync(p => p.ExamPaymentId == recordId, cancellationToken);
                return payment is null ? null : $"{payment.Exam?.Title} - {payment.User?.FullName} ({payment.TransactionId})";
            case "PaymentMethods":
                return (await _context.PaymentMethods.AsNoTracking()
                    .FirstOrDefaultAsync(pm => pm.PaymentMethodId == recordId, cancellationToken))?.Name;
            default:
                return null;
        }
    }

    private static string? SerializeChanges(List<AuditLogChangeDto>? changes)
    {
        if (changes == null || changes.Count == 0)
            return null;

        var trimmed = changes.Take(MaxTrackedChanges).Select(c => new AuditLogChangeDto
        {
            Field = c.Field,
            OldValue = TruncateValue(c.OldValue),
            NewValue = TruncateValue(c.NewValue)
        }).ToList();

        return JsonSerializer.Serialize(trimmed, ChangesJsonOptions);
    }

    private static List<AuditLogChangeDto>? DeserializeChanges(string? json)
    {
        if (string.IsNullOrWhiteSpace(json))
            return null;

        try
        {
            return JsonSerializer.Deserialize<List<AuditLogChangeDto>>(json, ChangesJsonOptions);
        }
        catch (JsonException)
        {
            return null;
        }
    }

    private static string? TruncateValue(string? value)
    {
        if (string.IsNullOrEmpty(value) || value.Length <= MaxChangeValueLength)
            return value;

        return value[..MaxChangeValueLength] + "…";
    }
}
