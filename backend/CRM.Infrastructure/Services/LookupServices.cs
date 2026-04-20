using CRM.Application.DTOs.Lookup;
using CRM.Application.Interfaces;
using CRM.Core.Entities;
using CRM.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace CRM.Infrastructure.Services;

public class CollectionService : ICollectionService
{
    private readonly CrmDbContext _db;

    public CollectionService(CrmDbContext db)
    {
        _db = db;
    }

    public async Task<IEnumerable<CollectionDto>> GetAllAsync()
    {
        var list = await _db.Collections
            .Include(c => c.Materials)
            .Include(c => c.Colors)
            .Include(c => c.Forms)
            .Include(c => c.Specifications)
            .OrderBy(c => c.Name)
            .ToListAsync();

        return list.Select(ToDto);
    }

    public async Task<CollectionDto?> GetByIdAsync(Guid id)
    {
        var c = await _db.Collections
            .Include(c => c.Materials)
            .Include(c => c.Colors)
            .Include(c => c.Forms)
            .Include(c => c.Specifications)
            .FirstOrDefaultAsync(x => x.Id == id);
        return c == null ? null : ToDto(c);
    }

    public async Task<CollectionDto> CreateAsync(CreateCollectionDto dto)
    {
        var entity = new Collection
        {
            Name = dto.Name,
            Description = dto.Description,
            IsActive = dto.IsActive
        };
        foreach (var id in dto.MaterialIds) entity.Materials.Add(new CollectionMaterial { MaterialId = id });
        foreach (var id in dto.ColorFabricIds) entity.Colors.Add(new CollectionColor { ColorFabricId = id });
        foreach (var id in dto.FormIds) entity.Forms.Add(new CollectionForm { ProductFormId = id });
        foreach (var id in dto.SpecificationIds) entity.Specifications.Add(new CollectionSpecification { ProductSpecificationId = id });

        _db.Collections.Add(entity);
        await _db.SaveChangesAsync();
        return (await GetByIdAsync(entity.Id))!;
    }

    public async Task<CollectionDto> UpdateAsync(UpdateCollectionDto dto)
    {
        var entity = await _db.Collections
            .Include(c => c.Materials)
            .Include(c => c.Colors)
            .Include(c => c.Forms)
            .Include(c => c.Specifications)
            .FirstOrDefaultAsync(c => c.Id == dto.Id)
            ?? throw new KeyNotFoundException("Không tìm thấy bộ sưu tập.");

        entity.Name = dto.Name;
        entity.Description = dto.Description;
        entity.IsActive = dto.IsActive;

        entity.Materials.Clear();
        entity.Colors.Clear();
        entity.Forms.Clear();
        entity.Specifications.Clear();
        foreach (var id in dto.MaterialIds) entity.Materials.Add(new CollectionMaterial { CollectionId = entity.Id, MaterialId = id });
        foreach (var id in dto.ColorFabricIds) entity.Colors.Add(new CollectionColor { CollectionId = entity.Id, ColorFabricId = id });
        foreach (var id in dto.FormIds) entity.Forms.Add(new CollectionForm { CollectionId = entity.Id, ProductFormId = id });
        foreach (var id in dto.SpecificationIds) entity.Specifications.Add(new CollectionSpecification { CollectionId = entity.Id, ProductSpecificationId = id });

        await _db.SaveChangesAsync();
        return (await GetByIdAsync(entity.Id))!;
    }

    public async Task DeleteAsync(Guid id)
    {
        var entity = await _db.Collections.FindAsync(id) ?? throw new KeyNotFoundException();
        _db.Collections.Remove(entity);
        await _db.SaveChangesAsync();
    }

    private static CollectionDto ToDto(Collection c) => new()
    {
        Id = c.Id,
        Name = c.Name,
        Description = c.Description,
        IsActive = c.IsActive,
        MaterialIds = c.Materials.Select(m => m.MaterialId).ToList(),
        ColorFabricIds = c.Colors.Select(m => m.ColorFabricId).ToList(),
        FormIds = c.Forms.Select(m => m.ProductFormId).ToList(),
        SpecificationIds = c.Specifications.Select(m => m.ProductSpecificationId).ToList()
    };
}

public abstract class SimpleLookupServiceBase<TEntity> where TEntity : BaseEntity, new()
{
    protected readonly CrmDbContext _db;
    protected SimpleLookupServiceBase(CrmDbContext db) { _db = db; }

    protected abstract DbSet<TEntity> DbSet { get; }
    protected abstract void SetFields(TEntity e, string name, string? description, bool isActive);
    protected abstract LookupItemDto ToDto(TEntity e);

    public async Task<IEnumerable<LookupItemDto>> GetAllAsync()
    {
        var list = await DbSet.ToListAsync();
        return list.Select(ToDto);
    }

    public async Task<LookupItemDto> CreateAsync(CreateLookupItemDto dto)
    {
        var e = new TEntity();
        SetFields(e, dto.Name, dto.Description, dto.IsActive);
        DbSet.Add(e);
        await _db.SaveChangesAsync();
        return ToDto(e);
    }

    public async Task<LookupItemDto> UpdateAsync(UpdateLookupItemDto dto)
    {
        var e = await DbSet.FindAsync(dto.Id) ?? throw new KeyNotFoundException();
        SetFields(e, dto.Name, dto.Description, dto.IsActive);
        await _db.SaveChangesAsync();
        return ToDto(e);
    }

    public async Task DeleteAsync(Guid id)
    {
        var e = await DbSet.FindAsync(id) ?? throw new KeyNotFoundException();
        DbSet.Remove(e);
        await _db.SaveChangesAsync();
    }
}

public class MaterialService : SimpleLookupServiceBase<Material>, IMaterialService
{
    public MaterialService(CrmDbContext db) : base(db) { }
    protected override DbSet<Material> DbSet => _db.Materials;
    protected override void SetFields(Material e, string name, string? description, bool isActive)
    { e.Name = name; e.Description = description; e.IsActive = isActive; }
    protected override LookupItemDto ToDto(Material e) => new() { Id = e.Id, Name = e.Name, Description = e.Description, IsActive = e.IsActive };
}

public class ProductFormService : SimpleLookupServiceBase<ProductForm>, IProductFormService
{
    public ProductFormService(CrmDbContext db) : base(db) { }
    protected override DbSet<ProductForm> DbSet => _db.ProductForms;
    protected override void SetFields(ProductForm e, string name, string? description, bool isActive)
    { e.Name = name; e.Description = description; e.IsActive = isActive; }
    protected override LookupItemDto ToDto(ProductForm e) => new() { Id = e.Id, Name = e.Name, Description = e.Description, IsActive = e.IsActive };
}

public class ProductSpecificationService : SimpleLookupServiceBase<ProductSpecification>, IProductSpecificationService
{
    public ProductSpecificationService(CrmDbContext db) : base(db) { }
    protected override DbSet<ProductSpecification> DbSet => _db.ProductSpecifications;
    protected override void SetFields(ProductSpecification e, string name, string? description, bool isActive)
    { e.Name = name; e.Description = description; e.IsActive = isActive; }
    protected override LookupItemDto ToDto(ProductSpecification e) => new() { Id = e.Id, Name = e.Name, Description = e.Description, IsActive = e.IsActive };
}

public class ProductionDaysOptionService : IProductionDaysOptionService
{
    private readonly CrmDbContext _db;
    public ProductionDaysOptionService(CrmDbContext db) { _db = db; }

    public async Task<IEnumerable<ProductionDaysOptionDto>> GetAllAsync()
    {
        var list = await _db.ProductionDaysOptions.OrderBy(x => x.Days).ToListAsync();
        return list.Select(ToDto);
    }

    public async Task<ProductionDaysOptionDto> CreateAsync(CreateProductionDaysOptionDto dto)
    {
        var e = new ProductionDaysOption { Name = dto.Name, Days = dto.Days, IsActive = dto.IsActive };
        _db.ProductionDaysOptions.Add(e);
        await _db.SaveChangesAsync();
        return ToDto(e);
    }

    public async Task<ProductionDaysOptionDto> UpdateAsync(UpdateProductionDaysOptionDto dto)
    {
        var e = await _db.ProductionDaysOptions.FindAsync(dto.Id) ?? throw new KeyNotFoundException();
        e.Name = dto.Name;
        e.Days = dto.Days;
        e.IsActive = dto.IsActive;
        await _db.SaveChangesAsync();
        return ToDto(e);
    }

    public async Task DeleteAsync(Guid id)
    {
        var e = await _db.ProductionDaysOptions.FindAsync(id) ?? throw new KeyNotFoundException();
        _db.ProductionDaysOptions.Remove(e);
        await _db.SaveChangesAsync();
    }

    private static ProductionDaysOptionDto ToDto(ProductionDaysOption e) => new()
    {
        Id = e.Id, Name = e.Name, Days = e.Days, IsActive = e.IsActive
    };
}

public class DepositTransactionService : IDepositTransactionService
{
    private readonly CrmDbContext _db;
    public DepositTransactionService(CrmDbContext db) { _db = db; }

    public async Task<IEnumerable<DepositTransactionDto>> GetAllAsync()
    {
        var list = await _db.DepositTransactions
            .OrderByDescending(x => x.TransactionDate)
            .ToListAsync();
        return list.Select(ToDto);
    }

    public async Task<DepositTransactionDto> CreateAsync(CreateDepositTransactionDto dto)
    {
        var e = new DepositTransaction
        {
            Code = dto.Code,
            Amount = dto.Amount,
            BankName = dto.BankName,
            AccountNumber = dto.AccountNumber,
            Description = dto.Description,
            TransactionDate = dto.TransactionDate,
            Source = "manual"
        };
        _db.DepositTransactions.Add(e);
        await _db.SaveChangesAsync();
        return ToDto(e);
    }

    public async Task<DepositTransactionDto> HandleSePayWebhookAsync(SePayWebhookPayload payload)
    {
        // Chỉ quan tâm giao dịch tiền vào
        if (!string.Equals(payload.TransferType, "in", StringComparison.OrdinalIgnoreCase))
            return new DepositTransactionDto();

        var externalId = payload.Id.ToString();
        var exists = await _db.DepositTransactions.AnyAsync(x => x.ExternalId == externalId);
        if (exists) return new DepositTransactionDto();

        var e = new DepositTransaction
        {
            // Ưu tiên `code` (SePay tự parse từ nội dung CK), fallback sang referenceCode
            Code = !string.IsNullOrWhiteSpace(payload.Code) ? payload.Code! : (payload.ReferenceCode ?? externalId),
            Amount = payload.TransferAmount,
            BankName = payload.Gateway ?? "Techcombank",
            AccountNumber = payload.AccountNumber,
            Description = payload.Content ?? payload.Description,
            TransactionDate = DateTime.TryParse(payload.TransactionDate, out var t) ? t : DateTime.UtcNow,
            Source = "sepay",
            ExternalId = externalId
        };
        _db.DepositTransactions.Add(e);
        await _db.SaveChangesAsync();
        return ToDto(e);
    }

    public async Task DeleteAsync(Guid id)
    {
        var e = await _db.DepositTransactions.FindAsync(id) ?? throw new KeyNotFoundException();
        _db.DepositTransactions.Remove(e);
        await _db.SaveChangesAsync();
    }

    private static DepositTransactionDto ToDto(DepositTransaction e) => new()
    {
        Id = e.Id,
        Code = e.Code,
        Amount = e.Amount,
        BankName = e.BankName,
        AccountNumber = e.AccountNumber,
        Description = e.Description,
        TransactionDate = e.TransactionDate,
        Source = e.Source,
        ExternalId = e.ExternalId,
        MatchedOrderId = e.MatchedOrderId,
        CreatedAt = e.CreatedAt
    };
}
