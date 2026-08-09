using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ELearningManagementSystem.Application.Features.Permissions.Services;

namespace ELearningManagementSystem.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Policy = "Permission:Permission.Read")]
public class PermissionsController : ControllerBase
{
    private readonly IPermissionManagementService _permissionService;

    public PermissionsController(IPermissionManagementService permissionService)
    {
        _permissionService = permissionService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAllPermissions()
    {
        var result = await _permissionService.GetAllPermissionsAsync();
        if (result.IsSuccess) return Ok(result.Value);
        return BadRequest(new { error = result.Error });
    }
}
