using CRM.Application.DTOs.Finance;
using CRM.Application.Interfaces;
using CRM.Core.Entities;
using CRM.Core.Enums;
using CRM.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace CRM.Infrastructure.Services.Finance;

public class ProfitReportService : IProfitReportService
{
    private readonly CrmDbContext _db;

    public ProfitReportService(CrmDbContext db) { _db = db; }

    /// <summary>Đơn tính vào báo cáo: đã lên sản xuất trở đi, không tính đơn hủy.</summary>
    private IQueryable<Order> RevenueOrders() =>
        _db.Orders.AsNoTracking()
            .Where(o => o.Status >= OrderStatus.InProduction && o.Status != OrderStatus.Cancelled);

    private static IQueryable<Order> ApplyDateRange(IQueryable<Order> q, string basis, DateTime? from, DateTime? to)
    {
        if (from.HasValue)
        {
            var f = from.Value.ToUniversalTime();
            q = basis switch
            {
                "order" => q.Where(o => o.OrderDate >= f),
                "completed" => q.Where(o => o.CompletionDate >= f),
                "delivered" => q.Where(o => o.ActualDeliveryDate >= f),
                _ => q.Where(o => (o.ConfirmedDate ?? o.OrderDate) >= f)
            };
        }
        if (to.HasValue)
        {
            var t = to.Value.ToUniversalTime();
            q = basis switch
            {
                "order" => q.Where(o => o.OrderDate <= t),
                "completed" => q.Where(o => o.CompletionDate <= t),
                "delivered" => q.Where(o => o.ActualDeliveryDate <= t),
                _ => q.Where(o => (o.ConfirmedDate ?? o.OrderDate) <= t)
            };
        }
        // Mốc "completed"/"delivered" có thể null → loại các đơn chưa có mốc đó khỏi báo cáo.
        return basis switch
        {
            "completed" => q.Where(o => o.CompletionDate != null),
            "delivered" => q.Where(o => o.ActualDeliveryDate != null),
            _ => q
        };
    }

    public async Task<OrderProfitResultDto> GetOrderProfitAsync(ProfitFilterDto filter)
    {
        var basis = FinanceHelpers.NormalizeBasis(filter.RevenueBasis);
        var orders = ApplyDateRange(RevenueOrders(), basis, filter.DateFrom, filter.DateTo);

        if (!string.IsNullOrWhiteSpace(filter.Search))
        {
            var s = filter.Search.Trim();
            orders = orders.Where(o =>
                EF.Functions.ILike(o.OrderNumber, $"%{s}%") ||
                (o.CustomerName != null && EF.Functions.ILike(o.CustomerName, $"%{s}%")));
        }

        var joined = from o in orders
                     join c in _db.OrderCosts.AsNoTracking() on o.Id equals c.OrderId into gc
                     from c in gc.DefaultIfEmpty()
                     select new { o, c };

        var totals = await joined
            .GroupBy(x => 1)
            .Select(g => new
            {
                Count = g.Count(),
                WithoutCost = g.Count(x => x.c == null),
                Revenue = g.Sum(x => x.o.TotalAmount),
                RevenueWithCost = g.Sum(x => x.c != null ? x.o.TotalAmount : 0m),
                Cost = g.Sum(x => x.c != null ? x.c.TotalCost : 0m)
            })
            .FirstOrDefaultAsync();

        var page = filter.Page < 1 ? 1 : filter.Page;
        var pageSize = filter.PageSize is < 1 or > 500 ? 100 : filter.PageSize;

        var rows = await joined
            .OrderByDescending(x => x.o.ConfirmedDate ?? x.o.OrderDate)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(x => new
            {
                x.o.Id,
                x.o.OrderNumber,
                x.o.CustomerName,
                x.o.Status,
                x.o.OrderDate,
                x.o.ConfirmedDate,
                x.o.CompletionDate,
                x.o.ActualDeliveryDate,
                x.o.TotalAmount,
                Cost = x.c
            })
            .ToListAsync();

        var items = rows.Select(r =>
        {
            var revenueDate = PickDate(basis, r.OrderDate, r.ConfirmedDate, r.CompletionDate, r.ActualDeliveryDate);
            var totalCost = r.Cost?.TotalCost ?? 0m;
            var profit = r.TotalAmount - totalCost;
            return new OrderProfitDto
            {
                OrderId = r.Id,
                OrderNumber = r.OrderNumber,
                CustomerName = r.CustomerName,
                RevenueDate = revenueDate,
                StatusName = FinanceHelpers.StatusName(r.Status),
                Revenue = r.TotalAmount,
                CostAmount = r.Cost?.CostAmount ?? 0m,
                ShippingCost = r.Cost?.ShippingCost ?? 0m,
                OutboundShippingCost = r.Cost?.OutboundShippingCost ?? 0m,
                OtherCost = r.Cost?.OtherCost ?? 0m,
                TotalCost = totalCost,
                Profit = profit,
                ProfitMargin = FinanceHelpers.Margin(r.TotalAmount, profit),
                HasCost = r.Cost != null
            };
        }).ToList();

        var count = totals?.Count ?? 0;
        var revenueWithCost = totals?.RevenueWithCost ?? 0m;
        var cost = totals?.Cost ?? 0m;
        var profitTotal = revenueWithCost - cost;

        return new OrderProfitResultDto
        {
            Items = items,
            TotalCount = count,
            Page = page,
            PageSize = pageSize,
            TotalPages = (int)Math.Ceiling(count / (double)pageSize),
            TotalRevenue = totals?.Revenue ?? 0m,
            TotalCost = cost,
            TotalProfit = profitTotal,
            AverageMargin = FinanceHelpers.Margin(revenueWithCost, profitTotal),
            OrdersWithoutCost = totals?.WithoutCost ?? 0,
            RevenueWithoutCost = (totals?.Revenue ?? 0m) - revenueWithCost
        };
    }

    public async Task<MonthlyProfitResultDto> GetMonthlyProfitAsync(int year, string? revenueBasis)
    {
        var basis = FinanceHelpers.NormalizeBasis(revenueBasis);

        // Biên năm theo giờ VN, quy về UTC để so với cột timestamptz.
        var yearStartUtc = FinanceHelpers.VnDateToUtc(year, 1, 1);
        var yearEndUtc = FinanceHelpers.VnDateToUtc(year + 1, 1, 1);

        var orders = ApplyDateRange(RevenueOrders(), basis, yearStartUtc, yearEndUtc);

        // Gom nhóm tháng ở bộ nhớ để quy đổi UTC → giờ VN cho chính xác.
        var rows = await (from o in orders
                          join c in _db.OrderCosts.AsNoTracking() on o.Id equals c.OrderId into gc
                          from c in gc.DefaultIfEmpty()
                          select new
                          {
                              o.OrderDate,
                              o.ConfirmedDate,
                              o.CompletionDate,
                              o.ActualDeliveryDate,
                              o.TotalAmount,
                              TotalCost = c != null ? c.TotalCost : (decimal?)null
                          }).ToListAsync();

        var byMonth = rows
            .Select(r => new
            {
                Month = FinanceHelpers.ToVn(PickDate(basis, r.OrderDate, r.ConfirmedDate, r.CompletionDate, r.ActualDeliveryDate)).Month,
                r.TotalAmount,
                r.TotalCost
            })
            .GroupBy(r => r.Month)
            .ToDictionary(g => g.Key, g => new
            {
                Count = g.Count(),
                WithoutCost = g.Count(x => x.TotalCost == null),
                Revenue = g.Sum(x => x.TotalAmount),
                Cogs = g.Sum(x => x.TotalCost ?? 0m)
            });

        var payroll = (await _db.PayrollEntries.AsNoTracking()
                .Where(x => x.Year == year)
                .GroupBy(x => x.Month)
                .Select(g => new { Month = g.Key, Total = g.Sum(x => x.TotalAmount) })
                .ToListAsync())
            .ToDictionary(x => x.Month, x => x.Total);

        var fromDate = new DateOnly(year, 1, 1);
        var toDate = new DateOnly(year, 12, 31);
        var fixedCosts = (await _db.FixedExpenses.AsNoTracking()
                .Where(x => x.ExpenseDate >= fromDate && x.ExpenseDate <= toDate)
                .Select(x => new { x.ExpenseDate, x.Amount })
                .ToListAsync())
            .GroupBy(x => x.ExpenseDate.Month)
            .ToDictionary(g => g.Key, g => g.Sum(x => x.Amount));

        var months = new List<MonthlyProfitDto>();
        for (var m = 1; m <= 12; m++)
        {
            byMonth.TryGetValue(m, out var o);
            payroll.TryGetValue(m, out var payrollCost);
            fixedCosts.TryGetValue(m, out var fixedCost);

            months.Add(BuildMonth(year, m,
                orderCount: o?.Count ?? 0,
                ordersWithoutCost: o?.WithoutCost ?? 0,
                revenue: o?.Revenue ?? 0m,
                cogs: o?.Cogs ?? 0m,
                payrollCost: payrollCost,
                fixedCost: fixedCost));
        }

        var total = BuildMonth(year, 0,
            months.Sum(x => x.OrderCount),
            months.Sum(x => x.OrdersWithoutCost),
            months.Sum(x => x.Revenue),
            months.Sum(x => x.Cogs),
            months.Sum(x => x.PayrollCost),
            months.Sum(x => x.FixedCost));
        total.Label = $"Cả năm {year}";

        return new MonthlyProfitResultDto
        {
            Year = year,
            RevenueBasis = basis,
            Months = months,
            Total = total
        };
    }

    public async Task<MonthlyProfitDetailDto> GetMonthDetailAsync(int year, int month, string? revenueBasis)
    {
        if (month is < 1 or > 12) throw new InvalidOperationException("Tháng không hợp lệ.");

        var basis = FinanceHelpers.NormalizeBasis(revenueBasis);
        var monthStartUtc = FinanceHelpers.VnDateToUtc(year, month, 1);
        var nextMonth = month == 12 ? FinanceHelpers.VnDateToUtc(year + 1, 1, 1) : FinanceHelpers.VnDateToUtc(year, month + 1, 1);

        var orderResult = await GetOrderProfitAsync(new ProfitFilterDto
        {
            DateFrom = monthStartUtc,
            DateTo = nextMonth.AddTicks(-1),
            RevenueBasis = basis,
            Page = 1,
            PageSize = 500
        });

        var payrollService = new PayrollService(_db);
        var payroll = await payrollService.GetPeriodAsync(year, month);

        var fromDate = new DateOnly(year, month, 1);
        var toDate = fromDate.AddMonths(1).AddDays(-1);
        // Sắp xếp ở client — xem chú thích tương tự trong FixedExpenseService.
        var fixedByCategory = (await _db.FixedExpenses.AsNoTracking()
                .Where(x => x.ExpenseDate >= fromDate && x.ExpenseDate <= toDate)
                .GroupBy(x => new { x.ExpenseCategoryId, x.CategoryName })
                .Select(g => new ExpenseCategoryTotalDto
                {
                    CategoryId = g.Key.ExpenseCategoryId,
                    CategoryName = g.Key.CategoryName,
                    Amount = g.Sum(x => x.Amount)
                })
                .ToListAsync())
            .OrderByDescending(x => x.Amount)
            .ToList();

        var summary = BuildMonth(year, month,
            orderCount: orderResult.TotalCount,
            ordersWithoutCost: orderResult.OrdersWithoutCost,
            revenue: orderResult.TotalRevenue,
            cogs: orderResult.TotalCost,
            payrollCost: payroll.GrandTotal,
            fixedCost: fixedByCategory.Sum(x => x.Amount));

        return new MonthlyProfitDetailDto
        {
            Summary = summary,
            FixedByCategory = fixedByCategory,
            PayrollEntries = payroll.Items,
            Orders = orderResult.Items
        };
    }

    // ── private ───────────────────────────────────────────────────────────

    private static MonthlyProfitDto BuildMonth(
        int year, int month, int orderCount, int ordersWithoutCost,
        decimal revenue, decimal cogs, decimal payrollCost, decimal fixedCost)
    {
        var gross = revenue - cogs;
        var net = gross - payrollCost - fixedCost;
        return new MonthlyProfitDto
        {
            Year = year,
            Month = month,
            Label = month == 0 ? $"Cả năm {year}" : $"{month:00}/{year}",
            OrderCount = orderCount,
            OrdersWithoutCost = ordersWithoutCost,
            Revenue = revenue,
            Cogs = cogs,
            GrossProfit = gross,
            PayrollCost = payrollCost,
            FixedCost = fixedCost,
            NetProfit = net,
            NetMargin = FinanceHelpers.Margin(revenue, net)
        };
    }

    private static DateTime PickDate(string basis, DateTime orderDate, DateTime? confirmed, DateTime? completed, DateTime? delivered)
        => basis switch
        {
            "order" => orderDate,
            "completed" => completed ?? orderDate,
            "delivered" => delivered ?? orderDate,
            _ => confirmed ?? orderDate
        };
}
