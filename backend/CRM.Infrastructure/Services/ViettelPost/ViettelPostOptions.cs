namespace CRM.Infrastructure.Services.ViettelPost;

// Binding từ appsettings: "ViettelPost": { ... }
public class ViettelPostOptions
{
    public string BaseUrl { get; set; } = "https://partner.viettelpost.vn/v2";

    // Token tĩnh lấy ở cổng đối tác ViettelPost. Nếu để trống sẽ thử Login bằng Username/Password.
    public string Token { get; set; } = string.Empty;
    public string? Username { get; set; }
    public string? Password { get; set; }
    public string? PartnerCode { get; set; }
    public string? WebhookSecret { get; set; }

    public ViettelPostPickOptions Pick { get; set; } = new();
    public ViettelPostDefaults Defaults { get; set; } = new();

    // Có token + kho gửi tối thiểu (địa chỉ + ID tỉnh/huyện) mới coi là đã cấu hình.
    public bool IsConfigured =>
        (!string.IsNullOrWhiteSpace(Token) || (!string.IsNullOrWhiteSpace(Username) && !string.IsNullOrWhiteSpace(Password)))
        && !string.IsNullOrWhiteSpace(Pick.Address)
        && Pick.ProvinceId > 0
        && Pick.DistrictId > 0;
}

public class ViettelPostPickOptions
{
    public string Name { get; set; } = string.Empty;
    public string Tel { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;

    // ID danh mục hành chính theo ViettelPost (khác mã hành chính của CRM).
    public int ProvinceId { get; set; }
    public int DistrictId { get; set; }
    public int WardId { get; set; }
    public string? ProvinceName { get; set; }
    public string? DistrictName { get; set; }
    public string? WardName { get; set; }

    // Kho gửi đã đăng ký trên cổng ViettelPost (nếu dùng).
    public int? GroupAddressId { get; set; }
    public int? CusId { get; set; }
}

public class ViettelPostDefaults
{
    public string OrderService { get; set; } = "VCN";     // Dịch vụ: VCN (chuyển phát nhanh), VCBI, SCOD...
    public string? OrderServiceAdd { get; set; }
    public int OrderPayment { get; set; } = 3;            // 1=gửi trả trước, 2=gửi trả sau, 3=nhận trả, 4=...
    public string ProductType { get; set; } = "HH";       // HH=hàng hóa, TH=thư
    public int NationalType { get; set; } = 1;            // 1 = nội địa
    public int DefaultWeightGram { get; set; } = 300;     // ViettelPost tính khối lượng theo gram
    public bool UseCod { get; set; } = false;
    public string AutoCreateOnStatus { get; set; } = "ReadyToShip";  // hoặc "None" để tắt auto
    public bool AutoOverrideShippingFee { get; set; } = false;
}
