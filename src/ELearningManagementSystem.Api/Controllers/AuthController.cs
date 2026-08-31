using ELearningManagementSystem.Application.Features.Authentication.DTOs;
using ELearningManagementSystem.Application.Features.Authentication.Interfaces;
using ELearningManagementSystem.Application.Features.Users.DTOs;
using ELearningManagementSystem.Application.Features.Users.Services;
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
    private readonly IUserService _userService;
    private readonly ICurrentUserService _currentUser;
    private readonly IValidator<RegisterRequest> _registerValidator;
    private readonly IValidator<LoginRequest> _loginValidator;
    private readonly IValidator<RefreshTokenRequest> _refreshValidator;
    private readonly IValidator<ChangePasswordRequest> _changePasswordValidator;
    private readonly IValidator<ForgotPasswordRequest> _forgotPasswordValidator;
    private readonly IValidator<VerifyOtpRequest> _verifyOtpValidator;
    private readonly IValidator<ResetPasswordRequest> _resetPasswordValidator;
    private readonly ILogger<AuthController> _logger;

    public AuthController(
        IAuthService authService,
        IUserService userService,
        ICurrentUserService currentUser,
        IValidator<RegisterRequest> registerValidator,
        IValidator<LoginRequest> loginValidator,
        IValidator<RefreshTokenRequest> refreshValidator,
        IValidator<ChangePasswordRequest> changePasswordValidator,
        IValidator<ForgotPasswordRequest> forgotPasswordValidator,
        IValidator<VerifyOtpRequest> verifyOtpValidator,
        IValidator<ResetPasswordRequest> resetPasswordValidator,
        ILogger<AuthController> logger)
    {
        _authService = authService;
        _userService = userService;
        _currentUser = currentUser;
        _registerValidator = registerValidator;
        _loginValidator = loginValidator;
        _refreshValidator = refreshValidator;
        _changePasswordValidator = changePasswordValidator;
        _forgotPasswordValidator = forgotPasswordValidator;
        _verifyOtpValidator = verifyOtpValidator;
        _resetPasswordValidator = resetPasswordValidator;
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

    /// <summary>POST /api/auth/change-password — Change the authenticated user's own password</summary>
    [HttpPost("change-password")]
    [Authorize]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request, CancellationToken cancellationToken)
    {
        var validation = await _changePasswordValidator.ValidateAsync(request, cancellationToken);
        if (!validation.IsValid)
            return BadRequest(new { Errors = validation.Errors.Select(e => e.ErrorMessage) });

        // Identity always comes from the server-side JWT claim — never trust a client-supplied UserId.
        var userId = _currentUser.UserId;
        if (userId is null)
            return Unauthorized(new { Error = "AuthenticationRequired: User is not authenticated." });

        var result = await _userService.ChangePasswordAsync(userId.Value, request);

        if (result.IsFailure)
            return BadRequest(new { Error = result.Error });

        return Ok(new { Message = "Password changed successfully." });
    }

    /// <summary>POST /api/auth/forgot-password — Send a password reset link to the given email</summary>
    [HttpPost("forgot-password")]
    [AllowAnonymous]
    public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequest request, CancellationToken cancellationToken)
    {
        var validation = await _forgotPasswordValidator.ValidateAsync(request, cancellationToken);
        if (!validation.IsValid)
            return BadRequest(new { Errors = validation.Errors.Select(e => e.ErrorMessage) });

        var result = await _authService.ForgotPasswordAsync(request.Email, cancellationToken);

        if (result.IsFailure)
            return BadRequest(new { Error = result.Error });

        // Always return the same message regardless of whether the email exists.
        return Ok(new { Message = "If an account exists for this email, a one-time password has been sent." });
    }

    /// <summary>POST /api/auth/verify-otp — Validate a one-time password without resetting the password</summary>
    [HttpPost("verify-otp")]
    [AllowAnonymous]
    public async Task<IActionResult> VerifyOtp([FromBody] VerifyOtpRequest request, CancellationToken cancellationToken)
    {
        var validation = await _verifyOtpValidator.ValidateAsync(request, cancellationToken);
        if (!validation.IsValid)
            return BadRequest(new { Errors = validation.Errors.Select(e => e.ErrorMessage) });

        var result = await _authService.VerifyOtpAsync(request.Otp, cancellationToken);

        if (result.IsFailure)
            return BadRequest(new { Error = result.Error });

        return Ok(new { Message = "OTP verified successfully." });
    }

    /// <summary>POST /api/auth/reset-password — Reset the password using a valid OTP</summary>
    [HttpPost("reset-password")]
    [AllowAnonymous]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest request, CancellationToken cancellationToken)
    {
        var validation = await _resetPasswordValidator.ValidateAsync(request, cancellationToken);
        if (!validation.IsValid)
            return BadRequest(new { Errors = validation.Errors.Select(e => e.ErrorMessage) });

        var result = await _authService.ResetPasswordAsync(request.Otp, request.NewPassword, cancellationToken);

        if (result.IsFailure)
            return BadRequest(new { Error = result.Error });

        return Ok(new { Message = "Your password has been reset. You can now sign in." });
    }
}
