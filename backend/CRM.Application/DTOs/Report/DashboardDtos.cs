namespace CRM.Application.DTOs.Report;

public class DashboardStatsDto
{
    public int TotalCustomers { get; set; }
    public int TotalDeals { get; set; }
    public int TotalTasks { get; set; }
    public decimal TotalRevenue { get; set; }
    public int WonDealsCount { get; set; }
    public int LostDealsCount { get; set; }
    public int PendingTasksCount { get; set; }
    public int OverdueTasksCount { get; set; }
    public decimal ConversionRate { get; set; }
    public decimal RevenueGrowth { get; set; }
    public int NewCustomersThisMonth { get; set; }
    public int DealsInPipeline { get; set; }
    public decimal PipelineValue { get; set; }
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
