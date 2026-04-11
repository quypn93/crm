namespace CRM.Core.Enums;

public enum OrderStatus
{
    Draft = 0,           // Nháp
    Confirmed = 1,       // Đã xác nhận
    InProduction = 2,    // Đang sản xuất
    QualityCheck = 3,    // Kiểm tra chất lượng
    ReadyToShip = 4,     // Sẵn sàng giao
    Shipping = 5,        // Đang giao hàng
    Delivered = 6,       // Đã giao
    Completed = 7,       // Hoàn thành
    Cancelled = 8        // Đã hủy
}
