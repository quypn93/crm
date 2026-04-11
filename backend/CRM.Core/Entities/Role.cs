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

    // Sales
    public const string SalesManager = "SalesManager";
    public const string SalesRep = "SalesRep";

    // Production
    public const string ProductionManager = "ProductionManager";
    public const string ProductionStaff = "ProductionStaff";

    // Quality Control
    public const string QualityManager = "QualityManager";
    public const string QualityControl = "QualityControl";

    // Delivery
    public const string DeliveryManager = "DeliveryManager";
    public const string DeliveryStaff = "DeliveryStaff";

    // Design
    public const string DesignManager = "DesignManager";
    public const string Designer = "Designer";

    // Role groups for authorization
    public static readonly string[] AllRoles = {
        Admin,
        SalesManager, SalesRep,
        ProductionManager, ProductionStaff,
        QualityManager, QualityControl,
        DeliveryManager, DeliveryStaff,
        DesignManager, Designer
    };
    public static readonly string[] SalesRoles = { Admin, SalesManager, SalesRep };
    public static readonly string[] ManagerRoles = { Admin, SalesManager };
    public static readonly string[] ProductionRoles = { Admin, ProductionManager, ProductionStaff };
    public static readonly string[] QualityRoles = { Admin, QualityManager, QualityControl };
    public static readonly string[] DeliveryRoles = { Admin, DeliveryManager, DeliveryStaff };
    public static readonly string[] DesignRoles = { Admin, DesignManager, Designer };
    public static readonly string[] OperationalRoles = {
        Admin, ProductionManager, ProductionStaff,
        QualityManager, QualityControl,
        DeliveryManager, DeliveryStaff
    };
}
