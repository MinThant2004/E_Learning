using ELearningManagementSystem.Application.Common;
using ELearningManagementSystem.Application.Features.StudentDashboard.DTOs;
using System.Threading.Tasks;

namespace ELearningManagementSystem.Application.Features.StudentDashboard.Services
{
    public interface IStudentDashboardService
    {
        Task<Result<StudentDashboardResponse>> GetDashboardSummaryAsync();
    }
}
