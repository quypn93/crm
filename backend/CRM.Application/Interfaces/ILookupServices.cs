using CRM.Application.DTOs.Lookup;

namespace CRM.Application.Interfaces;

public interface ICollectionService
{
    Task<IEnumerable<CollectionDto>> GetAllAsync();
    Task<CollectionDto?> GetByIdAsync(Guid id);
    Task<CollectionDto> CreateAsync(CreateCollectionDto dto);
    Task<CollectionDto> UpdateAsync(UpdateCollectionDto dto);
    Task DeleteAsync(Guid id);
}

public interface ISenderAddressService
{
    Task<IEnumerable<SenderAddressDto>> GetAllAsync();
    Task<SenderAddressDto?> GetByIdAsync(Guid id);
    Task<SenderAddressDto> CreateAsync(CreateSenderAddressDto dto);
    Task<SenderAddressDto> UpdateAsync(UpdateSenderAddressDto dto);
    Task DeleteAsync(Guid id);
}

public interface IMaterialService
{
    Task<IEnumerable<LookupItemDto>> GetAllAsync();
    Task<LookupItemDto> CreateAsync(CreateLookupItemDto dto);
    Task<LookupItemDto> UpdateAsync(UpdateLookupItemDto dto);
    Task DeleteAsync(Guid id);
}

public interface IProductFormService
{
    Task<IEnumerable<LookupItemDto>> GetAllAsync();
    Task<LookupItemDto> CreateAsync(CreateLookupItemDto dto);
    Task<LookupItemDto> UpdateAsync(UpdateLookupItemDto dto);
    Task DeleteAsync(Guid id);
}

public interface IProductSpecificationService
{
    Task<IEnumerable<LookupItemDto>> GetAllAsync();
    Task<LookupItemDto> CreateAsync(CreateLookupItemDto dto);
    Task<LookupItemDto> UpdateAsync(UpdateLookupItemDto dto);
    Task DeleteAsync(Guid id);
}

public interface IOrderTypeService
{
    Task<IEnumerable<LookupItemDto>> GetAllAsync();
    Task<LookupItemDto> CreateAsync(CreateLookupItemDto dto);
    Task<LookupItemDto> UpdateAsync(UpdateLookupItemDto dto);
    Task DeleteAsync(Guid id);
}

public interface IProductionDaysOptionService
{
    Task<IEnumerable<ProductionDaysOptionDto>> GetAllAsync();
    Task<ProductionDaysOptionDto> CreateAsync(CreateProductionDaysOptionDto dto);
    Task<ProductionDaysOptionDto> UpdateAsync(UpdateProductionDaysOptionDto dto);
    Task DeleteAsync(Guid id);
}

public interface IDepositTransactionService
{
    Task<IEnumerable<DepositTransactionDto>> GetAllAsync();
    Task<DepositTransactionDto> CreateAsync(CreateDepositTransactionDto dto);
    Task<IEnumerable<DepositTransactionDto>> SplitAsync(Guid id, SplitDepositDto dto);
    Task<int> HandleCassoWebhookAsync(CassoWebhookPayload payload);
    Task DeleteAsync(Guid id);
}

// Casso webhook shape (Secure-Token / legacy). Docs: https://developer.casso.vn/webhook/thiet-lap-webhook-thu-cong
// Casso gửi một envelope { "error": 0, "data": [ {...}, ... ] } — kể cả 1 giao dịch cũng nằm trong mảng.
public class CassoWebhookPayload
{
    public int Error { get; set; }
    public List<CassoTransaction> Data { get; set; } = new();
}

public class CassoTransaction
{
    public long Id { get; set; }                       // Id giao dịch của Casso (dùng dedupe)
    public string? Tid { get; set; }                   // Mã tham chiếu ngân hàng (FT...)
    public string? Description { get; set; }           // Nội dung chuyển khoản
    public decimal Amount { get; set; }                // > 0: tiền vào, < 0: tiền ra
    [System.Text.Json.Serialization.JsonPropertyName("cusum_balance")]
    public decimal? CusumBalance { get; set; }         // Số dư sau giao dịch
    public string? When { get; set; }                  // "yyyy-MM-dd HH:mm:ss" giờ VN
    [System.Text.Json.Serialization.JsonPropertyName("bank_sub_acc_id")]
    public string? BankSubAccId { get; set; }          // Số tài khoản nhận
    public string? SubAccId { get; set; }              // Số tài khoản nhận (bản mới)
    public string? BankName { get; set; }
    public string? BankAbbreviation { get; set; }      // vd "TCB", "VCB"
    public string? CorresponsiveName { get; set; }     // Tên người chuyển
    public string? CorresponsiveAccount { get; set; }  // STK người chuyển
    public string? CorresponsiveBankName { get; set; }
}
