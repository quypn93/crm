namespace CRM.Application.DTOs.Production;

public class OrderProductionStepDto
{
    public Guid Id { get; set; }
    public Guid OrderId { get; set; }
    public Guid ProductionStageId { get; set; }
    public int StageOrder { get; set; }
    public string StageName { get; set; } = string.Empty;
    public string? ResponsibleRole { get; set; }
    public bool IsCompleted { get; set; }
    public Guid? CompletedByUserId { get; set; }
    public string? CompletedByUserName { get; set; }
    public DateTime? CompletedAt { get; set; }
    public string? Notes { get; set; }
}

public class OrderProductionProgressDto
{
    public Guid OrderId { get; set; }
    public string OrderNumber { get; set; } = string.Empty;
    public string? CustomerName { get; set; }
    public int OrderStatus { get; set; }
    public int TotalSteps { get; set; }
    public int CompletedSteps { get; set; }
    public int ProgressPercent { get; set; }
    public string? CurrentStageName { get; set; }
    public bool IsFullyCompleted { get; set; }
    public List<OrderProductionStepDto> Steps { get; set; } = new();
}

public class CompleteProductionStepDto
{
    public string? Notes { get; set; }
}

// Xử lý khâu Vận đơn: người phụ trách chỉ CHỌN KHO GỬI rồi tạo vận đơn.
// Địa chỉ người nhận đã nhập lúc tạo đơn — không sửa ở đây.
public class ProcessWaybillDto
{
    public Guid? SenderAddressId { get; set; }          // Kho gửi (địa chỉ gửi hàng)
    public string? Notes { get; set; }                  // ghi chú hoàn tất khâu
}
