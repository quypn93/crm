namespace CRM.Core.Entities;

// Tỉnh / Thành phố — cấu trúc hành chính 2 cấp sau sáp nhập 2025 (34 đơn vị cấp tỉnh).
public class Province
{
    public string Code { get; set; } = string.Empty;   // Mã (2 ký tự, VD "01" = Hà Nội)
    public string Name { get; set; } = string.Empty;   // Tên thuần, VD "Hà Nội"
    public string FullName { get; set; } = string.Empty; // VD "Thành phố Hà Nội"
    public string Type { get; set; } = string.Empty;   // "Thành phố Trung ương" | "Tỉnh"
    public int SortOrder { get; set; }

    public virtual ICollection<Ward> Wards { get; set; } = new List<Ward>();
}
