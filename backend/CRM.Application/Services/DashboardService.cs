using AutoMapper;
using CRM.Application.DTOs.Customer;
using CRM.Application.DTOs.Report;
using CRM.Application.Interfaces;
using CRM.Core.Enums;
using CRM.Core.Interfaces;
using TaskStatusEnum = CRM.Core.Enums.TaskStatus;

namespace CRM.Application.Services;

public class DashboardService : IDashboardService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public DashboardService(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<DashboardStatsDto> GetDashboardStatsAsync(Guid? userId = null)
    {
        var now = DateTime.UtcNow;
        var startOfMonth = new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc);

        var totalCustomers = await _unitOfWork.Customers.CountAsync(c => c.IsActive);
        var totalOrders = await _unitOfWork.Orders.CountAsync();
        var totalTasks = await _unitOfWork.Tasks.CountAsync();

        // Completed orders (revenue-recognized)
        var completedOrdersCount = await _unitOfWork.Orders.CountAsync(
            o => o.Status == OrderStatus.Delivered || o.Status == OrderStatus.Completed);

        var cancelledOrdersCount = await _unitOfWork.Orders.GetOrderCountByStatusAsync(OrderStatus.Cancelled);

        // Revenue from completed orders
        var totalRevenue = await _unitOfWork.Orders.GetTotalRevenueAsync(null, null);

        var pendingTasksCount = await _unitOfWork.Tasks.GetPendingTasksCountAsync(userId);
        var overdueTasksCount = await _unitOfWork.Tasks.GetOverdueTasksCountAsync(userId);

        // New customers this month
        var newCustomersThisMonth = await _unitOfWork.Customers.CountAsync(
            c => c.CreatedAt >= startOfMonth && c.IsActive);

        // New orders this month
        var newOrdersThisMonth = await _unitOfWork.Orders.CountAsync(
            o => o.OrderDate >= startOfMonth);

        // Orders in progress (confirmed → shipping)
        var inProgressStatuses = new[]
        {
            OrderStatus.Confirmed,
            OrderStatus.InProduction,
            OrderStatus.QualityCheck,
            OrderStatus.ReadyToShip,
            OrderStatus.Shipping
        };
        var ordersInProgress = await _unitOfWork.Orders.CountAsync(
            o => inProgressStatuses.Contains(o.Status));

        var ordersInProduction = await _unitOfWork.Orders.GetOrderCountByStatusAsync(OrderStatus.InProduction);

        decimal inProgressValue = 0;
        foreach (var status in inProgressStatuses)
        {
            inProgressValue += await _unitOfWork.Orders.GetTotalAmountByStatusAsync(status);
        }

        // Completion rate
        var totalClosedOrders = completedOrdersCount + cancelledOrdersCount;
        var completionRate = totalClosedOrders > 0
            ? (decimal)completedOrdersCount / totalClosedOrders * 100
            : 0;

        return new DashboardStatsDto
        {
            TotalCustomers = totalCustomers,
            TotalOrders = totalOrders,
            TotalTasks = totalTasks,
            TotalRevenue = totalRevenue,
            CompletedOrdersCount = completedOrdersCount,
            CancelledOrdersCount = cancelledOrdersCount,
            PendingTasksCount = pendingTasksCount,
            OverdueTasksCount = overdueTasksCount,
            CompletionRate = Math.Round(completionRate, 2),
            NewCustomersThisMonth = newCustomersThisMonth,
            OrdersInProgress = ordersInProgress,
            InProgressOrdersValue = inProgressValue,
            NewOrdersThisMonth = newOrdersThisMonth,
            OrdersInProduction = ordersInProduction
        };
    }

    public async Task<IEnumerable<RevenueReportDto>> GetRevenueReportAsync(ReportFilterDto filter)
    {
        var revenueStatuses = new[] { OrderStatus.Delivered, OrderStatus.Completed };
        var orders = await _unitOfWork.Orders.FindAsync(o =>
            revenueStatuses.Contains(o.Status) &&
            (!filter.DateFrom.HasValue || o.OrderDate >= filter.DateFrom.Value) &&
            (!filter.DateTo.HasValue || o.OrderDate <= filter.DateTo.Value));

        if (!orders.Any()) return Enumerable.Empty<RevenueReportDto>();

        var grouped = filter.Period?.ToLower() switch
        {
            "daily" => orders.GroupBy(o => new
            {
                Period = o.OrderDate.Date.ToString("yyyy-MM-dd"),
                Month = o.OrderDate.Date.ToString("yyyy-MM-dd"),
                o.OrderDate.Year
            }),
            "weekly" => orders.GroupBy(o => new
            {
                Period = $"{o.OrderDate.Year}-W{GetWeekOfYear(o.OrderDate)}",
                Month = $"W{GetWeekOfYear(o.OrderDate)}",
                o.OrderDate.Year
            }),
            "yearly" => orders.GroupBy(o => new
            {
                Period = o.OrderDate.Year.ToString(),
                Month = o.OrderDate.Year.ToString(),
                o.OrderDate.Year
            }),
            _ => orders.GroupBy(o => new
            {
                Period = o.OrderDate.ToString("yyyy-MM"),
                Month = o.OrderDate.Month.ToString("D2"),
                o.OrderDate.Year
            })
        };

        return grouped.Select(g => new RevenueReportDto
        {
            Period = g.Key.Period,
            Month = g.Key.Month,
            Year = g.Key.Year,
            Revenue = g.Sum(o => o.TotalAmount),
            DealsCount = g.Count(),
            DealCount = g.Count()
        }).OrderBy(r => r.Period);
    }

    public async Task<IEnumerable<DealsByStageReportDto>> GetDealsByStageReportAsync(ReportFilterDto filter)
    {
        var workflowStages = new[]
        {
            (Status: OrderStatus.Draft,        Name: "Nháp",                Color: "#94A3B8"),
            (Status: OrderStatus.Confirmed,    Name: "Đã xác nhận",         Color: "#6366F1"),
            (Status: OrderStatus.InProduction, Name: "Đang sản xuất",       Color: "#F59E0B"),
            (Status: OrderStatus.QualityCheck, Name: "Kiểm tra chất lượng", Color: "#8B5CF6"),
            (Status: OrderStatus.ReadyToShip,  Name: "Sẵn sàng giao",       Color: "#06B6D4"),
            (Status: OrderStatus.Shipping,     Name: "Đang giao hàng",      Color: "#0EA5E9"),
            (Status: OrderStatus.Delivered,    Name: "Đã giao",             Color: "#10B981"),
            (Status: OrderStatus.Completed,    Name: "Hoàn thành",          Color: "#22C55E"),
            (Status: OrderStatus.Cancelled,    Name: "Đã hủy",              Color: "#EF4444")
        };

        var rows = new List<DealsByStageReportDto>();
        foreach (var s in workflowStages)
        {
            // Lọc theo ngày tạo đơn (CreatedAt) — khớp filter ngày ở trang Đơn hàng
            var count = await _unitOfWork.Orders.CountAsync(o =>
                o.Status == s.Status &&
                (!filter.DateFrom.HasValue || o.CreatedAt >= filter.DateFrom.Value) &&
                (!filter.DateTo.HasValue || o.CreatedAt <= filter.DateTo.Value));
            var totalValue = await _unitOfWork.Orders.SumAsync(o =>
                o.Status == s.Status &&
                (!filter.DateFrom.HasValue || o.CreatedAt >= filter.DateFrom.Value) &&
                (!filter.DateTo.HasValue || o.CreatedAt <= filter.DateTo.Value),
                o => o.TotalAmount);
            rows.Add(new DealsByStageReportDto
            {
                StageName = s.Name,
                StageColor = s.Color,
                Count = count,
                TotalValue = totalValue
            });
        }

        var totalCount = rows.Sum(r => r.Count);
        if (totalCount > 0)
        {
            foreach (var r in rows)
            {
                r.Percentage = Math.Round((decimal)r.Count / totalCount * 100, 2);
            }
        }

        return rows;
    }

    public async Task<IEnumerable<CustomersByIndustryReportDto>> GetCustomersByIndustryReportAsync(ReportFilterDto filter)
    {
        var customers = await _unitOfWork.Customers.GetAllWithOrdersAsync();
        var revenueStatuses = new[] { OrderStatus.Delivered, OrderStatus.Completed };

        // Lọc khách hàng theo ngày tạo
        if (filter.DateFrom.HasValue)
            customers = customers.Where(c => c.CreatedAt >= filter.DateFrom.Value).ToList();
        if (filter.DateTo.HasValue)
            customers = customers.Where(c => c.CreatedAt <= filter.DateTo.Value).ToList();

        var grouped = customers
            .GroupBy(c => string.IsNullOrWhiteSpace(c.Industry) ? CustomerIndustries.Other : c.Industry!)
            .Select(g => new CustomersByIndustryReportDto
            {
                Industry = g.Key,
                Count = g.Count(),
                TotalRevenue = g.SelectMany(c => c.Orders)
                    .Where(o => revenueStatuses.Contains(o.Status))
                    .Sum(o => o.TotalAmount)
            })
            .OrderByDescending(r => r.Count)
            .ToList();

        var totalCount = grouped.Sum(r => r.Count);
        if (totalCount > 0)
        {
            foreach (var r in grouped)
            {
                r.Percentage = Math.Round((decimal)r.Count / totalCount * 100, 2);
            }
        }

        return grouped;
    }

    public async Task<IEnumerable<SalesPerformanceDto>> GetSalesPerformanceAsync(ReportFilterDto filter)
    {
        var users = await _unitOfWork.Users.GetUsersWithAssignmentsAsync();
        var wonStage = await _unitOfWork.Deals.GetWonStageAsync();

        return users.Select(u => new SalesPerformanceDto
        {
            UserId = u.Id,
            UserName = $"{u.FirstName} {u.LastName}",
            DealsCount = u.AssignedDeals.Count,
            WonDealsCount = wonStage != null
                ? u.AssignedDeals.Count(d => d.StageId == wonStage.Id)
                : 0,
            TotalRevenue = wonStage != null
                ? u.AssignedDeals.Where(d => d.StageId == wonStage.Id).Sum(d => d.Value)
                : 0,
            ConversionRate = u.AssignedDeals.Count > 0 && wonStage != null
                ? Math.Round((decimal)u.AssignedDeals.Count(d => d.StageId == wonStage.Id) / u.AssignedDeals.Count * 100, 2)
                : 0,
            CustomersCount = u.AssignedCustomers.Count,
            TasksCompleted = u.AssignedTasks.Count(t => t.Status == TaskStatusEnum.Completed)
        }).OrderByDescending(p => p.TotalRevenue);
    }

    public async Task<IEnumerable<ActivityLogDto>> GetRecentActivitiesAsync(int count = 10)
    {
        var activities = await _unitOfWork.ActivityLogs.GetRecentActivitiesAsync(count);
        return _mapper.Map<IEnumerable<ActivityLogDto>>(activities);
    }

    public async Task<ProductionDashboardDto> GetProductionDashboardAsync()
    {
        var confirmedCount = await _unitOfWork.Orders.GetOrderCountByStatusAsync(OrderStatus.Confirmed);
        var inProductionCount = await _unitOfWork.Orders.GetOrderCountByStatusAsync(OrderStatus.InProduction);

        // Orders that moved to QualityCheck today
        var completedToday = await _unitOfWork.Orders.GetOrdersCompletedTodayByStatusAsync(OrderStatus.QualityCheck);

        // Total items in production
        var productionStatuses = new[] { OrderStatus.Confirmed, OrderStatus.InProduction };
        var totalItems = await _unitOfWork.Orders.GetTotalItemsCountByStatusesAsync(productionStatuses);

        // Get recent orders
        var recentOrders = await _unitOfWork.Orders.GetOrdersByStatusesAsync(productionStatuses, 10);

        // Status breakdown
        var statusBreakdown = new List<OrderStatusCountDto>
        {
            new() { Status = (int)OrderStatus.Confirmed, StatusName = "Cho san xuat", Count = confirmedCount },
            new() { Status = (int)OrderStatus.InProduction, StatusName = "Dang san xuat", Count = inProductionCount }
        };

        return new ProductionDashboardDto
        {
            OrdersWaitingProduction = confirmedCount,
            OrdersInProduction = inProductionCount,
            OrdersCompletedToday = completedToday,
            TotalItemsInProduction = totalItems,
            AverageProductionDays = 0, // Would need historical tracking
            StatusBreakdown = statusBreakdown,
            RecentOrders = MapToRecentOrderDtos(recentOrders)
        };
    }

    public async Task<QualityDashboardDto> GetQualityDashboardAsync()
    {
        var qcCount = await _unitOfWork.Orders.GetOrderCountByStatusAsync(OrderStatus.QualityCheck);

        // Orders passed today (moved to ReadyToShip today)
        var passedToday = await _unitOfWork.Orders.GetOrdersCompletedTodayByStatusAsync(OrderStatus.ReadyToShip);

        // Orders failed today (moved back to InProduction) - approximation
        var failedToday = 0; // Would need activity log tracking for accurate count

        // Pass rate - need historical data for accurate calculation
        var passRate = passedToday + failedToday > 0
            ? Math.Round((decimal)passedToday / (passedToday + failedToday) * 100, 2)
            : 100m;

        // Get pending QC orders
        var pendingOrders = await _unitOfWork.Orders.GetOrdersByStatusAsync(OrderStatus.QualityCheck, 10);

        return new QualityDashboardDto
        {
            OrdersWaitingQC = qcCount,
            OrdersPassedToday = passedToday,
            OrdersFailedToday = failedToday,
            PassRate = passRate,
            PendingQCOrders = MapToRecentOrderDtos(pendingOrders)
        };
    }

    public async Task<DeliveryDashboardDto> GetDeliveryDashboardAsync()
    {
        var readyToShipCount = await _unitOfWork.Orders.GetOrderCountByStatusAsync(OrderStatus.ReadyToShip);
        var shippingCount = await _unitOfWork.Orders.GetOrderCountByStatusAsync(OrderStatus.Shipping);

        // Orders delivered today
        var deliveredToday = await _unitOfWork.Orders.GetOrdersDeliveredTodayAsync();

        // Total value of orders being shipped
        var totalValueShipping = await _unitOfWork.Orders.GetTotalAmountByStatusAsync(OrderStatus.Shipping);

        // Get pending delivery orders
        var deliveryStatuses = new[] { OrderStatus.ReadyToShip, OrderStatus.Shipping };
        var pendingOrders = await _unitOfWork.Orders.GetOrdersByStatusesAsync(deliveryStatuses, 10);

        return new DeliveryDashboardDto
        {
            OrdersReadyToShip = readyToShipCount,
            OrdersShipping = shippingCount,
            OrdersDeliveredToday = deliveredToday,
            TotalValueShipping = totalValueShipping,
            PendingDeliveryOrders = MapToRecentOrderDtos(pendingOrders)
        };
    }

    private static IEnumerable<RecentOrderDto> MapToRecentOrderDtos(IEnumerable<CRM.Core.Entities.Order> orders)
    {
        return orders.Select(o => new RecentOrderDto
        {
            Id = o.Id,
            OrderNumber = o.OrderNumber,
            CustomerName = o.Customer?.Name ?? "N/A",
            Status = (int)o.Status,
            StatusName = GetStatusName(o.Status),
            OrderDate = o.OrderDate,
            RequiredDate = o.ExpectedDeliveryDate,
            TotalAmount = o.TotalAmount,
            TotalItems = o.Items.Sum(i => i.Quantity),
            DeliveryAddress = o.ShippingAddress,
            DeliveryPhone = o.ShippingPhone
        });
    }

    private static string GetStatusName(OrderStatus status)
    {
        return status switch
        {
            OrderStatus.Draft => "Nhap",
            OrderStatus.Confirmed => "Da xac nhan",
            OrderStatus.InProduction => "Dang san xuat",
            OrderStatus.QualityCheck => "Kiem tra chat luong",
            OrderStatus.ReadyToShip => "San sang giao",
            OrderStatus.Shipping => "Dang giao",
            OrderStatus.Delivered => "Da giao",
            OrderStatus.Completed => "Hoan thanh",
            OrderStatus.Cancelled => "Da huy",
            _ => status.ToString()
        };
    }

    private static int GetWeekOfYear(DateTime date)
    {
        var cal = System.Globalization.CultureInfo.CurrentCulture.Calendar;
        return cal.GetWeekOfYear(date, System.Globalization.CalendarWeekRule.FirstDay, DayOfWeek.Monday);
    }
}
