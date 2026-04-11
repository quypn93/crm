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
        var totalDeals = await _unitOfWork.Deals.CountAsync();
        var totalTasks = await _unitOfWork.Tasks.CountAsync();

        // Get won stage deals
        var wonStage = await _unitOfWork.Deals.GetWonStageAsync();
        var lostStage = await _unitOfWork.Deals.GetLostStageAsync();

        var wonDealsCount = wonStage != null
            ? await _unitOfWork.Deals.CountAsync(d => d.StageId == wonStage.Id)
            : 0;

        var lostDealsCount = lostStage != null
            ? await _unitOfWork.Deals.CountAsync(d => d.StageId == lostStage.Id)
            : 0;

        var totalRevenue = await _unitOfWork.Deals.GetTotalRevenueAsync(null, null);

        var pendingTasksCount = await _unitOfWork.Tasks.GetPendingTasksCountAsync(userId);
        var overdueTasksCount = await _unitOfWork.Tasks.GetOverdueTasksCountAsync(userId);

        // New customers this month
        var newCustomersThisMonth = await _unitOfWork.Customers.CountAsync(
            c => c.CreatedAt >= startOfMonth && c.IsActive);

        // Deals in pipeline (not won or lost)
        var dealsInPipeline = await _unitOfWork.Deals.GetDealsInPipelineCountAsync();
        var pipelineValue = await _unitOfWork.Deals.GetPipelineValueAsync();

        // Conversion rate
        var totalClosedDeals = wonDealsCount + lostDealsCount;
        var conversionRate = totalClosedDeals > 0
            ? (decimal)wonDealsCount / totalClosedDeals * 100
            : 0;

        return new DashboardStatsDto
        {
            TotalCustomers = totalCustomers,
            TotalDeals = totalDeals,
            TotalTasks = totalTasks,
            TotalRevenue = totalRevenue,
            WonDealsCount = wonDealsCount,
            LostDealsCount = lostDealsCount,
            PendingTasksCount = pendingTasksCount,
            OverdueTasksCount = overdueTasksCount,
            ConversionRate = Math.Round(conversionRate, 2),
            NewCustomersThisMonth = newCustomersThisMonth,
            DealsInPipeline = dealsInPipeline,
            PipelineValue = pipelineValue
        };
    }

    public async Task<IEnumerable<RevenueReportDto>> GetRevenueReportAsync(ReportFilterDto filter)
    {
        var deals = await _unitOfWork.Deals.GetWonDealsAsync(filter.DateFrom, filter.DateTo);

        if (!deals.Any()) return Enumerable.Empty<RevenueReportDto>();

        var grouped = filter.Period?.ToLower() switch
        {
            "daily" => deals.GroupBy(d => d.ActualCloseDate!.Value.Date.ToString("yyyy-MM-dd")),
            "weekly" => deals.GroupBy(d => $"{d.ActualCloseDate!.Value.Year}-W{GetWeekOfYear(d.ActualCloseDate.Value)}"),
            "yearly" => deals.GroupBy(d => d.ActualCloseDate!.Value.Year.ToString()),
            _ => deals.GroupBy(d => d.ActualCloseDate!.Value.ToString("yyyy-MM")) // monthly default
        };

        return grouped.Select(g => new RevenueReportDto
        {
            Period = g.Key,
            Revenue = g.Sum(d => d.Value),
            DealsCount = g.Count()
        }).OrderBy(r => r.Period);
    }

    public async Task<IEnumerable<DealsByStageReportDto>> GetDealsByStageReportAsync()
    {
        var stagesWithDeals = await _unitOfWork.Deals.GetAllStagesWithDealsAsync();

        return stagesWithDeals.Select(s => new DealsByStageReportDto
        {
            StageName = s.Stage.Name,
            StageColor = s.Stage.Color ?? "#6366F1",
            Count = s.Deals.Count(),
            TotalValue = s.Deals.Sum(d => d.Value)
        });
    }

    public async Task<IEnumerable<CustomersByIndustryReportDto>> GetCustomersByIndustryReportAsync()
    {
        var customers = await _unitOfWork.Customers.GetAllWithDealsAsync();
        var wonStage = await _unitOfWork.Deals.GetWonStageAsync();

        return customers
            .GroupBy(c => c.Industry ?? CustomerIndustries.Other)
            .Select(g => new CustomersByIndustryReportDto
            {
                Industry = g.Key,
                Count = g.Count(),
                TotalRevenue = wonStage != null
                    ? g.SelectMany(c => c.Deals).Where(d => d.StageId == wonStage.Id).Sum(d => d.Value)
                    : 0
            })
            .OrderByDescending(r => r.Count);
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
