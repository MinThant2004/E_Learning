using ELearningManagementSystem.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ELearningManagementSystem.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class HealthController : ControllerBase
{
    private readonly IAppDbContext _context;

    public HealthController(IAppDbContext context)
    {
        _context = context;
    }

    [HttpGet("db-check")]
    public async Task<IActionResult> CheckDatabaseConnection(CancellationToken cancellationToken)
    {
        try
        {
            var dbContext = (DbContext)_context;
            bool canConnect = await dbContext.Database.CanConnectAsync(cancellationToken);

            if (!canConnect)
            {
                return StatusCode(500, new
                {
                    Status = "Error",
                    Message = "Unable to connect to SQL Server database ELearningManagementSystem."
                });
            }

            var userCount = await _context.Users.CountAsync(cancellationToken);
            var courseCount = await _context.Courses.CountAsync(cancellationToken);
            var roleCount = await _context.Roles.CountAsync(cancellationToken);

            return Ok(new
            {
                Status = "Healthy",
                Message = "Successfully connected to existing SQL Server database ELearningManagementSystem.",
                DatabaseMetrics = new
                {
                    UserCount = userCount,
                    CourseCount = courseCount,
                    RoleCount = roleCount
                },
                Timestamp = DateTime.UtcNow
            });
        }
        catch (Exception ex)
        {
            return StatusCode(500, new
            {
                Status = "Exception",
                Message = "Database connectivity check failed.",
                Error = ex.Message
            });
        }
    }
}
