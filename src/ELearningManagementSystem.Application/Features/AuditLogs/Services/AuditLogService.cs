using System;
using System.Linq;
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
            CreatedAt = DateTime.UtcNow
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
            CreatedAt = log.CreatedAt
        };

        return Result.Success(response);
    }
}
