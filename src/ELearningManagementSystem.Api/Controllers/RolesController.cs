using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ELearningManagementSystem.Application.Features.Roles.DTOs;
using ELearningManagementSystem.Application.Features.Roles.Services;

namespace ELearningManagementSystem.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class RolesController : ControllerBase
{
    private readonly IRoleService _roleService;

    public RolesController(IRoleService roleService)
    {
        _roleService = roleService;
    }

    [HttpGet]
    [Authorize(Policy = "Permission:Role.Read")]
    public async Task<IActionResult> GetRoles([FromQuery] RoleListQuery query)
    {
        var result = await _roleService.GetRolesAsync(query);
        return Ok(result);
    }

    [HttpGet("{id}")]
    [Authorize(Policy = "Permission:Role.Read")]
    public async Task<IActionResult> GetRole(int id)
    {
        var result = await _roleService.GetRoleByIdAsync(id);
        if (result.IsSuccess) return Ok(result.Value);
        return NotFound(new { error = result.Error });
    }

    [HttpPost]
    [Authorize(Policy = "Permission:Role.Create")]
    public async Task<IActionResult> CreateRole([FromBody] CreateRoleRequest request)
    {
        var result = await _roleService.CreateRoleAsync(request);
        if (result.IsSuccess) return Ok(new { id = result.Value });
        return BadRequest(new { error = result.Error });
    }

    [HttpPut("{id}")]
    [Authorize(Policy = "Permission:Role.Update")]
    public async Task<IActionResult> UpdateRole(int id, [FromBody] UpdateRoleRequest request)
    {
        var result = await _roleService.UpdateRoleAsync(id, request);
        if (result.IsSuccess) return Ok();
        return BadRequest(new { error = result.Error });
    }

    [HttpPut("{id}/permissions")]
    [Authorize(Policy = "Permission:Permission.Assign")]
    public async Task<IActionResult> AssignPermissions(int id, [FromBody] AssignPermissionsRequest request)
    {
        var result = await _roleService.AssignPermissionsAsync(id, request);
        if (result.IsSuccess) return Ok();
        return result.ToActionResult();
    }
}
