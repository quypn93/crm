using System.Globalization;
using System.Text;
using ClosedXML.Excel;
using CRM.Application.DTOs.Finance;
using CRM.Application.Interfaces;
using CRM.Core.Entities;
using CRM.Core.Enums;
using CRM.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace CRM.Infrastructure.Services.Finance;

public class OrderCostService : IOrderCostService
{
    private readonly CrmDbContext _db;

    public OrderCostService(CrmDbContext db) { _db = db; }

    // Đơn "nhảy vào" mục chi phí khi đã lên sản xuất trở đi. Đơn hủy không tính.
    private IQueryable<Order> BaseOrderQuery(OrderStatus? status)
    {
        var q = _db.Orders.AsNoTracking();
        return status.HasValue
            ? q.Where(o => o.Status == status.Value)
            : q.Where(o => o.Status >= OrderStatus.InProduction && o.Status != OrderStatus.Cancelled);
    }

    public async Task<OrderCostListResultDto> GetListAsync(OrderCostFilterDto filter)
    {
        var basis = FinanceHelpers.NormalizeBasis(filter.DateBasis);

        var orders = BaseOrderQuery(filter.Status);

        if (filter.DateFrom.HasValue)
        {
            var from = filter.DateFrom.Value.ToUniversalTime();
            orders = basis switch
            {
                "order" => orders.Where(o => o.OrderDate >= from),
                "completed" => orders.Where(o => o.CompletionDate >= from),
                "delivered" => orders.Where(o => o.ActualDeliveryDate >= from),
                _ => orders.Where(o => (o.ConfirmedDate ?? o.OrderDate) >= from)
            };
        }
        if (filter.DateTo.HasValue)
        {
            var to = filter.DateTo.Value.ToUniversalTime();
            orders = basis switch
            {
                "order" => orders.Where(o => o.OrderDate <= to),
                "completed" => orders.Where(o => o.CompletionDate <= to),
                "delivered" => orders.Where(o => o.ActualDeliveryDate <= to),
                _ => orders.Where(o => (o.ConfirmedDate ?? o.OrderDate) <= to)
            };
        }

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

        if (filter.HasCost == true) joined = joined.Where(x => x.c != null);
        else if (filter.HasCost == false) joined = joined.Where(x => x.c == null);

        // Tổng của toàn bộ kết quả lọc (không chỉ trang hiện tại).
        var totals = await joined
            .GroupBy(x => 1)
            .Select(g => new
            {
                Count = g.Count(),
                WithCost = g.Count(x => x.c != null),
                Revenue = g.Sum(x => x.o.TotalAmount),
                Cost = g.Sum(x => x.c != null ? x.c.TotalCost : 0m),
                RevenueWithCost = g.Sum(x => x.c != null ? x.o.TotalAmount : 0m)
            })
            .FirstOrDefaultAsync();

        var page = filter.Page < 1 ? 1 : filter.Page;
        var pageSize = filter.PageSize is < 1 or > 500 ? 100 : filter.PageSize;

        var rows = await joined
            .OrderByDescending(x => x.o.ConfirmedDate ?? x.o.OrderDate)
            .ThenByDescending(x => x.o.OrderNumber)
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
                x.o.TotalAmount,
                x.o.PaidAmount,
                x.o.GhtkLabel,
                x.o.ViettelPostLabel,
                // Đơn đồng phục tách nhiều dòng theo size nhưng chỉ 1 loại SP — cộng hết lại.
                TotalQuantity = x.o.Items.Sum(i => (int?)i.Quantity) ?? 0,
                CreatorFirst = x.o.CreatedByUser.FirstName,
                CreatorLast = x.o.CreatedByUser.LastName,
                Cost = x.c,
                EnteredFirst = x.c != null && x.c.EnteredByUser != null ? x.c.EnteredByUser.FirstName : null,
                EnteredLast = x.c != null && x.c.EnteredByUser != null ? x.c.EnteredByUser.LastName : null
            })
            .ToListAsync();

        var items = rows.Select(r =>
        {
            var totalCost = r.Cost?.TotalCost ?? 0m;
            var profit = r.TotalAmount - totalCost;
            return new OrderCostListItemDto
            {
                OrderId = r.Id,
                OrderNumber = r.OrderNumber,
                CustomerName = r.CustomerName,
                Status = r.Status,
                StatusName = FinanceHelpers.StatusName(r.Status),
                OrderDate = r.OrderDate,
                ConfirmedDate = r.ConfirmedDate,
                CompletionDate = r.CompletionDate,
                CreatedByUserName = $"{r.CreatorFirst} {r.CreatorLast}".Trim(),
                Revenue = r.TotalAmount,
                PaidAmount = r.PaidAmount,
                TotalQuantity = r.TotalQuantity,
                UnitCost = r.Cost?.UnitCost ?? 0m,
                CostAmount = r.Cost?.CostAmount ?? 0m,
                GiftUnitCost = r.Cost?.GiftUnitCost ?? 0m,
                GiftQuantity = r.Cost?.GiftQuantity ?? 0,
                GiftAmount = r.Cost?.GiftAmount ?? 0m,
                ShippingCost = r.Cost?.ShippingCost ?? 0m,
                OutboundShippingCost = r.Cost?.OutboundShippingCost ?? 0m,
                OtherCost = r.Cost?.OtherCost ?? 0m,
                TotalCost = totalCost,
                ShippingCode = CarrierLabel(r.GhtkLabel, r.ViettelPostLabel) ?? r.Cost?.ShippingCode,
                ShippingCodeFromCarrier = CarrierLabel(r.GhtkLabel, r.ViettelPostLabel) != null,
                SettlementAmount = r.Cost?.SettlementAmount ?? 0m,
                Profit = profit,
                ProfitMargin = FinanceHelpers.Margin(r.TotalAmount, profit),
                HasCost = r.Cost != null,
                IsFinalized = r.Cost?.IsFinalized ?? false,
                Notes = r.Cost?.Notes,
                CostFileUrl = r.Cost?.CostFileUrl,
                CostFileName = r.Cost?.CostFileName,
                EnteredByUserName = r.Cost == null ? null : $"{r.EnteredFirst} {r.EnteredLast}".Trim(),
                EnteredAt = r.Cost?.EnteredAt
            };
        }).ToList();

        var count = totals?.Count ?? 0;
        return new OrderCostListResultDto
        {
            Items = items,
            TotalCount = count,
            Page = page,
            PageSize = pageSize,
            TotalPages = (int)Math.Ceiling(count / (double)pageSize),
            Summary = new OrderCostSummaryDto
            {
                TotalOrders = count,
                OrdersWithCost = totals?.WithCost ?? 0,
                OrdersWithoutCost = count - (totals?.WithCost ?? 0),
                TotalRevenue = totals?.Revenue ?? 0m,
                TotalCost = totals?.Cost ?? 0m,
                // Chỉ lấy lãi trên các đơn ĐÃ có cost — tránh lãi ảo từ đơn chưa nhập.
                TotalProfit = (totals?.RevenueWithCost ?? 0m) - (totals?.Cost ?? 0m)
            }
        };
    }

    public async Task<OrderCostListItemDto?> GetByOrderIdAsync(Guid orderId)
    {
        var order = await _db.Orders.AsNoTracking()
            .Include(o => o.CreatedByUser)
            .FirstOrDefaultAsync(o => o.Id == orderId);
        if (order == null) return null;

        var cost = await _db.OrderCosts.AsNoTracking()
            .Include(c => c.EnteredByUser)
            .FirstOrDefaultAsync(c => c.OrderId == orderId);

        return ToDto(order, cost);
    }

    public async Task<OrderCostListItemDto> UpsertAsync(Guid orderId, UpsertOrderCostDto dto, Guid userId, bool isAdmin)
    {
        var order = await _db.Orders.Include(o => o.CreatedByUser).Include(o => o.Items)
                .FirstOrDefaultAsync(o => o.Id == orderId)
            ?? throw new KeyNotFoundException("Không tìm thấy đơn hàng.");

        var cost = await ApplyUpsertAsync(order, dto, userId, isAdmin);
        await _db.SaveChangesAsync();

        await _db.Entry(cost).Reference(c => c.EnteredByUser).LoadAsync();
        return ToDto(order, cost);
    }

    public async Task<int> BulkUpsertAsync(BulkUpsertOrderCostDto dto, Guid userId, bool isAdmin)
    {
        if (dto.Items.Count == 0) return 0;

        var ids = dto.Items.Select(i => i.OrderId).Distinct().ToList();
        var orders = await _db.Orders.Include(o => o.Items)
            .Where(o => ids.Contains(o.Id)).ToDictionaryAsync(o => o.Id);

        var saved = 0;
        foreach (var item in dto.Items)
        {
            if (!orders.TryGetValue(item.OrderId, out var order)) continue;
            try
            {
                await ApplyUpsertAsync(order, item, userId, isAdmin);
                saved++;
            }
            catch (InvalidOperationException)
            {
                // Dòng đã chốt sổ mà người dùng không phải Admin — bỏ qua, các dòng khác vẫn lưu.
            }
        }

        await _db.SaveChangesAsync();
        return saved;
    }

    public async Task<OrderCostListItemDto> SetAttachmentAsync(Guid orderId, string url, string fileName, Guid userId)
    {
        var order = await _db.Orders.Include(o => o.CreatedByUser).Include(o => o.Items)
                .FirstOrDefaultAsync(o => o.Id == orderId)
            ?? throw new KeyNotFoundException("Không tìm thấy đơn hàng.");

        var cost = await _db.OrderCosts.FirstOrDefaultAsync(c => c.OrderId == orderId);
        if (cost == null)
        {
            cost = new OrderCost { OrderId = orderId };
            _db.OrderCosts.Add(cost);
        }

        cost.CostFileUrl = url;
        cost.CostFileName = fileName;
        cost.EnteredByUserId = userId;
        cost.EnteredAt = DateTime.UtcNow;
        cost.UpdatedAt = DateTime.UtcNow;
        cost.Recalculate(TotalQuantityOf(order));

        await _db.SaveChangesAsync();
        return ToDto(order, cost);
    }

    // ── Import file giá cost ──────────────────────────────────────────────
    // Cột: Mã đơn hàng | Giá cost | Chi phí ship hàng | Chi phí gửi hàng đi | Chi phí khác | Ghi chú
    // Ô để trống → GIỮ NGUYÊN giá trị cũ (không ghi đè bằng 0).
    public async Task<CostImportResultDto> ImportAsync(Stream fileStream, string fileName, Guid userId, bool isAdmin)
    {
        var rows = fileName.EndsWith(".xlsx", StringComparison.OrdinalIgnoreCase)
            ? ReadXlsx(fileStream)
            : ReadCsv(fileStream);

        var result = new CostImportResultDto { TotalRows = rows.Count };
        if (rows.Count == 0)
        {
            result.Errors.Add(new CostImportRowErrorDto { RowNumber = 0, Error = "File không có dòng dữ liệu nào." });
            return result;
        }

        var orderNumbers = rows.Select(r => r.OrderNumber).Where(n => !string.IsNullOrWhiteSpace(n)).Distinct().ToList();
        var orders = await _db.Orders
            .Include(o => o.Items)                       // cần tổng SL để nhân với đơn giá
            .Where(o => orderNumbers.Contains(o.OrderNumber))
            .ToDictionaryAsync(o => o.OrderNumber, StringComparer.OrdinalIgnoreCase);

        var existingCosts = (await _db.OrderCosts
                .Where(c => orders.Values.Select(o => o.Id).Contains(c.OrderId))
                .ToListAsync())
            .ToDictionary(c => c.OrderId);

        foreach (var row in rows)
        {
            if (string.IsNullOrWhiteSpace(row.OrderNumber))
            {
                result.Errors.Add(new CostImportRowErrorDto { RowNumber = row.RowNumber, Error = "Thiếu mã đơn hàng." });
                continue;
            }
            if (!orders.TryGetValue(row.OrderNumber.Trim(), out var order))
            {
                result.Errors.Add(new CostImportRowErrorDto
                {
                    RowNumber = row.RowNumber,
                    OrderNumber = row.OrderNumber,
                    Error = "Không tìm thấy đơn hàng với mã này."
                });
                continue;
            }
            if (row.ParseError != null)
            {
                result.Errors.Add(new CostImportRowErrorDto
                {
                    RowNumber = row.RowNumber,
                    OrderNumber = row.OrderNumber,
                    Error = row.ParseError
                });
                continue;
            }

            if (!existingCosts.TryGetValue(order.Id, out var cost))
            {
                cost = new OrderCost { OrderId = order.Id };
                _db.OrderCosts.Add(cost);
                existingCosts[order.Id] = cost;
            }
            else if (cost.IsFinalized && !isAdmin)
            {
                result.SkippedCount++;
                result.Errors.Add(new CostImportRowErrorDto
                {
                    RowNumber = row.RowNumber,
                    OrderNumber = row.OrderNumber,
                    Error = "Chi phí đơn này đã chốt sổ — bỏ qua."
                });
                continue;
            }

            if (row.UnitCost.HasValue) cost.UnitCost = row.UnitCost.Value;
            if (row.GiftUnitCost.HasValue) cost.GiftUnitCost = row.GiftUnitCost.Value;
            if (row.GiftQuantity.HasValue) cost.GiftQuantity = (int)row.GiftQuantity.Value;
            if (row.ShippingCost.HasValue) cost.ShippingCost = row.ShippingCost.Value;
            if (row.OutboundShippingCost.HasValue) cost.OutboundShippingCost = row.OutboundShippingCost.Value;
            if (row.OtherCost.HasValue) cost.OtherCost = row.OtherCost.Value;
            if (row.SettlementAmount.HasValue) cost.SettlementAmount = row.SettlementAmount.Value;
            // Mã hãng vận chuyển trả về luôn thắng mã trong file.
            if (!string.IsNullOrWhiteSpace(row.ShippingCode)
                && CarrierLabel(order.GhtkLabel, order.ViettelPostLabel) == null)
                cost.ShippingCode = row.ShippingCode.Trim();
            if (!string.IsNullOrWhiteSpace(row.Notes)) cost.Notes = row.Notes.Trim();

            cost.Recalculate(TotalQuantityOf(order));
            cost.EnteredByUserId = userId;
            cost.EnteredAt = DateTime.UtcNow;
            cost.UpdatedAt = DateTime.UtcNow;
            result.SuccessCount++;
        }

        if (result.SuccessCount > 0) await _db.SaveChangesAsync();
        return result;
    }

    // ── private ───────────────────────────────────────────────────────────

    private async Task<OrderCost> ApplyUpsertAsync(Order order, UpsertOrderCostDto dto, Guid userId, bool isAdmin)
    {
        var cost = await _db.OrderCosts.FirstOrDefaultAsync(c => c.OrderId == order.Id);
        if (cost == null)
        {
            cost = new OrderCost { OrderId = order.Id };
            _db.OrderCosts.Add(cost);
        }
        else if (cost.IsFinalized && !isAdmin)
        {
            throw new InvalidOperationException($"Chi phí đơn {order.OrderNumber} đã chốt sổ — chỉ Admin mở khóa được.");
        }

        if (dto.UnitCost < 0 || dto.GiftUnitCost < 0 || dto.ShippingCost < 0
            || dto.OutboundShippingCost < 0 || dto.OtherCost < 0 || dto.SettlementAmount < 0)
            throw new InvalidOperationException("Số tiền chi phí không được âm.");
        if (dto.GiftQuantity < 0)
            throw new InvalidOperationException("Số lượng quà tặng không được âm.");

        cost.UnitCost = dto.UnitCost;
        cost.GiftUnitCost = dto.GiftUnitCost;
        cost.GiftQuantity = dto.GiftQuantity;
        cost.ShippingCost = dto.ShippingCost;
        cost.OutboundShippingCost = dto.OutboundShippingCost;
        cost.OtherCost = dto.OtherCost;
        cost.SettlementAmount = dto.SettlementAmount;
        // Mã do hãng vận chuyển trả về là nguồn chuẩn — không cho ghi đè bằng mã nhập tay.
        if (CarrierLabel(order.GhtkLabel, order.ViettelPostLabel) == null)
            cost.ShippingCode = string.IsNullOrWhiteSpace(dto.ShippingCode) ? null : dto.ShippingCode.Trim();
        cost.Notes = dto.Notes;
        // Chỉ Admin được bỏ khóa; kế toán chỉ được khóa lại.
        cost.IsFinalized = isAdmin ? dto.IsFinalized : (cost.IsFinalized || dto.IsFinalized);
        cost.Recalculate(TotalQuantityOf(order));
        cost.EnteredByUserId = userId;
        cost.EnteredAt = DateTime.UtcNow;
        cost.UpdatedAt = DateTime.UtcNow;

        return cost;
    }

    private static OrderCostListItemDto ToDto(Order o, OrderCost? c)
    {
        var totalCost = c?.TotalCost ?? 0m;
        var profit = o.TotalAmount - totalCost;
        return new OrderCostListItemDto
        {
            OrderId = o.Id,
            OrderNumber = o.OrderNumber,
            CustomerName = o.CustomerName,
            Status = o.Status,
            StatusName = FinanceHelpers.StatusName(o.Status),
            OrderDate = o.OrderDate,
            ConfirmedDate = o.ConfirmedDate,
            CompletionDate = o.CompletionDate,
            CreatedByUserName = o.CreatedByUser != null
                ? $"{o.CreatedByUser.FirstName} {o.CreatedByUser.LastName}".Trim() : null,
            Revenue = o.TotalAmount,
            PaidAmount = o.PaidAmount,
            TotalQuantity = TotalQuantityOf(o),
            UnitCost = c?.UnitCost ?? 0m,
            CostAmount = c?.CostAmount ?? 0m,
            GiftUnitCost = c?.GiftUnitCost ?? 0m,
            GiftQuantity = c?.GiftQuantity ?? 0,
            GiftAmount = c?.GiftAmount ?? 0m,
            ShippingCost = c?.ShippingCost ?? 0m,
            OutboundShippingCost = c?.OutboundShippingCost ?? 0m,
            OtherCost = c?.OtherCost ?? 0m,
            TotalCost = totalCost,
            ShippingCode = CarrierLabel(o.GhtkLabel, o.ViettelPostLabel) ?? c?.ShippingCode,
            ShippingCodeFromCarrier = CarrierLabel(o.GhtkLabel, o.ViettelPostLabel) != null,
            SettlementAmount = c?.SettlementAmount ?? 0m,
            Profit = profit,
            ProfitMargin = FinanceHelpers.Margin(o.TotalAmount, profit),
            HasCost = c != null,
            IsFinalized = c?.IsFinalized ?? false,
            Notes = c?.Notes,
            CostFileUrl = c?.CostFileUrl,
            CostFileName = c?.CostFileName,
            EnteredByUserName = c?.EnteredByUser != null
                ? $"{c.EnteredByUser.FirstName} {c.EnteredByUser.LastName}".Trim() : null,
            EnteredAt = c?.EnteredAt
        };
    }

    /// <summary>
    /// Tổng SL sản phẩm của đơn — cộng mọi dòng. Đơn đồng phục tách nhiều dòng theo size
    /// nhưng chỉ 1 loại SP 1 mức giá, nên lấy dòng đầu tiên là sai với ~84% đơn.
    /// </summary>
    private static int TotalQuantityOf(Order order)
        => order.Items?.Sum(i => i.Quantity) ?? 0;

    /// <summary>Mã vận đơn do hãng vận chuyển sinh ra, null nếu đơn chưa tạo vận đơn qua API.</summary>
    private static string? CarrierLabel(string? ghtk, string? viettelPost)
        => !string.IsNullOrWhiteSpace(ghtk) ? ghtk
            : !string.IsNullOrWhiteSpace(viettelPost) ? viettelPost
            : null;

    private class ImportRow
    {
        public int RowNumber { get; set; }
        public string OrderNumber { get; set; } = string.Empty;
        public decimal? UnitCost { get; set; }
        public decimal? GiftUnitCost { get; set; }
        public decimal? GiftQuantity { get; set; }
        public decimal? ShippingCost { get; set; }
        public decimal? OutboundShippingCost { get; set; }
        public decimal? OtherCost { get; set; }
        public string? ShippingCode { get; set; }
        public decimal? SettlementAmount { get; set; }
        public string? Notes { get; set; }
        public string? ParseError { get; set; }
    }

    /// <summary>Số cột của file import — phải khớp thứ tự trong <see cref="TemplateHeaders"/>.</summary>
    private const int ImportColumnCount = 10;

    private static List<ImportRow> ReadXlsx(Stream stream)
    {
        var rows = new List<ImportRow>();
        using var wb = new XLWorkbook(stream);
        var ws = wb.Worksheets.FirstOrDefault();
        if (ws == null) return rows;

        var used = ws.RangeUsed();
        if (used == null) return rows;

        // Bỏ dòng đầu (tiêu đề).
        foreach (var r in used.RowsUsed().Skip(1))
        {
            var cells = Enumerable.Range(1, ImportColumnCount).Select(i => r.Cell(i).GetString()).ToArray();
            if (cells.All(string.IsNullOrWhiteSpace)) continue;
            rows.Add(ParseRow(r.RowNumber(), cells));
        }
        return rows;
    }

    private static List<ImportRow> ReadCsv(Stream stream)
    {
        var rows = new List<ImportRow>();
        using var reader = new StreamReader(stream, Encoding.UTF8, detectEncodingFromByteOrderMarks: true);

        var lineNo = 0;
        char? sep = null;
        string? line;
        while ((line = reader.ReadLine()) != null)
        {
            lineNo++;
            if (string.IsNullOrWhiteSpace(line)) continue;

            // Excel VN thường xuất CSV dấu ';'. Đoán dấu phân cách từ dòng đầu tiên.
            sep ??= line.Contains('\t') ? '\t' : line.Contains(';') ? ';' : ',';

            if (lineNo == 1) continue;   // dòng tiêu đề

            var cells = SplitCsvLine(line, sep.Value);
            if (cells.All(string.IsNullOrWhiteSpace)) continue;
            var padded = cells.Concat(Enumerable.Repeat(string.Empty, Math.Max(0, ImportColumnCount - cells.Length))).ToArray();
            rows.Add(ParseRow(lineNo, padded));
        }
        return rows;
    }

    private static string[] SplitCsvLine(string line, char sep)
    {
        var result = new List<string>();
        var sb = new StringBuilder();
        var inQuotes = false;

        for (var i = 0; i < line.Length; i++)
        {
            var ch = line[i];
            if (ch == '"')
            {
                if (inQuotes && i + 1 < line.Length && line[i + 1] == '"') { sb.Append('"'); i++; }
                else inQuotes = !inQuotes;
            }
            else if (ch == sep && !inQuotes)
            {
                result.Add(sb.ToString().Trim());
                sb.Clear();
            }
            else sb.Append(ch);
        }
        result.Add(sb.ToString().Trim());
        return result.ToArray();
    }

    private static ImportRow ParseRow(int rowNumber, string[] cells)
    {
        var row = new ImportRow { RowNumber = rowNumber, OrderNumber = cells[0].Trim() };

        var errors = new List<string>();
        row.UnitCost = ParseMoney(cells.ElementAtOrDefault(1), "Đơn giá cost", errors);
        row.GiftUnitCost = ParseMoney(cells.ElementAtOrDefault(2), "Đơn giá quà tặng", errors);
        row.GiftQuantity = ParseMoney(cells.ElementAtOrDefault(3), "SL quà tặng", errors);
        row.ShippingCost = ParseMoney(cells.ElementAtOrDefault(4), "Chi phí ship hàng", errors);
        row.OutboundShippingCost = ParseMoney(cells.ElementAtOrDefault(5), "Chi phí gửi hàng đi", errors);
        row.OtherCost = ParseMoney(cells.ElementAtOrDefault(6), "Chi phí khác", errors);
        row.ShippingCode = cells.ElementAtOrDefault(7);
        row.SettlementAmount = ParseMoney(cells.ElementAtOrDefault(8), "Số tiền thanh toán", errors);
        row.Notes = cells.ElementAtOrDefault(9);

        if (row.GiftQuantity is { } gq && gq != Math.Floor(gq))
            errors.Add("SL quà tặng phải là số nguyên");

        if (errors.Count > 0) row.ParseError = string.Join("; ", errors);
        return row;
    }

    /// <summary>Chấp nhận "1.200.000", "1,200,000", "1200000", "1200000.50". Ô trống → null (giữ giá trị cũ).</summary>
    private static decimal? ParseMoney(string? raw, string columnName, List<string> errors)
    {
        if (string.IsNullOrWhiteSpace(raw)) return null;

        var s = raw.Trim().Replace(" ", string.Empty).Replace("đ", string.Empty, StringComparison.OrdinalIgnoreCase);

        // Dấu phân cách nghìn kiểu VN ("1.200.000") vs thập phân kiểu Mỹ ("1200.50").
        var lastDot = s.LastIndexOf('.');
        var lastComma = s.LastIndexOf(',');
        if (lastDot >= 0 && lastComma >= 0)
        {
            // Cái nào đứng sau là dấu thập phân.
            if (lastComma > lastDot) s = s.Replace(".", string.Empty).Replace(',', '.');
            else s = s.Replace(",", string.Empty);
        }
        else if (lastDot >= 0)
        {
            // Đúng 3 chữ số sau dấu cuối → dấu phân cách nghìn kiểu VN: "48.000", "1.200.000".
            // VND không dùng phần lẻ nên quy ước này an toàn; "1200.5" vẫn hiểu là thập phân.
            if (s.Length - lastDot - 1 == 3) s = s.Replace(".", string.Empty);
        }
        else if (lastComma >= 0)
        {
            var decimals = s.Length - lastComma - 1;
            s = decimals == 3 ? s.Replace(",", string.Empty) : s.Replace(',', '.');
        }

        if (!decimal.TryParse(s, NumberStyles.Any, CultureInfo.InvariantCulture, out var value))
        {
            errors.Add($"{columnName}: '{raw}' không phải số hợp lệ");
            return null;
        }
        if (value < 0)
        {
            errors.Add($"{columnName}: không được âm");
            return null;
        }
        return value;
    }

    private static readonly string[] TemplateHeaders =
    {
        "Mã đơn hàng",
        "Đơn giá cost",        // giá 1 sản phẩm — hệ thống tự nhân tổng SL của đơn
        "Đơn giá quà tặng",
        "SL quà tặng",         // nhập tay, KHÁC số lượng áo
        "Chi phí ship hàng",
        "Chi phí gửi hàng đi",
        "Chi phí khác",
        "Mã giao hàng",
        "Số tiền thanh toán",
        "Ghi chú"
    };

    private static readonly string[] TemplateSampleRow =
    {
        "XA-000072", "72000", "15000", "5", "35000", "20000", "0", "VD123456789", "1000000",
        "Ví dụ — xóa dòng này trước khi import"
    };

    /// <summary>
    /// File mẫu .csv — cho ai không dùng Excel. Kèm BOM UTF-8 để Excel không vỡ tiếng Việt.
    /// </summary>
    public static byte[] BuildImportTemplateCsv()
    {
        var sb = new StringBuilder();
        sb.Append(string.Join(',', TemplateHeaders)).Append('\n');
        sb.Append(string.Join(',', TemplateSampleRow.Select(v =>
            v.Contains(',') || v.Contains('"') ? $"\"{v.Replace("\"", "\"\"")}\"" : v))).Append('\n');

        return Encoding.UTF8.GetPreamble()
            .Concat(Encoding.UTF8.GetBytes(sb.ToString()))
            .ToArray();
    }

    /// <summary>File mẫu .xlsx cho kế toán tải về.</summary>
    public static byte[] BuildImportTemplate()
    {
        using var wb = new XLWorkbook();
        var ws = wb.AddWorksheet("Gia cost");

        for (var i = 0; i < TemplateHeaders.Length; i++)
        {
            ws.Cell(1, i + 1).Value = TemplateHeaders[i];
            ws.Cell(1, i + 1).Style.Font.Bold = true;
            ws.Cell(1, i + 1).Style.Fill.BackgroundColor = XLColor.LightGray;
        }

        for (var i = 0; i < TemplateSampleRow.Length; i++)
        {
            var raw = TemplateSampleRow[i];
            // Cột tiền/số ghi dạng số để Excel không cảnh báo "number stored as text".
            if (decimal.TryParse(raw, System.Globalization.NumberStyles.Any,
                    CultureInfo.InvariantCulture, out var num))
                ws.Cell(2, i + 1).Value = num;
            else
                ws.Cell(2, i + 1).Value = raw;
        }

        ws.Columns().AdjustToContents();

        using var ms = new MemoryStream();
        wb.SaveAs(ms);
        return ms.ToArray();
    }
}
