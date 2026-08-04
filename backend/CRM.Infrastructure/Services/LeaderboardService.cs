using CRM.Application.DTOs.Report;
using CRM.Application.Interfaces;
using CRM.Core.Enums;
using CRM.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace CRM.Infrastructure.Services;

// Bảng xếp hạng hiệu suất theo bộ phận (Sales/Design/Production/Delivery) — ai cũng xem được,
// không phân biệt vai trò. Mọi bộ phận đều xếp theo SỐ ĐƠN HÀNG (Sales có thêm doanh số) —
// không tính theo số lượng công việc/công đoạn hoàn thành.
public class LeaderboardService : ILeaderboardService
{
    private readonly CrmDbContext _db;
    public LeaderboardService(CrmDbContext db) { _db = db; }

    public async Task<LeaderboardResultDto> GetLeaderboardAsync(LeaderboardScope scope, LeaderboardPeriod period, DateTime referenceDate)
    {
        var (start, end, label) = ResolvePeriod(period, referenceDate);
        var (prevStart, prevEnd, _) = ResolvePeriod(period, start.AddDays(-1));

        var current = await AggregateAsync(scope, start, end);
        var previous = await AggregateAsync(scope, prevStart, prevEnd);
        var prevByUser = previous.ToDictionary(x => x.UserId, x => x.Primary);

        var userIds = current.Select(x => x.UserId).ToList();
        var names = await _db.Users
            .Where(u => userIds.Contains(u.Id))
            .Select(u => new { u.Id, u.FirstName, u.LastName })
            .ToListAsync();
        var nameById = names.ToDictionary(u => u.Id, u => $"{u.FirstName} {u.LastName}".Trim());

        var entries = current
            .OrderByDescending(x => x.Primary)
            .Select((x, i) => new LeaderboardEntryDto
            {
                UserId = x.UserId,
                FullName = nameById.TryGetValue(x.UserId, out var n) && !string.IsNullOrWhiteSpace(n) ? n : "(Không rõ)",
                Rank = i + 1,
                Revenue = scope == LeaderboardScope.Sales ? x.Primary : 0,
                Count = x.Count,
                GrowthPercent = ComputeGrowth(x.Primary, prevByUser.GetValueOrDefault(x.UserId)),
                KpiProgressPercent = null
            })
            .ToList();

        return new LeaderboardResultDto
        {
            Scope = scope,
            Period = period,
            PeriodStart = start,
            PeriodEnd = end,
            PeriodLabel = label,
            UpdatedAt = DateTime.UtcNow,
            PrimaryMetric = scope == LeaderboardScope.Sales ? "revenue" : "count",
            CountLabel = CountLabel,
            Entries = entries
        };
    }

    private const string CountLabel = "Số đơn";

    private static decimal? ComputeGrowth(decimal current, decimal previous)
    {
        if (previous <= 0) return null;
        return Math.Round((current - previous) / previous * 100, 1);
    }

    private record AggregateRow(Guid UserId, decimal Primary, int Count);

    private async Task<List<AggregateRow>> AggregateAsync(LeaderboardScope scope, DateTime start, DateTime end)
    {
        switch (scope)
        {
            case LeaderboardScope.Sales:
                return await _db.Orders
                    .Where(o => o.AssignedToUserId != null
                             && o.Status != OrderStatus.Cancelled
                             && o.OrderDate >= start && o.OrderDate < end)
                    .GroupBy(o => o.AssignedToUserId!.Value)
                    .Select(g => new AggregateRow(g.Key, g.Sum(x => x.TotalAmount), g.Count()))
                    .ToListAsync();

            case LeaderboardScope.Design:
                // Đếm số ĐƠN HÀNG designer phụ trách (Order.DesignerUserId) — không đếm số bản ghi Design.
                return await _db.Orders
                    .Where(o => o.DesignerUserId != null
                             && o.Status != OrderStatus.Cancelled
                             && o.OrderDate >= start && o.OrderDate < end)
                    .GroupBy(o => o.DesignerUserId!.Value)
                    .Select(g => new AggregateRow(g.Key, g.Count(), g.Count()))
                    .ToListAsync();

            case LeaderboardScope.Production:
                // Đếm số ĐƠN HÀNG khác nhau (không đếm số công đoạn) mà nhân viên có hoàn thành ít nhất 1 công đoạn trong kỳ.
                return (await _db.Set<CRM.Core.Entities.OrderProductionStep>()
                    .Where(s => s.CompletedByUserId != null
                             && s.IsCompleted
                             && s.CompletedAt != null
                             && s.CompletedAt >= start && s.CompletedAt < end)
                    .Select(s => new { UserId = s.CompletedByUserId!.Value, s.OrderId })
                    .Distinct()
                    .ToListAsync())
                    .GroupBy(x => x.UserId)
                    .Select(g => new AggregateRow(g.Key, g.Count(), g.Count()))
                    .ToList();

            case LeaderboardScope.Delivery:
                // Không có cột ShippedDate riêng — dùng UpdatedAt của đơn ở trạng thái Đã giao/Hoàn thành làm mốc thời gian giao hàng.
                return await _db.Orders
                    .Where(o => o.ShipperUserId != null
                             && (o.Status == OrderStatus.Delivered || o.Status == OrderStatus.Completed)
                             && o.UpdatedAt != null
                             && o.UpdatedAt >= start && o.UpdatedAt < end)
                    .GroupBy(o => o.ShipperUserId!.Value)
                    .Select(g => new AggregateRow(g.Key, g.Count(), g.Count()))
                    .ToListAsync();

            default:
                return new List<AggregateRow>();
        }
    }

    private static (DateTime Start, DateTime End, string Label) ResolvePeriod(LeaderboardPeriod period, DateTime reference)
    {
        var r = DateTime.SpecifyKind(reference.Date, DateTimeKind.Utc);
        switch (period)
        {
            case LeaderboardPeriod.Week:
                // Tuần bắt đầu Thứ 2 (ISO), theo giờ UTC lưu trong DB.
                var diff = ((int)r.DayOfWeek + 6) % 7;
                var weekStart = r.AddDays(-diff);
                var weekEnd = weekStart.AddDays(7);
                return (weekStart, weekEnd, $"Tuần {System.Globalization.ISOWeek.GetWeekOfYear(weekStart)}/{weekStart.Year}");

            case LeaderboardPeriod.Quarter:
                var q = (r.Month - 1) / 3;
                var qStart = new DateTime(r.Year, q * 3 + 1, 1, 0, 0, 0, DateTimeKind.Utc);
                var qEnd = qStart.AddMonths(3);
                return (qStart, qEnd, $"Quý {q + 1}/{r.Year}");

            case LeaderboardPeriod.Month:
            default:
                var mStart = new DateTime(r.Year, r.Month, 1, 0, 0, 0, DateTimeKind.Utc);
                var mEnd = mStart.AddMonths(1);
                return (mStart, mEnd, $"Tháng {r.Month:D2}/{r.Year}");
        }
    }
}
