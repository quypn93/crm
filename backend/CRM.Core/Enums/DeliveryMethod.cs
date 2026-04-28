namespace CRM.Core.Enums;

public enum DeliveryMethod
{
    InHouse = 0,    // Nhà giao — nhân viên cửa hàng tự giao
    Vehicle = 1,    // Giao xe — khách hàng hoặc shipper riêng
    GHTK = 2        // Giao Hàng Tiết Kiệm (tích hợp ở Giai đoạn 3)
}
