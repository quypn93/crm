namespace CRM.Core.Entities;

public class Customer : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public string? Address { get; set; }
    public string? City { get; set; }
    public string? Country { get; set; }
    public string? PostalCode { get; set; }
    public string? CompanyName { get; set; }
    public string? Industry { get; set; }
    public string? Website { get; set; }
    public string? Notes { get; set; }
    public bool IsActive { get; set; } = true;

    // Foreign keys
    public Guid CreatedByUserId { get; set; }
    public Guid? AssignedToUserId { get; set; }

    // Navigation properties
    public virtual User CreatedByUser { get; set; } = null!;
    public virtual User? AssignedToUser { get; set; }
    public virtual ICollection<Deal> Deals { get; set; } = new List<Deal>();
    public virtual ICollection<TaskItem> Tasks { get; set; } = new List<TaskItem>();
}
