using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ELearningManagementSystem.Application.Features.Courses.DTOs;
using ELearningManagementSystem.Application.Features.Courses.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace ELearningManagementSystem.Api.Controllers;

[ApiController]
[Route("api/courses")]
public class CoursesController : ControllerBase
{
    private readonly ICourseService _courseService;
    private readonly IWebHostEnvironment _env;

    public CoursesController(ICourseService courseService, IWebHostEnvironment env)
    {
        _courseService = courseService;
        _env = env;
    }

    /// <summary>GET /api/courses — Browse active courses</summary>
    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> GetCourses([FromQuery] CourseListQuery query, CancellationToken cancellationToken)
    {
        var result = await _courseService.GetPagedListAsync(query, cancellationToken);
        if (result.IsFailure) return BadRequest(new { Error = result.Error });
        return Ok(result.Value);
    }

    /// <summary>GET /api/courses/{id} — View one active course</summary>
    [HttpGet("{id}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetCourse(int id, CancellationToken cancellationToken)
    {
        var result = await _courseService.GetByIdAsync(id, cancellationToken);
        if (result.IsFailure) return NotFound(new { Error = result.Error });
        return Ok(result.Value);
    }

    /// <summary>POST /api/courses — Create a course with optional thumbnail</summary>
    [HttpPost]
    [Authorize(Policy = "Permission:Course.Create")]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> CreateCourse([FromForm] CreateCourseForm form, CancellationToken cancellationToken)
    {
        string? thumbnailUrl = null;
        if (form.Thumbnail != null)
        {
            var (path, error) = await SaveThumbnailAsync(form.Thumbnail);
            if (error != null)
                return BadRequest(new { Error = error });
            thumbnailUrl = path;
        }

        var request = new CreateCourseRequest
        {
            Title = form.Title,
            Description = form.Description,
            CategoryId = form.CategoryId,
            ThumbnailUrl = thumbnailUrl
        };

        var result = await _courseService.CreateAsync(request, cancellationToken);
        if (result.IsFailure)
        {
            // Cleanup saved thumbnail on failure
            if (thumbnailUrl != null)
                DeleteThumbnail(thumbnailUrl);
            return BadRequest(new { Error = result.Error });
        }

        return Created($"/api/courses/{result.Value!.CourseId}", result.Value);
    }

    /// <summary>PUT /api/courses/{id} — Update a course with optional thumbnail</summary>
    [HttpPut("{id}")]
    [Authorize(Policy = "Permission:Course.Update")]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> UpdateCourse(int id, [FromForm] UpdateCourseForm form, CancellationToken cancellationToken)
    {
        var existingResult = await _courseService.GetByIdAsync(id, cancellationToken);
        if (existingResult.IsFailure)
            return NotFound(new { Error = "CourseNotFound" });

        var oldThumbnailUrl = existingResult.Value?.ThumbnailUrl;
        string? newThumbnailUrl = null;
        bool hasNewThumbnail = false;

        if (form.Thumbnail != null)
        {
            var (path, error) = await SaveThumbnailAsync(form.Thumbnail);
            if (error != null)
                return BadRequest(new { Error = error });
            newThumbnailUrl = path;
            hasNewThumbnail = true;
        }

        var request = new UpdateCourseRequest
        {
            Title = form.Title,
            Description = form.Description,
            CategoryId = form.CategoryId,
            ThumbnailUrl = hasNewThumbnail ? newThumbnailUrl : null,
            RemoveThumbnail = form.RemoveThumbnail
        };

        var result = await _courseService.UpdateAsync(id, request, cancellationToken);
        if (result.IsFailure)
        {
            if (newThumbnailUrl != null)
                DeleteThumbnail(newThumbnailUrl);
            if (result.Error == "CourseNotFound") return NotFound(new { Error = result.Error });
            return BadRequest(new { Error = result.Error });
        }

        // Cleanup old thumbnail file if removed or replaced
        if (form.RemoveThumbnail || hasNewThumbnail)
        {
            if (!string.IsNullOrEmpty(oldThumbnailUrl))
                DeleteThumbnail(oldThumbnailUrl);
        }

        return Ok(result.Value);
    }

    /// <summary>DELETE /api/courses/{id} — Soft-delete a course</summary>
    [HttpDelete("{id}")]
    [Authorize(Policy = "Permission:Course.Delete")]
    public async Task<IActionResult> DeleteCourse(int id, CancellationToken cancellationToken)
    {
        var result = await _courseService.SoftDeleteAsync(id, cancellationToken);
        if (result.IsFailure)
        {
            if (result.Error == "CourseNotFound") return NotFound(new { Error = result.Error });
            return BadRequest(new { Error = result.Error });
        }
        return Ok(new { Message = "Course deleted successfully." });
    }

    // ── Helpers ─────────────────────────────────────────────────────────────

    private async Task<(string? path, string? error)> SaveThumbnailAsync(IFormFile file)
    {
        if (!file.ContentType.StartsWith("image/", StringComparison.OrdinalIgnoreCase))
            return (null, "Uploaded file must be an image.");

        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".webp" };
        if (!allowedExtensions.Contains(extension))
            return (null, "Only .jpg, .jpeg, .png, and .webp images are allowed.");

        var allowedMimeTypes = new[] { "image/jpeg", "image/png", "image/webp" };
        if (!allowedMimeTypes.Contains(file.ContentType.ToLowerInvariant()))
            return (null, "Only JPEG, PNG, and WebP images are allowed.");

        if (file.Length > 5 * 1024 * 1024)
            return (null, "Image file size must be less than 5 MB.");

        var fileName = $"{Guid.NewGuid()}{extension}";
        var uploadDir = Path.Combine(_env.WebRootPath, "uploads", "courses");

        if (!Directory.Exists(uploadDir))
            Directory.CreateDirectory(uploadDir);

        var filePath = Path.Combine(uploadDir, fileName);
        using (var stream = new FileStream(filePath, FileMode.Create))
        {
            await file.CopyToAsync(stream);
        }

        return ($"/uploads/courses/{fileName}", null);
    }

    private void DeleteThumbnail(string relativeUrl)
    {
        try
        {
            if (string.IsNullOrEmpty(relativeUrl)) return;
            var normalizedPath = relativeUrl.TrimStart('/');
            var absolutePath = Path.Combine(_env.WebRootPath, normalizedPath);
            if (System.IO.File.Exists(absolutePath))
            {
                System.IO.File.Delete(absolutePath);
            }
        }
        catch
        {
            // Ignore failure
        }
    }
}

public class CreateCourseForm
{
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int CategoryId { get; set; }
    public IFormFile? Thumbnail { get; set; }
}

public class UpdateCourseForm
{
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public int CategoryId { get; set; }
    public IFormFile? Thumbnail { get; set; }
    public bool RemoveThumbnail { get; set; }
}
