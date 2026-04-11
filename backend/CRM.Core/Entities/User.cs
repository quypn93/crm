namespace CRM.Core.Entities;

public class User : BaseEntity
{
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
    public string? AvatarUrl { get; set; }
    public bool IsActive { get; set; } = true;
    public string? RefreshToken { get; set; }
    public DateTime? RefreshTokenExpiryTime { get; set; }
    public DateTime? LastLoginAt { get; set; }

    // Navigation properties
    public virtual ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
    public virtual ICollection<Customer> CreatedCustomers { get; set; } = new List<Customer>();
    public virtual ICollection<Customer> AssignedCustomers { get; set; } = new List<Customer>();
    public virtual ICollection<Deal> CreatedDeals { get; set; } = new List<Deal>();
    public virtual ICollection<Deal> AssignedDeals { get; set; } = new List<Deal>();
    public virtual ICollection<TaskItem> CreatedTasks { get; set; } = new List<TaskItem>();
    public virtual ICollection<TaskItem> AssignedTasks { get; set; } = new List<TaskItem>();
    public virtual ICollection<ActivityLog> ActivityLogs { get; set; } = new List<ActivityLog>();
    public virtual ICollection<Order> CreatedOrders { get; set; } = new List<Order>();
    public virtual ICollection<Order> AssignedOrders { get; set; } = new List<Order>();

    public string FullName => $"{FirstName} {LastName}";
}
