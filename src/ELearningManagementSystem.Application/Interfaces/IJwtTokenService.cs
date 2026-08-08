using System.Collections.Generic;
using ELearningManagementSystem.Domain.Entities;

namespace ELearningManagementSystem.Application.Interfaces;

public interface IJwtTokenService
{
    string GenerateToken(User user, string roleName, IEnumerable<string> permissions);
    DateTime GetExpiry();
}
