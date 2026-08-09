using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;

namespace ELearningManagementSystem.Application.Features.Users.DTOs;

public class UpdateUserRequest
{
    [Required]
    [MaxLength(100)]
    public string FullName { get; set; } = string.Empty;

    public bool Status { get; set; }
    public List<string> Roles { get; set; } = new();
}
