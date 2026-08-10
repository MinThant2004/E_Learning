using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ELearningManagementSystem.Application.Features.Permissions.Services;

namespace ELearningManagementSystem.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
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
        bool hasPermission = User.HasClaim(c => c.Type == "permission" && 
            (c.Value == "Permission.Read" || c.Value == "Permission.Assign"));
        bool isAdmin = User.IsInRole("Admin") || User.IsInRole("SuperAdmin");

        if (!hasPermission && !isAdmin)
        {
            return Forbid();
        }

        var result = await _permissionService.GetAllPermissionsAsync();
        if (result.IsSuccess) return Ok(result.Value);
        return BadRequest(new { error = result.Error });
    }
}
