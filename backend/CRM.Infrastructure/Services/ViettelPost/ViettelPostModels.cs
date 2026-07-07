using System.Text.Json.Serialization;

namespace CRM.Infrastructure.Services.ViettelPost;

// Wrapper phản hồi chung của Viettel Post: { status, error, message, data }
public class VtpApiResponse<T>
{
    [JsonPropertyName("status")] public int Status { get; set; }
    [JsonPropertyName("error")] public bool Error { get; set; }
    [JsonPropertyName("message")] public string? Message { get; set; }
    [JsonPropertyName("data")] public T? Data { get; set; }
}

public class VtpLoginRequest
{
    [JsonPropertyName("USERNAME")] public string Username { get; set; } = string.Empty;
    [JsonPropertyName("PASSWORD")] public string Password { get; set; } = string.Empty;
}

public class VtpLoginData
{
    [JsonPropertyName("token")] public string? Token { get; set; }
    [JsonPropertyName("userId")] public long? UserId { get; set; }
}

public class VtpCreateOrderRequest
{
    [JsonPropertyName("ORDER_NUMBER")] public string OrderNumber { get; set; } = string.Empty;
    [JsonPropertyName("GROUPADDRESS_ID")] public int? GroupAddressId { get; set; }
    [JsonPropertyName("CUS_ID")] public int? CusId { get; set; }
    [JsonPropertyName("DELIVERY_DATE")] public string? DeliveryDate { get; set; }

    [JsonPropertyName("SENDER_FULLNAME")] public string? SenderFullname { get; set; }
    [JsonPropertyName("SENDER_ADDRESS")] public string? SenderAddress { get; set; }
    [JsonPropertyName("SENDER_PHONE")] public string? SenderPhone { get; set; }
    [JsonPropertyName("SENDER_PROVINCE")] public int SenderProvince { get; set; }
    [JsonPropertyName("SENDER_DISTRICT")] public int SenderDistrict { get; set; }
    [JsonPropertyName("SENDER_WARD")] public int SenderWard { get; set; }

    [JsonPropertyName("RECEIVER_FULLNAME")] public string? ReceiverFullname { get; set; }
    [JsonPropertyName("RECEIVER_ADDRESS")] public string? ReceiverAddress { get; set; }
    [JsonPropertyName("RECEIVER_PHONE")] public string? ReceiverPhone { get; set; }
    [JsonPropertyName("RECEIVER_PROVINCE")] public int ReceiverProvince { get; set; }
    [JsonPropertyName("RECEIVER_DISTRICT")] public int ReceiverDistrict { get; set; }
    [JsonPropertyName("RECEIVER_WARD")] public int ReceiverWard { get; set; }

    [JsonPropertyName("PRODUCT_NAME")] public string? ProductName { get; set; }
    [JsonPropertyName("PRODUCT_DESCRIPTION")] public string? ProductDescription { get; set; }
    [JsonPropertyName("PRODUCT_QUANTITY")] public int ProductQuantity { get; set; } = 1;
    [JsonPropertyName("PRODUCT_PRICE")] public decimal ProductPrice { get; set; }
    [JsonPropertyName("PRODUCT_WEIGHT")] public int ProductWeight { get; set; }        // gram
    [JsonPropertyName("PRODUCT_TYPE")] public string ProductType { get; set; } = "HH";

    [JsonPropertyName("ORDER_PAYMENT")] public int OrderPayment { get; set; } = 3;
    [JsonPropertyName("ORDER_SERVICE")] public string OrderService { get; set; } = "VCN";
    [JsonPropertyName("ORDER_SERVICE_ADD")] public string? OrderServiceAdd { get; set; }
    [JsonPropertyName("ORDER_NOTE")] public string? OrderNote { get; set; }
    [JsonPropertyName("MONEY_COLLECTION")] public decimal MoneyCollection { get; set; }
    [JsonPropertyName("NATIONAL_TYPE")] public int NationalType { get; set; } = 1;

    [JsonPropertyName("LIST_ITEM")] public List<VtpItem> ListItem { get; set; } = new();
}

public class VtpItem
{
    [JsonPropertyName("PRODUCT_NAME")] public string? ProductName { get; set; }
    [JsonPropertyName("PRODUCT_QUANTITY")] public int ProductQuantity { get; set; } = 1;
    [JsonPropertyName("PRODUCT_PRICE")] public decimal ProductPrice { get; set; }
    [JsonPropertyName("PRODUCT_WEIGHT")] public int ProductWeight { get; set; }
}

public class VtpCreateOrderData
{
    [JsonPropertyName("ORDER_NUMBER")] public string? OrderNumber { get; set; }
    [JsonPropertyName("MONEY_TOTAL")] public decimal? MoneyTotal { get; set; }
    [JsonPropertyName("MONEY_TOTAL_FEE")] public decimal? MoneyTotalFee { get; set; }
    [JsonPropertyName("MONEY_FEE")] public decimal? MoneyFee { get; set; }
    [JsonPropertyName("MONEY_COLLECTION_FEE")] public decimal? MoneyCollectionFee { get; set; }
    [JsonPropertyName("MONEY_VAS")] public decimal? MoneyVas { get; set; }
    [JsonPropertyName("EXCHANGE_WEIGHT")] public decimal? ExchangeWeight { get; set; }
}

public class VtpPriceRequest
{
    [JsonPropertyName("SENDER_PROVINCE")] public int SenderProvince { get; set; }
    [JsonPropertyName("SENDER_DISTRICT")] public int SenderDistrict { get; set; }
    [JsonPropertyName("RECEIVER_PROVINCE")] public int ReceiverProvince { get; set; }
    [JsonPropertyName("RECEIVER_DISTRICT")] public int ReceiverDistrict { get; set; }
    [JsonPropertyName("PRODUCT_TYPE")] public string ProductType { get; set; } = "HH";
    [JsonPropertyName("PRODUCT_WEIGHT")] public int ProductWeight { get; set; }
    [JsonPropertyName("PRODUCT_PRICE")] public decimal ProductPrice { get; set; }
    [JsonPropertyName("MONEY_COLLECTION")] public decimal MoneyCollection { get; set; }
    [JsonPropertyName("ORDER_SERVICE")] public string? OrderService { get; set; }
    [JsonPropertyName("TYPE")] public int Type { get; set; } = 1;
}

public class VtpPriceData
{
    [JsonPropertyName("MONEY_TOTAL")] public decimal? MoneyTotal { get; set; }
    [JsonPropertyName("MONEY_FEE")] public decimal? MoneyFee { get; set; }
    [JsonPropertyName("MONEY_TOTAL_FEE")] public decimal? MoneyTotalFee { get; set; }
    [JsonPropertyName("MONEY_COLLECTION_FEE")] public decimal? MoneyCollectionFee { get; set; }
    [JsonPropertyName("MONEY_FEECOD")] public decimal? MoneyFeeCod { get; set; }
    [JsonPropertyName("ORDER_SERVICE")] public string? OrderService { get; set; }
}

public class VtpCancelRequest
{
    [JsonPropertyName("TYPE")] public int Type { get; set; } = 4;   // 4 = Hủy đơn
    [JsonPropertyName("ORDER_NUMBER")] public string OrderNumber { get; set; } = string.Empty;
    [JsonPropertyName("NOTE")] public string? Note { get; set; }
}

public class VtpOrderStatusData
{
    [JsonPropertyName("ORDER_NUMBER")] public string? OrderNumber { get; set; }
    [JsonPropertyName("ORDER_STATUS")] public int? OrderStatus { get; set; }
    [JsonPropertyName("STATUS_NAME")] public string? StatusName { get; set; }
    [JsonPropertyName("ORDER_STATUSDATE")] public string? OrderStatusDate { get; set; }
    [JsonPropertyName("MONEY_TOTAL")] public decimal? MoneyTotal { get; set; }
}

// Payload webhook Viettel Post đẩy về khi trạng thái đơn thay đổi.
public class VtpWebhookPayload
{
    [JsonPropertyName("ORDER_NUMBER")] public string? OrderNumber { get; set; }
    [JsonPropertyName("ORDER_STATUS")] public int? OrderStatus { get; set; }
    [JsonPropertyName("STATUS_NAME")] public string? StatusName { get; set; }
    [JsonPropertyName("ORDER_STATUSDATE")] public string? OrderStatusDate { get; set; }
    [JsonPropertyName("MONEY_TOTAL")] public decimal? MoneyTotal { get; set; }
    [JsonPropertyName("NOTE")] public string? Note { get; set; }
}

// Danh mục hành chính Viettel Post (dùng chung cho listProvince/listDistrict/listWards).
public class VtpCategory
{
    [JsonPropertyName("PROVINCE_ID")] public int? ProvinceId { get; set; }
    [JsonPropertyName("DISTRICT_ID")] public int? DistrictId { get; set; }
    [JsonPropertyName("WARDS_ID")] public int? WardsId { get; set; }
    [JsonPropertyName("PROVINCE_NAME")] public string? ProvinceName { get; set; }
    [JsonPropertyName("DISTRICT_NAME")] public string? DistrictName { get; set; }
    [JsonPropertyName("WARDS_NAME")] public string? WardsName { get; set; }
    [JsonPropertyName("PROVINCE_CODE")] public string? ProvinceCode { get; set; }
}

// Query nội bộ để tính phí (map sang VtpPriceRequest trong client).
public class VtpFeeQuery
{
    public int SenderProvince { get; set; }
    public int SenderDistrict { get; set; }
    public int ReceiverProvince { get; set; }
    public int ReceiverDistrict { get; set; }
    public int WeightGram { get; set; }
    public decimal Value { get; set; }
    public decimal MoneyCollection { get; set; }
    public string? OrderService { get; set; }
}
