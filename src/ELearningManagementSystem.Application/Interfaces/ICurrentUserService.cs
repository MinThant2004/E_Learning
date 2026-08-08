namespace ELearningManagementSystem.Application.Interfaces;

public interface ICurrentUserService
{
    int? UserId { get; }
    string? Email { get; }
    string? FullName { get; }
    bool IsAuthenticated { get; }
}
