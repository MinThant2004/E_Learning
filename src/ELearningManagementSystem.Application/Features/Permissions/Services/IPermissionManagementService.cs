using System.Collections.Generic;
using System.Threading.Tasks;
using ELearningManagementSystem.Application.Common;
using ELearningManagementSystem.Application.Features.Permissions.DTOs;

namespace ELearningManagementSystem.Application.Features.Permissions.Services;

public interface IPermissionManagementService
{
    Task<Result<List<PermissionResponse>>> GetAllPermissionsAsync();
}
