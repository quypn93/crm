namespace CRM.Core.Enums;

public enum PaymentStatus
{
    Pending = 0,         // Chờ thanh toán
    PartialPaid = 1,     // Thanh toán một phần
    Paid = 2,            // Đã thanh toán
    Refunded = 3,        // Đã hoàn tiền
    Cancelled = 4        // Đã hủy
}
