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
    public const string ProductionStaff = "ProductionStaff"; // generic fallback, giữ tương thích

    // Production stage-specific roles (mỗi khâu 1 role)
    public const string CuttingStaff    = "CuttingStaff";    // Cắt vải
    public const string SewingStaff     = "SewingStaff";     // May
    public const string PrintingStaff   = "PrintingStaff";   // In / Thêu logo
    public const string FinishingStaff  = "FinishingStaff";  // Hoàn thiện (vệ sinh, cắt chỉ)
    public const string PackagingStaff  = "PackagingStaff";  // Đóng gói

    // Quality Control
    public const string QualityManager = "QualityManager";
    public const string QualityControl = "QualityControl"; // Kiểm tra chất lượng

    // Delivery
    public const string DeliveryManager = "DeliveryManager";
    public const string DeliveryStaff = "DeliveryStaff";

    // Design
    public const string DesignManager = "DesignManager";
    public const string Designer = "Designer";

    // Content (giao việc cho design)
    public const string ContentManager = "ContentManager";
    public const string ContentStaff = "ContentStaff";

    // Role groups for authorization
    public static readonly string[] AllRoles = {
        Admin,
        SalesManager, SalesRep,
        ProductionManager, ProductionStaff,
        CuttingStaff, SewingStaff, PrintingStaff, FinishingStaff, PackagingStaff,
        QualityManager, QualityControl,
        DeliveryManager, DeliveryStaff,
        DesignManager, Designer,
        ContentManager, ContentStaff
    };
    public static readonly string[] SalesRoles = { Admin, SalesManager, SalesRep };
    public static readonly string[] ManagerRoles = { Admin, SalesManager };
    public static readonly string[] ProductionRoles = {
        Admin, ProductionManager, ProductionStaff,
        CuttingStaff, SewingStaff, PrintingStaff, FinishingStaff, PackagingStaff
    };
    // Các role chuyên môn hóa cho từng khâu sản xuất
    public static readonly string[] ProductionStageRoles = {
        CuttingStaff, SewingStaff, PrintingStaff, FinishingStaff, PackagingStaff
    };
    public static readonly string[] QualityRoles = { Admin, QualityManager, QualityControl };
    public static readonly string[] DeliveryRoles = { Admin, DeliveryManager, DeliveryStaff };
    public static readonly string[] DesignRoles = { Admin, DesignManager, Designer };
    public static readonly string[] OperationalRoles = {
        Admin, ProductionManager, ProductionStaff,
        CuttingStaff, SewingStaff, PrintingStaff, FinishingStaff, PackagingStaff,
        QualityManager, QualityControl,
        DeliveryManager, DeliveryStaff
    };
}
