using System.Threading;
using System.Threading.Tasks;
using ELearningManagementSystem.Application.Features.AuditLogs.DTOs;
using ELearningManagementSystem.Application.Features.AuditLogs.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ELearningManagementSystem.Api.Controllers;

[ApiController]
[Route("api/audit-logs")]
public class AuditLogsController : ControllerBase
{
    private readonly IAuditLogService _auditLogService;

    public AuditLogsController(IAuditLogService auditLogService)
    {
        _auditLogService = auditLogService;
    }

    [HttpGet]
    [Authorize(Policy = "Permission:AuditLog.Read")]
    public async Task<IActionResult> GetAuditLogs([FromQuery] AuditLogListQuery query, CancellationToken cancellationToken)
    {
        var result = await _auditLogService.GetPagedListAsync(query, cancellationToken);
        if (result.IsFailure)
            return BadRequest(new { Error = result.Error });

        return Ok(result.Value);
    }

    [HttpGet("{id}")]
    [Authorize(Policy = "Permission:AuditLog.Read")]
    public async Task<IActionResult> GetAuditLog(int id, CancellationToken cancellationToken)
    {
        var result = await _auditLogService.GetByIdAsync(id, cancellationToken);
        if (result.IsFailure)
        {
            if (result.Error == "AuditLogNotFound")
                return NotFound(new { Error = result.Error });

            return BadRequest(new { Error = result.Error });
        }

        return Ok(result.Value);
    }
}
