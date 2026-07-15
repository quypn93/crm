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
    public const string WaybillStaff    = "WaybillStaff";    // Vận đơn (chọn kho, nhập địa chỉ nhận, tạo vận đơn)

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

    // Marketing
    public const string MarketingManager = "MarketingManager";
    public const string MediaMarketing = "MediaMarketing";
    public const string DigitalAds = "DigitalAds";
    public const string Media = "Media";

    // Role groups for authorization
    public static readonly string[] AllRoles = {
        Admin,
        SalesManager, SalesRep,
        ProductionManager, ProductionStaff,
        CuttingStaff, SewingStaff, PrintingStaff, FinishingStaff, PackagingStaff, WaybillStaff,
        QualityManager, QualityControl,
        DeliveryManager, DeliveryStaff,
        DesignManager, Designer,
        ContentManager, ContentStaff,
        MarketingManager, MediaMarketing, DigitalAds, Media
    };
    public static readonly string[] SalesRoles = { Admin, SalesManager, SalesRep };
    public static readonly string[] ManagerRoles = { Admin, SalesManager };
    public static readonly string[] ProductionRoles = {
        Admin, ProductionManager, ProductionStaff,
        CuttingStaff, SewingStaff, PrintingStaff, FinishingStaff, PackagingStaff, WaybillStaff
    };
    // Các role chuyên môn hóa cho từng khâu sản xuất
    public static readonly string[] ProductionStageRoles = {
        CuttingStaff, SewingStaff, PrintingStaff, FinishingStaff, PackagingStaff
    };
    public static readonly string[] QualityRoles = { Admin, QualityManager, QualityControl };
    public static readonly string[] DeliveryRoles = { Admin, DeliveryManager, DeliveryStaff };
    public static readonly string[] DesignRoles = { Admin, DesignManager, Designer };
    public static readonly string[] MarketingRoles = {
        Admin, MarketingManager, ContentManager, ContentStaff, Designer, MediaMarketing, DigitalAds, Media
    };
    public static readonly string[] OperationalRoles = {
        Admin, ProductionManager, ProductionStaff,
        CuttingStaff, SewingStaff, PrintingStaff, FinishingStaff, PackagingStaff, WaybillStaff,
        QualityManager, QualityControl,
        DeliveryManager, DeliveryStaff
    };

    // ── Quản lý tài khoản theo phòng ban ──────────────────────────────
    // Mỗi trưởng phòng (manager) được tạo/quản lý các tài khoản nhân viên thuộc phòng mình.
    public static readonly string[] AllManagerRoles = {
        SalesManager, ProductionManager, DesignManager,
        DeliveryManager, QualityManager, ContentManager, MarketingManager
    };

    // manager role → các staff role được phép tạo/quản lý.
    public static readonly Dictionary<string, string[]> DepartmentStaff = new()
    {
        [SalesManager]      = new[] { SalesRep },
        [ProductionManager] = new[] { ProductionStaff, CuttingStaff, SewingStaff, PrintingStaff, FinishingStaff, PackagingStaff, WaybillStaff },
        [DesignManager]     = new[] { Designer },
        [DeliveryManager]   = new[] { DeliveryStaff },
        [QualityManager]    = new[] { QualityControl },
        [ContentManager]    = new[] { ContentStaff },
        [MarketingManager]  = new[] { MediaMarketing, DigitalAds, Media },
    };

    // Cho [Authorize(Roles = ...)] — Admin + tất cả trưởng phòng.
    public const string AdminAndManagers =
        Admin + "," + SalesManager + "," + ProductionManager + "," + DesignManager + "," +
        DeliveryManager + "," + QualityManager + "," + ContentManager + "," + MarketingManager;
}
