namespace CRM.Core.Entities;

public class DepositTransaction : BaseEntity
{
    public string Code { get; set; } = string.Empty;          // Mã giao dịch từ Casso / bank ref
    public decimal Amount { get; set; }
    public string BankName { get; set; } = string.Empty;
    public string? AccountNumber { get; set; }
    public string? Description { get; set; }                  // Nội dung chuyển khoản
    public DateTime TransactionDate { get; set; }
    public string Source { get; set; } = "manual";            // manual | sepay
    public string? ExternalId { get; set; }                   // Id từ webhook nhà cung cấp (dedupe)
    public Guid? MatchedOrderId { get; set; }                 // Order mà sale đã claim mã này
    public virtual Order? MatchedOrder { get; set; }
}
