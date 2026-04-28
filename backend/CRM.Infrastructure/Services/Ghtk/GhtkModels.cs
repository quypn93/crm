using System.Text.Json.Serialization;

namespace CRM.Infrastructure.Services.Ghtk;

// Payload tạo đơn: POST /services/shipment/order
public class GhtkCreateOrderRequest
{
    [JsonPropertyName("products")]
    public List<GhtkProduct> Products { get; set; } = new();

    [JsonPropertyName("order")]
    public GhtkOrderPayload Order { get; set; } = new();
}

public class GhtkProduct
{
    [JsonPropertyName("name")]          public string Name { get; set; } = string.Empty;
    [JsonPropertyName("weight")]        public decimal Weight { get; set; }
    [JsonPropertyName("quantity")]      public int Quantity { get; set; } = 1;
    [JsonPropertyName("product_code")]  public string? ProductCode { get; set; }
}

public class GhtkOrderPayload
{
    [JsonPropertyName("id")]              public string Id { get; set; } = string.Empty;     // mã đơn bên mình
    [JsonPropertyName("pick_name")]       public string PickName { get; set; } = string.Empty;
    [JsonPropertyName("pick_money")]      public decimal PickMoney { get; set; }
    [JsonPropertyName("pick_address")]    public string PickAddress { get; set; } = string.Empty;
    [JsonPropertyName("pick_province")]   public string PickProvince { get; set; } = string.Empty;
    [JsonPropertyName("pick_district")]   public string PickDistrict { get; set; } = string.Empty;
    [JsonPropertyName("pick_ward")]       public string? PickWard { get; set; }
    [JsonPropertyName("pick_tel")]        public string PickTel { get; set; } = string.Empty;

    [JsonPropertyName("tel")]             public string Tel { get; set; } = string.Empty;
    [JsonPropertyName("name")]            public string Name { get; set; } = string.Empty;
    [JsonPropertyName("address")]         public string Address { get; set; } = string.Empty;
    [JsonPropertyName("province")]        public string Province { get; set; } = string.Empty;
    [JsonPropertyName("district")]        public string? District { get; set; }
    [JsonPropertyName("ward")]            public string? Ward { get; set; }

    [JsonPropertyName("is_freeship")]     public int IsFreeship { get; set; } = 1;   // 1 = shop trả phí, 0 = thu khách
    [JsonPropertyName("value")]           public decimal Value { get; set; }
    [JsonPropertyName("transport")]       public string Transport { get; set; } = "road";
    [JsonPropertyName("pick_work_shift")] public int PickWorkShift { get; set; } = 3;
    [JsonPropertyName("note")]            public string? Note { get; set; }
}

// Response schema GHTK
public class GhtkApiResponse<T>
{
    [JsonPropertyName("success")] public bool Success { get; set; }
    [JsonPropertyName("message")] public string? Message { get; set; }
    [JsonPropertyName("order")]   public T? Order { get; set; }
    [JsonPropertyName("fee")]     public T? Fee { get; set; }
    [JsonPropertyName("error_code")] public int? ErrorCode { get; set; }
}

public class GhtkOrderResponse
{
    [JsonPropertyName("label")]            public string? Label { get; set; }
    [JsonPropertyName("partner_id")]       public string? PartnerId { get; set; }
    [JsonPropertyName("status_id")]        public int? StatusId { get; set; }
    [JsonPropertyName("fee")]              public decimal? Fee { get; set; }
    [JsonPropertyName("insurance_fee")]    public decimal? InsuranceFee { get; set; }
    [JsonPropertyName("tracking_id")]      public long? TrackingId { get; set; }
    [JsonPropertyName("tracking_url")]     public string? TrackingUrl { get; set; }
    [JsonPropertyName("estimated_pick_time")] public string? EstimatedPickTime { get; set; }
    [JsonPropertyName("estimated_deliver_time")] public string? EstimatedDeliverTime { get; set; }
}

public class GhtkFeeResponse
{
    [JsonPropertyName("name")]          public string? Name { get; set; }
    [JsonPropertyName("fee")]           public decimal? Fee { get; set; }
    [JsonPropertyName("insurance_fee")] public decimal? InsuranceFee { get; set; }
    [JsonPropertyName("delivery_type")] public string? DeliveryType { get; set; }
}

public class GhtkStatusResponse
{
    [JsonPropertyName("label_id")]     public string? LabelId { get; set; }
    [JsonPropertyName("status")]       public string? Status { get; set; }
    [JsonPropertyName("status_text")]  public string? StatusText { get; set; }
    [JsonPropertyName("created")]      public string? Created { get; set; }
    [JsonPropertyName("modified")]     public string? Modified { get; set; }
    [JsonPropertyName("message")]      public string? Message { get; set; }
    [JsonPropertyName("pick_date")]    public string? PickDate { get; set; }
    [JsonPropertyName("deliver_date")] public string? DeliverDate { get; set; }
}

// Webhook payload (GHTK gửi về khi đơn thay đổi trạng thái)
public class GhtkWebhookPayload
{
    [JsonPropertyName("label_id")]      public string? LabelId { get; set; }
    [JsonPropertyName("partner_id")]    public string? PartnerId { get; set; }
    [JsonPropertyName("status_id")]     public int? StatusId { get; set; }
    [JsonPropertyName("action_time")]   public string? ActionTime { get; set; }
    [JsonPropertyName("reason_code")]   public string? ReasonCode { get; set; }
    [JsonPropertyName("reason")]        public string? Reason { get; set; }
    [JsonPropertyName("weight")]        public decimal? Weight { get; set; }
    [JsonPropertyName("fee")]           public decimal? Fee { get; set; }
}

// Input cho tính phí (dùng nội bộ)
public class GhtkFeeQuery
{
    public string PickProvince { get; set; } = string.Empty;
    public string PickDistrict { get; set; } = string.Empty;
    public string Province { get; set; } = string.Empty;
    public string? District { get; set; }
    public string? Ward { get; set; }
    public string? Address { get; set; }
    public decimal Weight { get; set; }
    public decimal Value { get; set; }
    public string Transport { get; set; } = "road";
}
