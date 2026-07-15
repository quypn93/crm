namespace CRM.Core.Entities;

// Địa chỉ gửi hàng (kho gửi) cho Viettel Post. Có thể có nhiều, 1 cái đặt mặc định.
public class SenderAddress : BaseEntity
{
    public string Name { get; set; } = string.Empty;       // Tên người/kho gửi
    public string Phone { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;    // Số nhà, đường...

    // ID danh mục hành chính theo Viettel Post.
    public int ProvinceId { get; set; }
    public int DistrictId { get; set; }
    public int WardId { get; set; }
    public string? ProvinceName { get; set; }
    public string? DistrictName { get; set; }
    public string? WardName { get; set; }

    public bool IsDefault { get; set; }
    public bool IsActive { get; set; } = true;

    // Tài khoản Quản lý kho (WarehouseManager) phụ trách kho này — để lọc "đơn của mình".
    public Guid? AssignedUserId { get; set; }
    public virtual User? AssignedUser { get; set; }
}
