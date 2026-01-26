namespace CRM.Core.Entities;

public class Role : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }

    // Navigation properties
    public virtual ICollection<UserRole> UserRoles { get; set; } = new List<UserRole>();
}

public static class RoleNames
{
    public const string Admin = "Admin";
    public const string SalesManager = "SalesManager";
    public const string SalesRep = "SalesRep";
}
