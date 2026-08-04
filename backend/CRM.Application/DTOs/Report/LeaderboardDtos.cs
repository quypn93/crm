namespace CRM.Application.DTOs.Report;

public enum LeaderboardScope
{
    Sales = 0,
    Design = 1,
    Production = 2,
    Delivery = 3
}

public enum LeaderboardPeriod
{
    Week = 0,
    Month = 1,
    Quarter = 2
}

public class LeaderboardEntryDto
{
    public Guid UserId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public int Rank { get; set; }

    // Sales: tổng doanh số đơn phụ trách. Các scope khác luôn 0.
    public decimal Revenue { get; set; }
    // Sales: số đơn phụ trách. Design: số thiết kế hoàn thành. Production: số công đoạn hoàn thành. Delivery: số đơn đã giao.
    public int Count { get; set; }

    // % tăng trưởng so với kỳ liền trước (theo cùng chỉ số dùng để xếp hạng). Null nếu kỳ trước = 0 (không tính được %).
    public decimal? GrowthPercent { get; set; }

    // Tiến độ KPI cá nhân — chưa có dữ liệu mục tiêu KPI trong hệ thống nên luôn null (hiển thị "Chưa thiết lập").
    public decimal? KpiProgressPercent { get; set; }
}

public class LeaderboardResultDto
{
    public LeaderboardScope Scope { get; set; }
    public LeaderboardPeriod Period { get; set; }
    public DateTime PeriodStart { get; set; }
    public DateTime PeriodEnd { get; set; }
    public string PeriodLabel { get; set; } = string.Empty;
    public DateTime UpdatedAt { get; set; }
    // "revenue" (Sales, có thể toggle sang số đơn ở FE) hoặc "count" (Design/Production/Delivery).
    public string PrimaryMetric { get; set; } = "revenue";
    public string CountLabel { get; set; } = string.Empty;
    public List<LeaderboardEntryDto> Entries { get; set; } = new();
}
