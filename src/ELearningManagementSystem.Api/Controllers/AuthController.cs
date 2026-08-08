using ELearningManagementSystem.Application.Features.Authentication.DTOs;
using ELearningManagementSystem.Application.Features.Authentication.Interfaces;
using ELearningManagementSystem.Application.Interfaces;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ELearningManagementSystem.Api.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly ICurrentUserService _currentUser;
    private readonly IValidator<RegisterRequest> _registerValidator;
    private readonly IValidator<LoginRequest> _loginValidator;
    private readonly IValidator<RefreshTokenRequest> _refreshValidator;
    private readonly ILogger<AuthController> _logger;

    public AuthController(
        IAuthService authService,
        ICurrentUserService currentUser,
        IValidator<RegisterRequest> registerValidator,
        IValidator<LoginRequest> loginValidator,
        IValidator<RefreshTokenRequest> refreshValidator,
        ILogger<AuthController> logger)
    {
        _authService = authService;
        _currentUser = currentUser;
        _registerValidator = registerValidator;
        _loginValidator = loginValidator;
        _refreshValidator = refreshValidator;
        _logger = logger;
    }

    /// <summary>POST /api/auth/register — Public student registration</summary>
    [HttpPost("register")]
    [AllowAnonymous]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request, CancellationToken cancellationToken)
    {
        var validation = await _registerValidator.ValidateAsync(request, cancellationToken);
        if (!validation.IsValid)
            return BadRequest(new { Errors = validation.Errors.Select(e => e.ErrorMessage) });

        var result = await _authService.RegisterAsync(request, cancellationToken);

        if (result.IsFailure)
            return Conflict(new { Error = result.Error });

        return CreatedAtAction(nameof(Me), null, result.Value);
    }

    /// <summary>POST /api/auth/login — Authenticate and receive token pair</summary>
    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<IActionResult> Login([FromBody] LoginRequest request, CancellationToken cancellationToken)
    {
        var validation = await _loginValidator.ValidateAsync(request, cancellationToken);
        if (!validation.IsValid)
            return BadRequest(new { Errors = validation.Errors.Select(e => e.ErrorMessage) });

        var result = await _authService.LoginAsync(request, cancellationToken);

        if (result.IsFailure)
            return Unauthorized(new { Error = result.Error });

        return Ok(result.Value);
    }

    /// <summary>POST /api/auth/refresh — Rotate a valid refresh token</summary>
    [HttpPost("refresh")]
    [AllowAnonymous]
    public async Task<IActionResult> Refresh([FromBody] RefreshTokenRequest request, CancellationToken cancellationToken)
    {
        var validation = await _refreshValidator.ValidateAsync(request, cancellationToken);
        if (!validation.IsValid)
            return BadRequest(new { Errors = validation.Errors.Select(e => e.ErrorMessage) });

        var result = await _authService.RefreshTokenAsync(request.RefreshToken, cancellationToken);

        if (result.IsFailure)
            return Unauthorized(new { Error = result.Error });

        return Ok(result.Value);
    }

    /// <summary>POST /api/auth/logout — Revoke the current refresh token</summary>
    [HttpPost("logout")]
    [AllowAnonymous] // Token may be expired; logout should still succeed if refresh token is valid
    public async Task<IActionResult> Logout([FromBody] RefreshTokenRequest request, CancellationToken cancellationToken)
    {
        await _authService.LogoutAsync(request.RefreshToken, cancellationToken);
        return Ok(new { Message = "Logged out successfully." });
    }

    /// <summary>GET /api/auth/me — Return the authenticated user profile</summary>
    [HttpGet("me")]
    [Authorize]
    public async Task<IActionResult> Me(CancellationToken cancellationToken)
    {
        var userId = _currentUser.UserId;

        if (userId is null)
            return Unauthorized(new { Error = "AuthenticationRequired: User is not authenticated." });

        var result = await _authService.GetCurrentUserAsync(userId.Value, cancellationToken);

        if (result.IsFailure)
            return NotFound(new { Error = result.Error });

        return Ok(result.Value);
    }
}
