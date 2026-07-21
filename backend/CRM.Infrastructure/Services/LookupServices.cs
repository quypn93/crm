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

public class SenderAddressService : ISenderAddressService
{
    private readonly CrmDbContext _db;

    public SenderAddressService(CrmDbContext db)
    {
        _db = db;
    }

    public async Task<IEnumerable<SenderAddressDto>> GetAllAsync()
    {
        var list = await _db.SenderAddresses
            .Include(x => x.AssignedUser)
            .OrderByDescending(x => x.IsDefault).ThenBy(x => x.Name)
            .ToListAsync();
        return list.Select(ToDto);
    }

    public async Task<SenderAddressDto?> GetByIdAsync(Guid id)
    {
        var x = await _db.SenderAddresses.Include(a => a.AssignedUser).FirstOrDefaultAsync(a => a.Id == id);
        return x == null ? null : ToDto(x);
    }

    public async Task<SenderAddressDto> CreateAsync(CreateSenderAddressDto dto)
    {
        var entity = new SenderAddress
        {
            Name = dto.Name.Trim(),
            Phone = dto.Phone.Trim(),
            Address = dto.Address.Trim(),
            ProvinceId = dto.ProvinceId,
            DistrictId = dto.DistrictId,
            WardId = dto.WardId,
            ProvinceName = dto.ProvinceName,
            DistrictName = dto.DistrictName,
            WardName = dto.WardName,
            IsDefault = dto.IsDefault,
            IsActive = dto.IsActive,
            AssignedUserId = dto.AssignedUserId
        };
        _db.SenderAddresses.Add(entity);
        await _db.SaveChangesAsync();
        await EnsureSingleDefaultAsync(entity);
        return ToDto(entity);
    }

    public async Task<SenderAddressDto> UpdateAsync(UpdateSenderAddressDto dto)
    {
        var entity = await _db.SenderAddresses.FindAsync(dto.Id)
            ?? throw new KeyNotFoundException("Không tìm thấy địa chỉ gửi hàng.");

        entity.Name = dto.Name.Trim();
        entity.Phone = dto.Phone.Trim();
        entity.Address = dto.Address.Trim();
        entity.ProvinceId = dto.ProvinceId;
        entity.DistrictId = dto.DistrictId;
        entity.WardId = dto.WardId;
        entity.ProvinceName = dto.ProvinceName;
        entity.DistrictName = dto.DistrictName;
        entity.WardName = dto.WardName;
        entity.IsDefault = dto.IsDefault;
        entity.IsActive = dto.IsActive;
        entity.AssignedUserId = dto.AssignedUserId;

        await _db.SaveChangesAsync();
        await EnsureSingleDefaultAsync(entity);
        // Reload navigation để trả AssignedUserName mới.
        var reloaded = await _db.SenderAddresses.Include(a => a.AssignedUser).FirstOrDefaultAsync(a => a.Id == entity.Id);
        return ToDto(reloaded ?? entity);
    }

    public async Task DeleteAsync(Guid id)
    {
        var entity = await _db.SenderAddresses.FindAsync(id) ?? throw new KeyNotFoundException();
        _db.SenderAddresses.Remove(entity);
        await _db.SaveChangesAsync();
    }

    // Chỉ 1 địa chỉ được đặt mặc định: nếu entity vừa lưu là default thì bỏ default các cái khác.
    private async Task EnsureSingleDefaultAsync(SenderAddress entity)
    {
        if (!entity.IsDefault) return;
        var others = await _db.SenderAddresses.Where(x => x.Id != entity.Id && x.IsDefault).ToListAsync();
        if (others.Count == 0) return;
        foreach (var o in others) o.IsDefault = false;
        await _db.SaveChangesAsync();
    }

    private static SenderAddressDto ToDto(SenderAddress x) => new()
    {
        Id = x.Id,
        Name = x.Name,
        Phone = x.Phone,
        Address = x.Address,
        ProvinceId = x.ProvinceId,
        DistrictId = x.DistrictId,
        WardId = x.WardId,
        ProvinceName = x.ProvinceName,
        DistrictName = x.DistrictName,
        WardName = x.WardName,
        IsDefault = x.IsDefault,
        IsActive = x.IsActive,
        AssignedUserId = x.AssignedUserId,
        AssignedUserName = x.AssignedUser != null ? $"{x.AssignedUser.FirstName} {x.AssignedUser.LastName}".Trim() : null
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

public class OrderTypeService : SimpleLookupServiceBase<OrderType>, IOrderTypeService
{
    public OrderTypeService(CrmDbContext db) : base(db) { }
    protected override DbSet<OrderType> DbSet => _db.OrderTypes;
    protected override void SetFields(OrderType e, string name, string? description, bool isActive)
    { e.Name = name; e.Description = description; e.IsActive = isActive; }
    protected override LookupItemDto ToDto(OrderType e) => new() { Id = e.Id, Name = e.Name, Description = e.Description, IsActive = e.IsActive };
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
        var parentIds = list.Where(x => x.ParentId.HasValue).Select(x => x.ParentId!.Value).ToHashSet();
        return list.Select(e => ToDto(e, parentIds.Contains(e.Id)));
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

    public async Task<int> HandleCassoWebhookAsync(CassoWebhookPayload payload)
    {
        if (payload.Data == null || payload.Data.Count == 0) return 0;

        // Casso gửi cả tiền vào (amount > 0) lẫn tiền ra (amount < 0) — chỉ giữ tiền vào
        var incoming = payload.Data.Where(t => t.Amount > 0).ToList();
        if (incoming.Count == 0) return 0;

        // Lọc các giao dịch đã lưu trước đó (dedupe theo Id của Casso)
        var externalIds = incoming.Select(t => t.Id.ToString()).ToList();
        var existing = await _db.DepositTransactions
            .Where(x => x.ExternalId != null && externalIds.Contains(x.ExternalId))
            .Select(x => x.ExternalId!)
            .ToListAsync();
        var existingSet = existing.ToHashSet();

        var added = 0;
        foreach (var t in incoming)
        {
            var externalId = t.Id.ToString();
            if (existingSet.Contains(externalId)) continue;
            existingSet.Add(externalId); // tránh trùng ngay trong cùng 1 batch

            _db.DepositTransactions.Add(new DepositTransaction
            {
                // Casso không tách sẵn mã đơn — dùng mã tham chiếu ngân hàng (tid) làm Mã GD,
                // mã khách gõ (nếu có) nằm trong Description để sale đối chiếu.
                Code = !string.IsNullOrWhiteSpace(t.Tid) ? t.Tid! : externalId,
                Amount = t.Amount,
                BankName = t.BankAbbreviation ?? t.BankName ?? string.Empty,
                AccountNumber = t.SubAccId ?? t.BankSubAccId,
                Description = ExtractRealContent(t.Description),
                TransactionDate = ParseCassoDateUtc(t.When),
                Source = "casso",
                ExternalId = externalId
            });
            added++;
        }

        if (added > 0) await _db.SaveChangesAsync();
        return added;
    }

    // Tách 1 giao dịch gốc (khách cọc gộp) thành nhiều khoản con để gắn vào nhiều đơn.
    // Khoản con mang mã "<mã gốc>-1", "-2"... Giao dịch gốc giữ lại đối soát, hết claim được.
    public async Task<IEnumerable<DepositTransactionDto>> SplitAsync(Guid id, SplitDepositDto dto)
    {
        var parent = await _db.DepositTransactions.FindAsync(id)
            ?? throw new KeyNotFoundException("Không tìm thấy giao dịch.");

        if (parent.MatchedOrderId.HasValue)
            throw new InvalidOperationException("Giao dịch đã gắn vào đơn hàng — gỡ mã khỏi đơn trước khi tách.");

        if (await _db.DepositTransactions.AnyAsync(x => x.ParentId == parent.Id))
            throw new InvalidOperationException("Giao dịch này đã được tách rồi.");

        var amounts = dto.Amounts.Where(a => a != 0).ToList();
        if (amounts.Count < 2)
            throw new InvalidOperationException("Cần ít nhất 2 khoản để tách.");
        if (amounts.Any(a => a <= 0))
            throw new InvalidOperationException("Số tiền mỗi khoản phải lớn hơn 0.");
        if (amounts.Sum() != parent.Amount)
            throw new InvalidOperationException(
                $"Tổng các khoản ({amounts.Sum():N0} đ) phải đúng bằng số tiền gốc ({parent.Amount:N0} đ).");

        // Tránh đụng mã đã tồn tại (ví dụ tách lại sau khi xóa khoản con cũ, hoặc mã trùng).
        var usedCodes = (await _db.DepositTransactions
                .Where(x => x.Code.StartsWith(parent.Code + "-"))
                .Select(x => x.Code)
                .ToListAsync())
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        var children = new List<DepositTransaction>();
        var index = 1;
        foreach (var amount in amounts)
        {
            while (usedCodes.Contains($"{parent.Code}-{index}")) index++;
            var code = $"{parent.Code}-{index}";
            usedCodes.Add(code);

            children.Add(new DepositTransaction
            {
                Code = code,
                Amount = amount,
                BankName = parent.BankName,
                AccountNumber = parent.AccountNumber,
                Description = string.IsNullOrWhiteSpace(parent.Description)
                    ? $"Tách từ {parent.Code}"
                    : $"{parent.Description} (tách từ {parent.Code})",
                TransactionDate = parent.TransactionDate,
                Source = parent.Source,
                ExternalId = null,          // giữ ExternalId ở bản gốc để webhook Casso vẫn dedupe được
                ParentId = parent.Id
            });
        }

        _db.DepositTransactions.AddRange(children);
        await _db.SaveChangesAsync();
        return children.Select(c => ToDto(c, false));
    }

    public async Task DeleteAsync(Guid id)
    {
        var e = await _db.DepositTransactions.FindAsync(id) ?? throw new KeyNotFoundException();
        if (await _db.DepositTransactions.AnyAsync(x => x.ParentId == e.Id))
            throw new InvalidOperationException("Giao dịch đã tách thành các khoản con — xóa các khoản con trước.");
        _db.DepositTransactions.Remove(e);
        await _db.SaveChangesAsync();
    }

    private static DepositTransactionDto ToDto(DepositTransaction e, bool isSplit = false) => new()
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
        CreatedAt = e.CreatedAt,
        ParentId = e.ParentId,
        IsSplit = isSplit
    };

    private static readonly TimeZoneInfo VnTimeZone = TimeZoneInfo.FindSystemTimeZoneById(
        OperatingSystem.IsWindows() ? "SE Asia Standard Time" : "Asia/Ho_Chi_Minh");

    // Casso gửi `when` dạng "yyyy-MM-dd HH:mm:ss" theo giờ Việt Nam, không kèm timezone.
    // PostgreSQL `timestamptz` chỉ chấp nhận DateTime.Kind=Utc nên phải convert.
    private static DateTime ParseCassoDateUtc(string? raw)
    {
        if (!DateTime.TryParse(raw, out var t)) return DateTime.UtcNow;
        return t.Kind == DateTimeKind.Unspecified
            ? TimeZoneInfo.ConvertTimeToUtc(t, VnTimeZone)
            : t.ToUniversalTime();
    }

    // BIDV/VCB hay bọc nội dung CK trong metadata, vd:
    //   MBVCB.13929087925.885563.PHAM NHU QUY chuyen tien.CT tu 1018590541 PHAM NHU QUY toi 96247KJUNZ PHAM
    // → phần khách thực sự gõ chỉ là "PHAM NHU QUY chuyen tien".
    private static readonly System.Text.RegularExpressions.Regex BankPrefixRegex =
        new(@"^[A-Z0-9]+(\.\d+)+\.", System.Text.RegularExpressions.RegexOptions.Compiled);

    private static string? ExtractRealContent(string? raw)
    {
        if (string.IsNullOrWhiteSpace(raw)) return raw;
        var s = raw;

        // Cắt suffix ".CT tu ..." / ".CT toi ..." (bank tự thêm thông tin from/to)
        var ctIdx = s.IndexOf(".CT ", StringComparison.OrdinalIgnoreCase);
        if (ctIdx > 0) s = s.Substring(0, ctIdx);

        // Cắt prefix dạng "MBVCB.13929087925.885563."
        var m = BankPrefixRegex.Match(s);
        if (m.Success) s = s.Substring(m.Length);

        s = s.Trim();
        return string.IsNullOrEmpty(s) ? raw : s;
    }
}
