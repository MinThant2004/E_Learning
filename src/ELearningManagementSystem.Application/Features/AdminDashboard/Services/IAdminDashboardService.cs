using System.Threading.Tasks;
using ELearningManagementSystem.Application.Common;
using ELearningManagementSystem.Application.Features.AdminDashboard.DTOs;

namespace ELearningManagementSystem.Application.Features.AdminDashboard.Services
{
    public interface IAdminDashboardService
    {
        Task<Result<AdminDashboardResponse>> GetDashboardAsync(string range = "monthly", DateTime? customStartDate = null, DateTime? customEndDate = null);
    }
}
