using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using ELearningManagementSystem.Application.Common;
using ELearningManagementSystem.Application.Interfaces;
using ELearningManagementSystem.Application.Features.Permissions.DTOs;

namespace ELearningManagementSystem.Application.Features.Permissions.Services;

public class PermissionManagementService : IPermissionManagementService
{
    private readonly IAppDbContext _context;

    public PermissionManagementService(IAppDbContext context)
    {
        _context = context;
    }

    public async Task<Result<List<PermissionResponse>>> GetAllPermissionsAsync()
    {
        var permissions = await _context.Permissions
            .AsNoTracking()
            .Select(p => new PermissionResponse
            {
                PermissionId = p.PermissionId,
                PermissionCode = p.PermissionCode,
                PermissionName = p.PermissionName,
                Module = p.Module
            })
            .ToListAsync();

        return Result<List<PermissionResponse>>.Success(permissions);
    }
}
