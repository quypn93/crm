namespace CRM.Core.Entities;

public class Material : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsActive { get; set; } = true;
}

public class ProductForm : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsActive { get; set; } = true;
}

public class ProductSpecification : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsActive { get; set; } = true;
}

public class OrderType : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsActive { get; set; } = true;
}

// Màu phối — chọn tự do, không phụ thuộc chất liệu (khác ColorFabric "ăn theo" chất liệu).
public class AccentColor : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsActive { get; set; } = true;
}

// Bo cổ (VD: X-00 .. X-15)
public class Collar : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsActive { get; set; } = true;
    // Số lượng màu bo cổ cần chọn khi lên đơn (1 = chỉ màu chính, 2 = +màu phối, 3 = +màu phối 1&2).
    // Quyết định số selectbox "Màu bo cổ" hiển thị ở form đơn hàng — xem QUY ĐỊNH CHỌN MÀU trong bảng bo cổ.
    public int ColorCount { get; set; } = 1;
}
