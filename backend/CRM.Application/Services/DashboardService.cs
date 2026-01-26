using AutoMapper;
using CRM.Application.DTOs.Customer;
using CRM.Application.DTOs.Report;
using CRM.Core.Enums;
using CRM.Core.Interfaces;
using CRM.Core.Interfaces.Services;
using CRM.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace CRM.Application.Services;

public class DashboardService : IDashboardService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly CrmDbContext _context;

    public DashboardService(IUnitOfWork unitOfWork, IMapper mapper, CrmDbContext context)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _context = context;
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
        var dealsInPipeline = await _context.Deals
            .Where(d => !d.Stage.IsWonStage && !d.Stage.IsLostStage)
            .CountAsync();

        var pipelineValue = await _context.Deals
            .Where(d => !d.Stage.IsWonStage && !d.Stage.IsLostStage)
            .SumAsync(d => d.Value);

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
        var wonStage = await _unitOfWork.Deals.GetWonStageAsync();
        if (wonStage == null) return Enumerable.Empty<RevenueReportDto>();

        var query = _context.Deals
            .Where(d => d.StageId == wonStage.Id && d.ActualCloseDate.HasValue);

        if (filter.DateFrom.HasValue)
            query = query.Where(d => d.ActualCloseDate >= filter.DateFrom.Value);

        if (filter.DateTo.HasValue)
            query = query.Where(d => d.ActualCloseDate <= filter.DateTo.Value);

        var deals = await query.ToListAsync();

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
        var stages = await _context.DealStages
            .Include(s => s.Deals)
            .OrderBy(s => s.Order)
            .ToListAsync();

        return stages.Select(s => new DealsByStageReportDto
        {
            StageName = s.Name,
            StageColor = s.Color ?? "#6366F1",
            Count = s.Deals.Count,
            TotalValue = s.Deals.Sum(d => d.Value)
        });
    }

    public async Task<IEnumerable<CustomersByIndustryReportDto>> GetCustomersByIndustryReportAsync()
    {
        var customers = await _context.Customers
            .Include(c => c.Deals)
            .Where(c => c.IsActive && !string.IsNullOrEmpty(c.Industry))
            .ToListAsync();

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
        var users = await _context.Users
            .Include(u => u.AssignedDeals)
                .ThenInclude(d => d.Stage)
            .Include(u => u.AssignedCustomers)
            .Include(u => u.AssignedTasks)
            .Where(u => u.IsActive)
            .ToListAsync();

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
            TasksCompleted = u.AssignedTasks.Count(t => t.Status == TaskStatus.Completed)
        }).OrderByDescending(p => p.TotalRevenue);
    }

    public async Task<IEnumerable<ActivityLogDto>> GetRecentActivitiesAsync(int count = 10)
    {
        var activities = await _context.ActivityLogs
            .Include(a => a.User)
            .OrderByDescending(a => a.CreatedAt)
            .Take(count)
            .ToListAsync();

        return _mapper.Map<IEnumerable<ActivityLogDto>>(activities);
    }

    private static int GetWeekOfYear(DateTime date)
    {
        var cal = System.Globalization.CultureInfo.CurrentCulture.Calendar;
        return cal.GetWeekOfYear(date, System.Globalization.CalendarWeekRule.FirstDay, DayOfWeek.Monday);
    }
}
