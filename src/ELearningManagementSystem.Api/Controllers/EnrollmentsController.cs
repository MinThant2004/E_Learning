using System.Threading;
using System.Threading.Tasks;
using ELearningManagementSystem.Application.Features.Enrollments.DTOs;
using ELearningManagementSystem.Application.Features.Enrollments.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ELearningManagementSystem.Api.Controllers;

[ApiController]
[Route("api")]
[Authorize]
public class EnrollmentsController : ControllerBase
{
    private readonly IEnrollmentService _enrollmentService;

    public EnrollmentsController(IEnrollmentService enrollmentService)
    {
        _enrollmentService = enrollmentService;
    }

    [HttpPost("courses/{courseId:int}/enrollments")]
    public async Task<IActionResult> Enroll(int courseId, CancellationToken cancellationToken)
    {
        var result = await _enrollmentService.EnrollAsync(courseId, cancellationToken);

        if (result.IsSuccess)
        {
            return Ok(result.Value);
        }

        return result.Error switch
        {
            "CourseNotFound" => NotFound(new { error = "Course not found." }),
            "CourseArchived" => BadRequest(new { error = "Cannot enroll in an archived course." }),
            "AlreadyEnrolled" => Conflict(new { error = "You are already enrolled in this course." }),
            "AccountNotFound" => Unauthorized(new { error = "Account not found." }),
            _ => BadRequest(new { error = result.Error })
        };
    }

    [HttpGet("enrollments/my")]
    public async Task<IActionResult> GetMyEnrollments(CancellationToken cancellationToken)
    {
        var result = await _enrollmentService.GetMyEnrollmentsAsync(cancellationToken);

        if (result.IsSuccess)
        {
            return Ok(result.Value);
        }

        if (result.Error == "AccountNotFound")
            return Unauthorized(new { error = "Account not found." });

        return BadRequest(new { error = result.Error });
    }

    [HttpGet("courses/{courseId:int}/enrollment")]
    public async Task<IActionResult> CheckEnrollmentStatus(int courseId, CancellationToken cancellationToken)
    {
        var result = await _enrollmentService.CheckEnrollmentStatusAsync(courseId, cancellationToken);
        
        if (result.IsSuccess)
        {
            return Ok(new { isEnrolled = result.Value });
        }
        
        return BadRequest(new { error = result.Error });
    }
}
