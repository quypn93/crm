using CRM.Core.Enums;

namespace CRM.Application.DTOs.Order;

public class OrderDto
{
    public Guid Id { get; set; }
    public string OrderNumber { get; set; } = string.Empty;
    public Guid? CustomerId { get; set; }
    public string? CustomerName { get; set; }
    public Guid? DealId { get; set; }
    public string? DealTitle { get; set; }

    public OrderStatus Status { get; set; }
    public string StatusName => GetStatusName(Status);
    public decimal SubTotal { get; set; }
    public decimal DiscountPercent { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal TaxPercent { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal TotalAmount { get; set; }
    public string Currency { get; set; } = "VND";

    public DateTime OrderDate { get; set; }
    public DateTime? ExpectedDeliveryDate { get; set; }
    public DateTime? CompletionDate { get; set; }
    public DateTime? ReturnDate { get; set; }
    public DateTime? ActualDeliveryDate { get; set; }

    // Production days option
    public Guid? ProductionDaysOptionId { get; set; }
    public int? ProductionDays { get; set; }
    public string? ProductionDaysOptionName { get; set; }

    // Deposit
    public string? DepositCode { get; set; }

    // Designer upload
    public string? DesignImageUrl { get; set; }

    public string? ShippingAddress { get; set; }
    public string? ShippingCity { get; set; }
    public string? ShippingPhone { get; set; }
    public string? ShippingNotes { get; set; }

    public PaymentStatus PaymentStatus { get; set; }
    public string PaymentStatusName => GetPaymentStatusName(PaymentStatus);
    public string? PaymentMethod { get; set; }
    public decimal PaidAmount { get; set; }
    public DateTime? PaymentDate { get; set; }

    public string? Notes { get; set; }
    public string? InternalNotes { get; set; }
    public string? StyleNotes { get; set; }

    public Guid CreatedByUserId { get; set; }
    public string? CreatedByUserName { get; set; }
    public Guid? AssignedToUserId { get; set; }
    public string? AssignedToUserName { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    public List<OrderItemDto> Items { get; set; } = new();
    public int ItemsCount { get; set; }

    // Production / QR
    public string? QrCodeToken { get; set; }
    public string? QrCodeImageBase64 { get; set; }
    public Guid? DesignerUserId { get; set; }
    public string? DesignerUserName { get; set; }

    private static string GetStatusName(OrderStatus status) => status switch
    {
        OrderStatus.Draft => "Nháp",
        OrderStatus.Confirmed => "Đã xác nhận",
        OrderStatus.InProduction => "Đang sản xuất",
        OrderStatus.QualityCheck => "Kiểm tra chất lượng",
        OrderStatus.ReadyToShip => "Sẵn sàng giao",
        OrderStatus.Shipping => "Đang giao hàng",
        OrderStatus.Delivered => "Đã giao",
        OrderStatus.Completed => "Hoàn thành",
        OrderStatus.Cancelled => "Đã hủy",
        _ => "Không xác định"
    };

    private static string GetPaymentStatusName(PaymentStatus status) => status switch
    {
        PaymentStatus.Pending => "Chờ thanh toán",
        PaymentStatus.PartialPaid => "Thanh toán một phần",
        PaymentStatus.Paid => "Đã thanh toán",
        PaymentStatus.Refunded => "Đã hoàn tiền",
        PaymentStatus.Cancelled => "Đã hủy",
        _ => "Không xác định"
    };
}

public class OrderItemDto
{
    public Guid Id { get; set; }
    public Guid OrderId { get; set; }
    public Guid? CollectionId { get; set; }
    public string? CollectionName { get; set; }
    public string? ProductCode { get; set; }
    public string? Description { get; set; }
    public string? Size { get; set; }
    public Guid? MainColorId { get; set; }
    public Guid? AccentColorId { get; set; }
    public Guid? MaterialId { get; set; }
    public Guid? FormId { get; set; }
    public Guid? SpecificationId { get; set; }
    public string? MainColorName { get; set; }
    public string? AccentColorName { get; set; }
    public string? MaterialName { get; set; }
    public string? FormName { get; set; }
    public string? SpecificationName { get; set; }
    public int Quantity { get; set; }
    public string Unit { get; set; } = "cái";
    public decimal UnitPrice { get; set; }
    public decimal DiscountPercent { get; set; }
    public decimal DiscountAmount { get; set; }
    public decimal LineTotal { get; set; }
    public string? Notes { get; set; }
}

public class CreateOrderDto
{
    public Guid? CustomerId { get; set; }
    public string? CustomerName { get; set; }
    public Guid? DealId { get; set; }
    public DateTime? ExpectedDeliveryDate { get; set; }
    public DateTime? CompletionDate { get; set; }
    public DateTime? ReturnDate { get; set; }
    public Guid? ProductionDaysOptionId { get; set; }
    public string? DepositCode { get; set; }
    public string? ShippingAddress { get; set; }
    public string? ShippingCity { get; set; }
    public string? ShippingPhone { get; set; }
    public string? ShippingNotes { get; set; }
    public decimal DiscountPercent { get; set; }
    public decimal TaxPercent { get; set; } = 10;
    public string? Notes { get; set; }
    public string? InternalNotes { get; set; }
    public string? StyleNotes { get; set; }
    public Guid? AssignedToUserId { get; set; }
    public Guid? DesignerUserId { get; set; }
    public List<CreateOrderItemDto> Items { get; set; } = new();
}

public class CreateOrderItemDto
{
    public Guid? CollectionId { get; set; }
    public string? ProductCode { get; set; }
    public string? Description { get; set; }
    public string? Size { get; set; }
    public Guid? MainColorId { get; set; }
    public Guid? AccentColorId { get; set; }
    public Guid? MaterialId { get; set; }
    public Guid? FormId { get; set; }
    public Guid? SpecificationId { get; set; }
    public int Quantity { get; set; }
    public string Unit { get; set; } = "cái";
    public decimal UnitPrice { get; set; }
    public decimal DiscountPercent { get; set; }
    public string? Notes { get; set; }
}

public class UpdateOrderDto : CreateOrderDto
{
    public Guid Id { get; set; }
}

public class UpdateOrderStatusDto
{
    public OrderStatus Status { get; set; }
    public string? Notes { get; set; }
}

public class UpdatePaymentDto
{
    public PaymentStatus PaymentStatus { get; set; }
    public string? PaymentMethod { get; set; }
    public decimal PaidAmount { get; set; }
    public DateTime? PaymentDate { get; set; }
}

public class OrderFilterDto
{
    public string? Search { get; set; }
    public Guid? CustomerId { get; set; }
    public Guid? DealId { get; set; }
    public Guid? AssignedTo { get; set; }
    public Guid? CreatedBy { get; set; }
    public Guid? DesignerUserId { get; set; }
    public OrderStatus? Status { get; set; }
    public PaymentStatus? PaymentStatus { get; set; }
    public DateTime? OrderDateFrom { get; set; }
    public DateTime? OrderDateTo { get; set; }
    public decimal? MinAmount { get; set; }
    public decimal? MaxAmount { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
    public string? SortBy { get; set; }
    public string SortOrder { get; set; } = "desc";
}

public class OrderSummaryDto
{
    public int TotalOrders { get; set; }
    public int DraftOrders { get; set; }
    public int InProgressOrders { get; set; }
    public int CompletedOrders { get; set; }
    public decimal TotalRevenue { get; set; }
    public decimal PendingPayment { get; set; }
}
