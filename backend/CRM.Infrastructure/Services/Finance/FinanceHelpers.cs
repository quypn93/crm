using CRM.Core.Enums;

namespace CRM.Infrastructure.Services.Finance;

internal static class FinanceHelpers
{
    /// <summary>Giờ VN — dùng khi quy đổi mốc UTC trong DB về tháng dương lịch của kế toán.</summary>
    public static readonly TimeZoneInfo VnTimeZone = TimeZoneInfo.FindSystemTimeZoneById(
        OperatingSystem.IsWindows() ? "SE Asia Standard Time" : "Asia/Ho_Chi_Minh");

    public static DateTime ToVn(DateTime utc) =>
        TimeZoneInfo.ConvertTimeFromUtc(
            utc.Kind == DateTimeKind.Utc ? utc : DateTime.SpecifyKind(utc, DateTimeKind.Utc),
            VnTimeZone);

    /// <summary>Đầu ngày giờ VN → UTC, để so sánh với cột timestamptz.</summary>
    public static DateTime VnDateToUtc(int year, int month, int day) =>
        TimeZoneInfo.ConvertTimeToUtc(new DateTime(year, month, day, 0, 0, 0, DateTimeKind.Unspecified), VnTimeZone);

    /// <summary>Chuẩn hóa tham số mốc ghi nhận doanh thu. Mặc định: ngày xác nhận đơn.</summary>
    public static string NormalizeBasis(string? basis) => (basis ?? "confirmed").Trim().ToLowerInvariant() switch
    {
        "order" => "order",
        "completed" => "completed",
        "delivered" => "delivered",
        _ => "confirmed"
    };

    public static string StatusName(OrderStatus status) => status switch
    {
        OrderStatus.Draft => "Nháp",
        OrderStatus.Confirmed => "Đã xác nhận",
        OrderStatus.InProduction => "Đang sản xuất",
        OrderStatus.QualityCheck => "Kiểm tra chất lượng",
        OrderStatus.ReadyToShip => "Sẵn sàng giao",
        OrderStatus.Shipping => "Đang giao hàng",
        OrderStatus.Delivered => "Đã giao",
        OrderStatus.Completed => "Hoàn thành",
        OrderStatus.Cancelled => "Đã hủy",
        _ => "Không xác định"
    };

    public static decimal Margin(decimal revenue, decimal profit) =>
        revenue > 0 ? Math.Round(profit / revenue * 100, 2) : 0;
}
