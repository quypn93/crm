using CRM.Core.Enums;

namespace CRM.Application.DTOs.Finance;

/// <summary>1 dòng trong màn "Chi phí sản xuất hàng hóa" — đơn hàng kèm chi phí đã nhập (nếu có).</summary>
public class OrderCostListItemDto
{
    public Guid OrderId { get; set; }
    public string OrderNumber { get; set; } = string.Empty;
    public string? CustomerName { get; set; }
    public OrderStatus Status { get; set; }
    public string StatusName { get; set; } = string.Empty;
    public DateTime OrderDate { get; set; }
    public DateTime? ConfirmedDate { get; set; }
    public DateTime? CompletionDate { get; set; }
    public string? CreatedByUserName { get; set; }

    public decimal Revenue { get; set; }          // Order.TotalAmount
    public decimal PaidAmount { get; set; }

    /// <summary>Tổng SL sản phẩm hiện tại của đơn (cộng mọi dòng size) — dùng nhân với đơn giá.</summary>
    public int TotalQuantity { get; set; }

    // Chi phí — 0 khi chưa nhập
    public decimal UnitCost { get; set; }        // đơn giá 1 sản phẩm
    public decimal CostAmount { get; set; }      // = UnitCost × TotalQuantity
    public decimal GiftUnitCost { get; set; }    // đơn giá 1 phần quà
    public int GiftQuantity { get; set; }        // SL quà, khác SL áo
    public decimal GiftAmount { get; set; }      // = GiftUnitCost × GiftQuantity
    public decimal ShippingCost { get; set; }
    public decimal OutboundShippingCost { get; set; }
    public decimal OtherCost { get; set; }
    public decimal TotalCost { get; set; }

    /// <summary>Mã giao hàng: ưu tiên mã do hãng vận chuyển trả về, không có thì lấy mã nhập tay.</summary>
    public string? ShippingCode { get; set; }
    /// <summary>true = mã do API hãng vận chuyển sinh, UI để chỉ đọc.</summary>
    public bool ShippingCodeFromCarrier { get; set; }

    public decimal SettlementAmount { get; set; }   // số tiền thanh toán để đối soát

    public decimal Profit { get; set; }           // Revenue - TotalCost (chỉ có nghĩa khi HasCost)
    public decimal ProfitMargin { get; set; }     // %

    public bool HasCost { get; set; }             // đã nhập chi phí chưa
    public bool IsFinalized { get; set; }
    public string? Notes { get; set; }
    public string? CostFileUrl { get; set; }
    public string? CostFileName { get; set; }
    public string? EnteredByUserName { get; set; }
    public DateTime? EnteredAt { get; set; }
}

public class OrderCostFilterDto
{
    public DateTime? DateFrom { get; set; }
    public DateTime? DateTo { get; set; }
    public OrderStatus? Status { get; set; }
    /// <summary>true = chỉ đơn đã nhập cost, false = chỉ đơn chưa nhập, null = tất cả.</summary>
    public bool? HasCost { get; set; }
    public string? Search { get; set; }
    /// <summary>Mốc ngày dùng để lọc: order | confirmed | completed | delivered.</summary>
    public string? DateBasis { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 100;
}

/// <summary>Tổng hợp hiển thị trên đầu màn danh sách chi phí.</summary>
public class OrderCostSummaryDto
{
    public int TotalOrders { get; set; }
    public int OrdersWithCost { get; set; }
    public int OrdersWithoutCost { get; set; }
    public decimal TotalRevenue { get; set; }
    public decimal TotalCost { get; set; }
    public decimal TotalProfit { get; set; }
}

public class OrderCostListResultDto
{
    public List<OrderCostListItemDto> Items { get; set; } = new();
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalPages { get; set; }
    public OrderCostSummaryDto Summary { get; set; } = new();
}

public class UpsertOrderCostDto
{
    public decimal UnitCost { get; set; }
    public decimal GiftUnitCost { get; set; }
    public int GiftQuantity { get; set; }
    public decimal ShippingCost { get; set; }
    public decimal OutboundShippingCost { get; set; }
    public decimal OtherCost { get; set; }
    public string? ShippingCode { get; set; }
    public decimal SettlementAmount { get; set; }
    public string? Notes { get; set; }
    public bool IsFinalized { get; set; }
}

public class BulkOrderCostItemDto : UpsertOrderCostDto
{
    public Guid OrderId { get; set; }
}

public class BulkUpsertOrderCostDto
{
    public List<BulkOrderCostItemDto> Items { get; set; } = new();
}

public class CostImportRowErrorDto
{
    public int RowNumber { get; set; }
    public string? OrderNumber { get; set; }
    public string Error { get; set; } = string.Empty;
}

public class CostImportResultDto
{
    public int TotalRows { get; set; }
    public int SuccessCount { get; set; }
    public int SkippedCount { get; set; }
    public List<CostImportRowErrorDto> Errors { get; set; } = new();
}
