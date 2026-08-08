namespace ELearningManagementSystem.Domain.Entities;

public partial class Permission
{
    public int PermissionId { get; set; }
    public string PermissionCode { get; set; } = null!;
    public string PermissionName { get; set; } = null!;
    public string Module { get; set; } = null!;

    public virtual ICollection<RolePermission> RolePermissions { get; set; } = new List<RolePermission>();
}
