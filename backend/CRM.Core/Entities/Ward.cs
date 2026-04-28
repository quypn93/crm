namespace CRM.Core.Entities;

// Xã / Phường / Đặc khu — cấp dưới Tỉnh. Sau 2025 bỏ cấp Huyện.
public class Ward
{
    public string Code { get; set; } = string.Empty;      // Mã xã (5 ký tự)
    public string Name { get; set; } = string.Empty;      // VD "Phường Ba Đình"
    public string FullName { get; set; } = string.Empty;  // VD "Phường Ba Đình, Thành phố Hà Nội"
    public string Type { get; set; } = string.Empty;      // "Phường" | "Xã" | "Đặc khu"
    public string ProvinceCode { get; set; } = string.Empty;

    public virtual Province Province { get; set; } = null!;
}
