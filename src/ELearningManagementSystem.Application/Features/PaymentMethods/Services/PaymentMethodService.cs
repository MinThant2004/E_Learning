using ELearningManagementSystem.Application.Common;
using ELearningManagementSystem.Application.Features.PaymentMethods.DTOs;
using ELearningManagementSystem.Application.Features.AuditLogs.DTOs;
using ELearningManagementSystem.Application.Features.AuditLogs.Services;
using ELearningManagementSystem.Application.Interfaces;
using ELearningManagementSystem.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ELearningManagementSystem.Application.Features.PaymentMethods.Services;

public class PaymentMethodService : IPaymentMethodService
{
    private readonly IAppDbContext _context;
    private readonly ILogger<PaymentMethodService> _logger;
    private readonly IAuditLogService _auditLogService;

    public PaymentMethodService(IAppDbContext context, ILogger<PaymentMethodService> logger, IAuditLogService auditLogService)
    {
        _context = context;
        _logger = logger;
        _auditLogService = auditLogService;
    }

    public async Task<Result<List<PaymentMethodResponse>>> GetActivePaymentMethodsAsync(CancellationToken cancellationToken = default)
    {
        var methods = await _context.PaymentMethods
            .Where(pm => pm.IsActive && !pm.DeleteFlag)
            .OrderBy(pm => pm.DisplayOrder)
            .Select(pm => new PaymentMethodResponse
            {
                PaymentMethodId = pm.PaymentMethodId,
                Code = pm.Code,
                Name = pm.Name,
                LogoUrl = pm.LogoUrl,
                AccountName = pm.AccountName,
                AccountNumber = pm.AccountNumber,
                QrCodeUrl = pm.QrCodeUrl,
                IsActive = pm.IsActive,
                DisplayOrder = pm.DisplayOrder
            })
            .ToListAsync(cancellationToken);

        return Result.Success(methods);
    }

    public async Task<Result<List<PaymentMethodResponse>>> GetAllPaymentMethodsAdminAsync(CancellationToken cancellationToken = default)
    {
        var methods = await _context.PaymentMethods
            .Where(pm => !pm.DeleteFlag)
            .OrderBy(pm => pm.DisplayOrder)
            .Select(pm => new PaymentMethodResponse
            {
                PaymentMethodId = pm.PaymentMethodId,
                Code = pm.Code,
                Name = pm.Name,
                LogoUrl = pm.LogoUrl,
                AccountName = pm.AccountName,
                AccountNumber = pm.AccountNumber,
                QrCodeUrl = pm.QrCodeUrl,
                IsActive = pm.IsActive,
                DisplayOrder = pm.DisplayOrder
            })
            .ToListAsync(cancellationToken);

        return Result.Success(methods);
    }

    public async Task<Result<PaymentMethodResponse>> CreatePaymentMethodAsync(CreatePaymentMethodRequest request, int userId, CancellationToken cancellationToken = default)
    {
        var entity = new PaymentMethod
        {
            Code = request.Code.ToUpper().Trim(),
            Name = request.Name.Trim(),
            LogoUrl = request.LogoUrl,
            AccountName = request.AccountName.Trim(),
            AccountNumber = request.AccountNumber.Trim(),
            QrCodeUrl = request.QrCodeUrl,
            IsActive = request.IsActive,
            DisplayOrder = request.DisplayOrder,
            CreatedAt = DateTime.UtcNow,
            DeleteFlag = false
        };

        _context.PaymentMethods.Add(entity);
        await _context.SaveChangesAsync(cancellationToken);

        await _auditLogService.CreateAuditLogAsync(new CreateAuditLogRequest
        {
            UserId = userId,
            Action = "Create",
            TableName = "PaymentMethods",
            RecordId = entity.PaymentMethodId,
            Changes = new List<AuditLogChangeDto>
            {
                new() { Field = "Name", NewValue = entity.Name },
                new() { Field = "Code", NewValue = entity.Code },
                new() { Field = "IsActive", NewValue = entity.IsActive.ToString() }
            }
        }, cancellationToken);

        _logger.LogInformation("Admin {UserId} created PaymentMethod {PaymentMethodId} ({Name})", userId, entity.PaymentMethodId, entity.Name);

        return Result.Success(new PaymentMethodResponse
        {
            PaymentMethodId = entity.PaymentMethodId,
            Code = entity.Code,
            Name = entity.Name,
            LogoUrl = entity.LogoUrl,
            AccountName = entity.AccountName,
            AccountNumber = entity.AccountNumber,
            QrCodeUrl = entity.QrCodeUrl,
            IsActive = entity.IsActive,
            DisplayOrder = entity.DisplayOrder
        });
    }

    public async Task<Result<PaymentMethodResponse>> UpdatePaymentMethodAsync(int id, UpdatePaymentMethodRequest request, int userId, CancellationToken cancellationToken = default)
    {
        var entity = await _context.PaymentMethods.FirstOrDefaultAsync(pm => pm.PaymentMethodId == id && !pm.DeleteFlag, cancellationToken);
        if (entity == null) return Result.Failure<PaymentMethodResponse>("Payment method not found.");

        var oldName = entity.Name;
        var oldActive = entity.IsActive;

        entity.Name = request.Name.Trim();
        entity.LogoUrl = request.LogoUrl;
        entity.AccountName = request.AccountName.Trim();
        entity.AccountNumber = request.AccountNumber.Trim();
        entity.QrCodeUrl = request.QrCodeUrl;
        entity.IsActive = request.IsActive;
        entity.DisplayOrder = request.DisplayOrder;
        entity.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync(cancellationToken);

        await _auditLogService.CreateAuditLogAsync(new CreateAuditLogRequest
        {
            UserId = userId,
            Action = "Update",
            TableName = "PaymentMethods",
            RecordId = entity.PaymentMethodId,
            Changes = new List<AuditLogChangeDto>
            {
                new() { Field = "Name", OldValue = oldName, NewValue = entity.Name },
                new() { Field = "IsActive", OldValue = oldActive.ToString(), NewValue = entity.IsActive.ToString() }
            }
        }, cancellationToken);

        _logger.LogInformation("Admin {UserId} updated PaymentMethod {PaymentMethodId} ({Name})", userId, entity.PaymentMethodId, entity.Name);

        return Result.Success(new PaymentMethodResponse
        {
            PaymentMethodId = entity.PaymentMethodId,
            Code = entity.Code,
            Name = entity.Name,
            LogoUrl = entity.LogoUrl,
            AccountName = entity.AccountName,
            AccountNumber = entity.AccountNumber,
            QrCodeUrl = entity.QrCodeUrl,
            IsActive = entity.IsActive,
            DisplayOrder = entity.DisplayOrder
        });
    }

    public async Task<Result<bool>> DeletePaymentMethodAsync(int id, int userId, CancellationToken cancellationToken = default)
    {
        var entity = await _context.PaymentMethods.FirstOrDefaultAsync(pm => pm.PaymentMethodId == id && !pm.DeleteFlag, cancellationToken);
        if (entity == null) return Result.Failure<bool>("Payment method not found.");

        entity.DeleteFlag = true;
        await _context.SaveChangesAsync(cancellationToken);

        await _auditLogService.CreateAuditLogAsync(new CreateAuditLogRequest
        {
            UserId = userId,
            Action = "Delete",
            TableName = "PaymentMethods",
            RecordId = entity.PaymentMethodId
        }, cancellationToken);

        _logger.LogInformation("Admin {UserId} deleted PaymentMethod {PaymentMethodId} ({Name})", userId, entity.PaymentMethodId, entity.Name);

        return Result.Success(true);
    }
}
