using System.Threading;
using System.Threading.Tasks;
using ELearningManagementSystem.Application.Common;
using ELearningManagementSystem.Application.Features.AuditLogs.DTOs;

namespace ELearningManagementSystem.Application.Features.AuditLogs.Services;

public interface IAuditLogService
{
    Task<Result<bool>> CreateAuditLogAsync(CreateAuditLogRequest request, CancellationToken cancellationToken = default);
    Task<Result<PagedResult<AuditLogResponse>>> GetPagedListAsync(AuditLogListQuery query, CancellationToken cancellationToken = default);
    Task<Result<AuditLogResponse>> GetByIdAsync(int auditLogId, CancellationToken cancellationToken = default);
}
