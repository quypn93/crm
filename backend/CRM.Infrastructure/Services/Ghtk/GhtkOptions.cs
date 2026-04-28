namespace CRM.Infrastructure.Services.Ghtk;

// Binding từ appsettings: "Ghtk": { ... }
public class GhtkOptions
{
    public string BaseUrl { get; set; } = "https://services.giaohangtietkiem.vn";
    public string Token { get; set; } = string.Empty;
    public string? PartnerCode { get; set; }
    public string? WebhookSecret { get; set; }

    public GhtkPickOptions Pick { get; set; } = new();
    public GhtkDefaults Defaults { get; set; } = new();

    public bool IsConfigured => !string.IsNullOrWhiteSpace(Token)
        && !string.IsNullOrWhiteSpace(Pick.Address)
        && !string.IsNullOrWhiteSpace(Pick.Province);
}

public class GhtkPickOptions
{
    public string Name { get; set; } = string.Empty;
    public string Tel { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string Province { get; set; } = string.Empty;
    public string District { get; set; } = string.Empty;    // GHTK v1 vẫn yêu cầu district cũ
    public string Ward { get; set; } = string.Empty;
}

public class GhtkDefaults
{
    public string Transport { get; set; } = "road";    // "road" | "fly"
    public int PickWorkShift { get; set; } = 3;        // 1=sáng, 2=chiều, 3=cả ngày
    public decimal DefaultWeightKg { get; set; } = 0.3m;
    public bool UseCod { get; set; } = false;
    public string AutoCreateOnStatus { get; set; } = "ReadyToShip";  // hoặc "None" để tắt auto
    public bool AutoOverrideShippingFee { get; set; } = false;
}
