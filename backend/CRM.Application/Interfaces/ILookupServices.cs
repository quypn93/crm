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
    Task<DepositTransactionDto> HandleSePayWebhookAsync(SePayWebhookPayload payload);
    Task DeleteAsync(Guid id);
}

// SePay webhook shape. Docs: https://docs.sepay.vn/tich-hop-webhooks.html
public class SePayWebhookPayload
{
    public long Id { get; set; }
    public string? Gateway { get; set; }               // Ngân hàng, vd "Techcombank"
    public string? TransactionDate { get; set; }       // "yyyy-MM-dd HH:mm:ss"
    public string? AccountNumber { get; set; }
    public string? Code { get; set; }                  // Mã code tự parse từ nội dung
    public string? Content { get; set; }               // Nội dung chuyển khoản
    public string? TransferType { get; set; }          // "in" | "out"
    public decimal TransferAmount { get; set; }
    public decimal? Accumulated { get; set; }          // Số dư sau giao dịch
    public string? SubAccount { get; set; }
    public string? ReferenceCode { get; set; }         // Mã tham chiếu ngân hàng (FT...)
    public string? Description { get; set; }
}
