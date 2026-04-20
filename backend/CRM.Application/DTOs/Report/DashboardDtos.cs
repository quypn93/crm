namespace CRM.Application.DTOs.Report;

public class DashboardStatsDto
{
    public int TotalCustomers { get; set; }
    public int TotalOrders { get; set; }
    public int TotalTasks { get; set; }
    public decimal TotalRevenue { get; set; }
    public int CompletedOrdersCount { get; set; }
    public int CancelledOrdersCount { get; set; }
    public int PendingTasksCount { get; set; }
    public int OverdueTasksCount { get; set; }
    public decimal CompletionRate { get; set; }
    public decimal RevenueGrowth { get; set; }
    public int NewCustomersThisMonth { get; set; }
    public int OrdersInProgress { get; set; }
    public decimal InProgressOrdersValue { get; set; }
    public int NewOrdersThisMonth { get; set; }
}

public class RevenueReportDto
{
    public string Period { get; set; } = string.Empty;
    public decimal Revenue { get; set; }
    public int DealsCount { get; set; }
}

public class DealsByStageReportDto
{
    public string StageName { get; set; } = string.Empty;
    public string StageColor { get; set; } = string.Empty;
    public int Count { get; set; }
    public decimal TotalValue { get; set; }
}

public class CustomersByIndustryReportDto
{
    public string Industry { get; set; } = string.Empty;
    public int Count { get; set; }
    public decimal TotalRevenue { get; set; }
}

public class TaskCompletionReportDto
{
    public string Period { get; set; } = string.Empty;
    public int TotalTasks { get; set; }
    public int CompletedTasks { get; set; }
    public int OverdueTasks { get; set; }
    public decimal CompletionRate { get; set; }
}

public class SalesPerformanceDto
{
    public Guid UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public int DealsCount { get; set; }
    public int WonDealsCount { get; set; }
    public decimal TotalRevenue { get; set; }
    public decimal ConversionRate { get; set; }
    public int CustomersCount { get; set; }
    public int TasksCompleted { get; set; }
}

public class ActivityLogDto
{
    public Guid Id { get; set; }
    public string EntityType { get; set; } = string.Empty;
    public Guid EntityId { get; set; }
    public string Action { get; set; } = string.Empty;
    public string? Description { get; set; }
    public Guid UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}

public class ReportFilterDto
{
    public DateTime? DateFrom { get; set; }
    public DateTime? DateTo { get; set; }
    public string? Period { get; set; } = "monthly"; // daily, weekly, monthly, yearly
    public Guid? UserId { get; set; }
}

// Role-specific Dashboard DTOs

public class ProductionDashboardDto
{
    public int OrdersWaitingProduction { get; set; }      // Confirmed status
    public int OrdersInProduction { get; set; }           // InProduction status
    public int OrdersCompletedToday { get; set; }         // Moved to QualityCheck today
    public int TotalItemsInProduction { get; set; }
    public decimal AverageProductionDays { get; set; }    // Average days in production
    public IEnumerable<OrderStatusCountDto> StatusBreakdown { get; set; } = new List<OrderStatusCountDto>();
    public IEnumerable<RecentOrderDto> RecentOrders { get; set; } = new List<RecentOrderDto>();
}

public class QualityDashboardDto
{
    public int OrdersWaitingQC { get; set; }              // QualityCheck status
    public int OrdersPassedToday { get; set; }            // Moved to ReadyToShip today
    public int OrdersFailedToday { get; set; }            // Moved back to InProduction today (rework)
    public decimal PassRate { get; set; }                 // Pass percentage
    public IEnumerable<RecentOrderDto> PendingQCOrders { get; set; } = new List<RecentOrderDto>();
}

public class DeliveryDashboardDto
{
    public int OrdersReadyToShip { get; set; }            // ReadyToShip status
    public int OrdersShipping { get; set; }               // Shipping status
    public int OrdersDeliveredToday { get; set; }         // Delivered today
    public decimal TotalValueShipping { get; set; }
    public IEnumerable<RecentOrderDto> PendingDeliveryOrders { get; set; } = new List<RecentOrderDto>();
}

public class OrderStatusCountDto
{
    public int Status { get; set; }
    public string StatusName { get; set; } = string.Empty;
    public int Count { get; set; }
}

public class RecentOrderDto
{
    public Guid Id { get; set; }
    public string OrderNumber { get; set; } = string.Empty;
    public string CustomerName { get; set; } = string.Empty;
    public int Status { get; set; }
    public string StatusName { get; set; } = string.Empty;
    public DateTime OrderDate { get; set; }
    public DateTime? RequiredDate { get; set; }
    public decimal TotalAmount { get; set; }
    public int TotalItems { get; set; }
    public string? DeliveryAddress { get; set; }
    public string? DeliveryPhone { get; set; }
}
