using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ELearningManagementSystem.Application.Features.Users.DTOs;
using ELearningManagementSystem.Application.Features.Users.Services;

namespace ELearningManagementSystem.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class UsersController : ControllerBase
{
    private readonly IUserService _userService;

    public UsersController(IUserService userService)
    {
        _userService = userService;
    }

    [HttpGet]
    [Authorize(Policy = "Permission:User.Read")]
    public async Task<IActionResult> GetUsers([FromQuery] UserListQuery query)
    {
        var result = await _userService.GetPagedUsersAsync(query);
        return Ok(result);
    }

    [HttpGet("roles")]
    [Authorize(Policy = "Permission:User.Read")]
    public async Task<IActionResult> GetAvailableRoles()
    {
        var result = await _userService.GetAvailableRolesAsync();
        return Ok(result.Value);
    }

    [HttpGet("{id}")]
    [Authorize(Policy = "Permission:User.Read")]
    public async Task<IActionResult> GetUserById(int id)
    {
        var result = await _userService.GetUserByIdAsync(id);
        if (result.IsSuccess)
            return Ok(result.Value);
            
        return NotFound(result.Error);
    }

    [HttpPut("{id}")]
    [Authorize(Policy = "Permission:User.Update")]
    public async Task<IActionResult> UpdateUser(int id, [FromBody] UpdateUserRequest request)
    {
        var currentUserIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!int.TryParse(currentUserIdClaim, out int currentUserId))
            return Unauthorized();

        var result = await _userService.UpdateUserAsync(id, request, currentUserId);
        if (result.IsSuccess)
            return NoContent();

        return BadRequest(new { error = result.Error });
    }

    [HttpPost("{id}/archive")]
    [Authorize(Policy = "Permission:User.Update")]
    public async Task<IActionResult> ArchiveUser(int id)
    {
        var currentUserIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!int.TryParse(currentUserIdClaim, out int currentUserId))
            return Unauthorized();

        var result = await _userService.ArchiveUserAsync(id, currentUserId);
        if (result.IsSuccess)
            return NoContent();

        return BadRequest(new { error = result.Error });
    }

    [HttpPost("{id}/restore")]
    [Authorize(Policy = "Permission:User.Update")]
    public async Task<IActionResult> RestoreUser(int id)
    {
        var currentUserIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (!int.TryParse(currentUserIdClaim, out int currentUserId))
            return Unauthorized();

        var result = await _userService.RestoreUserAsync(id, currentUserId);
        if (result.IsSuccess)
            return NoContent();

        return BadRequest(new { error = result.Error });
    }
}
