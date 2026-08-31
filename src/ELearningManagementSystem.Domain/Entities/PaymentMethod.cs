namespace ELearningManagementSystem.Domain.Entities;

public partial class PaymentMethod
{
    public int PaymentMethodId { get; set; }
    public string Code { get; set; } = null!;
    public string Name { get; set; } = null!;
    public string? LogoUrl { get; set; }
    public string AccountName { get; set; } = null!;
    public string AccountNumber { get; set; } = null!;
    public string? QrCodeUrl { get; set; }
    public bool IsActive { get; set; } = true;
    public int DisplayOrder { get; set; } = 1;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
    public bool DeleteFlag { get; set; } = false;
}
