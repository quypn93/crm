using CRM.Application.DTOs.Report;

namespace CRM.Application.Interfaces;

public interface IDashboardService
{
    Task<DashboardStatsDto> GetDashboardStatsAsync(Guid? userId = null);
    Task<IEnumerable<RevenueReportDto>> GetRevenueReportAsync(ReportFilterDto filter);
    Task<IEnumerable<DealsByStageReportDto>> GetDealsByStageReportAsync();
    Task<IEnumerable<CustomersByIndustryReportDto>> GetCustomersByIndustryReportAsync();
    Task<IEnumerable<SalesPerformanceDto>> GetSalesPerformanceAsync(ReportFilterDto filter);
    Task<IEnumerable<ActivityLogDto>> GetRecentActivitiesAsync(int count = 10);

    // Role-specific dashboards
    Task<ProductionDashboardDto> GetProductionDashboardAsync();
    Task<QualityDashboardDto> GetQualityDashboardAsync();
    Task<DeliveryDashboardDto> GetDeliveryDashboardAsync();
}
