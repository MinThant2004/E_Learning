using System.IO;
using ELearningManagementSystem.Application.Features.PaymentMethods.DTOs;
using ELearningManagementSystem.Application.Features.PaymentMethods.Services;
using ELearningManagementSystem.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace ELearningManagementSystem.Api.Controllers;

[ApiController]
[Route("api/payment-methods")]
public class PaymentMethodsController : ControllerBase
{
    private readonly IPaymentMethodService _paymentMethodService;
    private readonly IWebHostEnvironment _env;
    private readonly ICurrentUserService _currentUser;

    public PaymentMethodsController(IPaymentMethodService paymentMethodService, IWebHostEnvironment env, ICurrentUserService currentUser)
    {
        _paymentMethodService = paymentMethodService;
        _env = env;
        _currentUser = currentUser;
    }

    /// <summary>GET /api/payment-methods/active — Get active payment methods for student modal</summary>
    [HttpGet("active")]
    [Authorize]
    public async Task<IActionResult> GetActivePaymentMethods(CancellationToken cancellationToken)
    {
        var result = await _paymentMethodService.GetActivePaymentMethodsAsync(cancellationToken);
        return result.ToActionResult();
    }

    /// <summary>GET /api/payment-methods/admin — Get all payment methods for admin management</summary>
    [HttpGet("admin")]
    [Authorize(Policy = "Permission:Course.Update")]
    public async Task<IActionResult> GetAllPaymentMethodsAdmin(CancellationToken cancellationToken)
    {
        var result = await _paymentMethodService.GetAllPaymentMethodsAdminAsync(cancellationToken);
        return result.ToActionResult();
    }

    /// <summary>POST /api/payment-methods/admin — Create new payment method</summary>
    [HttpPost("admin")]
    [Authorize(Policy = "Permission:Course.Update")]
    public async Task<IActionResult> CreatePaymentMethod([FromBody] CreatePaymentMethodRequest request, CancellationToken cancellationToken)
    {
        var result = await _paymentMethodService.CreatePaymentMethodAsync(request, _currentUser.UserId ?? 0, cancellationToken);
        return result.ToActionResult();
    }

    /// <summary>PUT /api/payment-methods/admin/{id:int} — Update payment method</summary>
    [HttpPut("admin/{id:int}")]
    [Authorize(Policy = "Permission:Course.Update")]
    public async Task<IActionResult> UpdatePaymentMethod(int id, [FromBody] UpdatePaymentMethodRequest request, CancellationToken cancellationToken)
    {
        var result = await _paymentMethodService.UpdatePaymentMethodAsync(id, request, _currentUser.UserId ?? 0, cancellationToken);
        return result.ToActionResult();
    }

    /// <summary>DELETE /api/payment-methods/admin/{id:int} — Delete payment method</summary>
    [HttpDelete("admin/{id:int}")]
    [Authorize(Policy = "Permission:Course.Update")]
    public async Task<IActionResult> DeletePaymentMethod(int id, CancellationToken cancellationToken)
    {
        var result = await _paymentMethodService.DeletePaymentMethodAsync(id, _currentUser.UserId ?? 0, cancellationToken);
        return result.ToActionResult();
    }

    /// <summary>POST /api/payment-methods/upload-asset — Upload logo or QR code image from desktop</summary>
    [HttpPost("upload-asset")]
    [Authorize(Policy = "Permission:Course.Update")]
    public async Task<IActionResult> UploadPaymentAsset(IFormFile file, CancellationToken cancellationToken)
    {
        if (file == null || file.Length == 0)
            return BadRequest(new { Error = "FileRequired: Image file is required." });

        var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".webp", ".svg" };
        var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (!allowedExtensions.Contains(ext))
            return BadRequest(new { Error = "InvalidFileType: Only JPG, PNG, WEBP, or SVG image files are allowed." });

        if (file.Length > 5 * 1024 * 1024)
            return BadRequest(new { Error = "FileTooLarge: Image file size must not exceed 5MB." });

        var uploadsDir = Path.Combine(_env.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot"), "uploads", "payment-methods");
        if (!Directory.Exists(uploadsDir))
            Directory.CreateDirectory(uploadsDir);

        var fileName = $"{Guid.NewGuid()}{ext}";
        var filePath = Path.Combine(uploadsDir, fileName);

        using (var stream = new FileStream(filePath, FileMode.Create))
        {
            await file.CopyToAsync(stream, cancellationToken);
        }

        var relativeUrl = $"/uploads/payment-methods/{fileName}";
        return Ok(new { Url = relativeUrl });
    }
}
