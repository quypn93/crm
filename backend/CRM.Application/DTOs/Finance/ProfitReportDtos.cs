namespace CRM.Application.DTOs.Finance;

public class ProfitFilterDto
{
    public DateTime? DateFrom { get; set; }
    public DateTime? DateTo { get; set; }
    public int? Year { get; set; }
    /// <summary>Mốc ghi nhận doanh thu: order | confirmed | completed | delivered. Mặc định confirmed.</summary>
    public string? RevenueBasis { get; set; }
    public string? Search { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 100;
}

/// <summary>Lãi/lỗ của 1 đơn hàng.</summary>
public class OrderProfitDto
{
    public Guid OrderId { get; set; }
    public string OrderNumber { get; set; } = string.Empty;
    public string? CustomerName { get; set; }
    public DateTime RevenueDate { get; set; }        // mốc ngày dùng để quy về tháng
    public string StatusName { get; set; } = string.Empty;

    public decimal Revenue { get; set; }
    public decimal CostAmount { get; set; }
    public decimal ShippingCost { get; set; }
    public decimal OutboundShippingCost { get; set; }
    public decimal OtherCost { get; set; }
    public decimal TotalCost { get; set; }
    public decimal Profit { get; set; }
    public decimal ProfitMargin { get; set; }
    public bool HasCost { get; set; }
}

public class OrderProfitResultDto
{
    public List<OrderProfitDto> Items { get; set; } = new();
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalPages { get; set; }

    // Tổng của TOÀN BỘ kết quả lọc (không chỉ trang hiện tại)
    public decimal TotalRevenue { get; set; }
    public decimal TotalCost { get; set; }
    public decimal TotalProfit { get; set; }
    public decimal AverageMargin { get; set; }
    public int OrdersWithoutCost { get; set; }
    /// <summary>Doanh thu của các đơn CHƯA nhập cost — tách riêng để không tạo lãi ảo.</summary>
    public decimal RevenueWithoutCost { get; set; }
}

/// <summary>Lãi/lỗ tổng theo 1 tháng.</summary>
public class MonthlyProfitDto
{
    public int Year { get; set; }
    public int Month { get; set; }
    public string Label { get; set; } = string.Empty;    // "01/2026"

    public int OrderCount { get; set; }
    public int OrdersWithoutCost { get; set; }

    public decimal Revenue { get; set; }
    public decimal Cogs { get; set; }                    // giá vốn = tổng chi phí đơn hàng
    public decimal GrossProfit { get; set; }             // Revenue - Cogs
    public decimal PayrollCost { get; set; }             // chi phí nhân sự
    public decimal FixedCost { get; set; }               // chi phí cố định
    public decimal NetProfit { get; set; }               // GrossProfit - Payroll - Fixed
    public decimal NetMargin { get; set; }               // %
}

public class MonthlyProfitResultDto
{
    public int Year { get; set; }
    public string RevenueBasis { get; set; } = string.Empty;
    public List<MonthlyProfitDto> Months { get; set; } = new();
    public MonthlyProfitDto Total { get; set; } = new();
}

/// <summary>Bóc tách chi tiết 1 tháng khi người dùng bấm vào dòng tháng.</summary>
public class MonthlyProfitDetailDto
{
    public MonthlyProfitDto Summary { get; set; } = new();
    public List<ExpenseCategoryTotalDto> FixedByCategory { get; set; } = new();
    public List<PayrollEntryDto> PayrollEntries { get; set; } = new();
    public List<OrderProfitDto> Orders { get; set; } = new();
}
