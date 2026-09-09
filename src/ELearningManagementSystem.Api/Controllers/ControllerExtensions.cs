using Microsoft.AspNetCore.Mvc;
using ELearningManagementSystem.Application.Common;

namespace ELearningManagementSystem.Api.Controllers;

public static class ControllerExtensions
{
    public static IActionResult ToActionResult(this Result result)
    {
        if (result.IsSuccess)
        {
            return new NoContentResult();
        }

        return MapFailure(result.Error);
    }

    public static IActionResult ToActionResult<T>(this Result<T> result)
    {
        if (result.IsSuccess)
        {
            if (result.Value is null) return new NoContentResult();
            return new OkObjectResult(result.Value);
        }

        return MapFailure(result.Error);
    }

    private static IActionResult MapFailure(string? error)
    {
        if (string.IsNullOrWhiteSpace(error))
        {
            return new BadRequestObjectResult(new { Error = "An unknown error occurred." });
        }

        // Standardized mapping:
        // PermissionDenied / Unauthorized -> 403 Forbidden
        if (error == "PermissionDenied" || error.StartsWith("PermissionDenied"))
        {
            return new ObjectResult(new { Error = error }) { StatusCode = 403 };
        }

        // Concurrency conflict -> 409 Conflict
        if (error == "ConcurrencyConflict")
        {
            return new ConflictObjectResult(new { Error = "This record was modified by another user. Reload the latest version and try again." });
        }

        // NotFound -> 404 NotFound
        if (error.Contains("NotFound"))
        {
            return new NotFoundObjectResult(new { Error = error });
        }

        // Conflict -> 409 Conflict
        if (error.Contains("Already") || error.Contains("Conflict") || error == "CategoryInUse")
        {
            return new ConflictObjectResult(new { Error = error });
        }

        // Default to 400 BadRequest
        return new BadRequestObjectResult(new { Error = error });
    }
}
