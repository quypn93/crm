namespace CRM.Application.DTOs.Lookup;

public class LookupItemDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsActive { get; set; } = true;
}

public class CreateLookupItemDto
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsActive { get; set; } = true;
}

public class UpdateLookupItemDto : CreateLookupItemDto
{
    public Guid Id { get; set; }
}

public class CollectionDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsActive { get; set; } = true;
    public List<Guid> MaterialIds { get; set; } = new();
    public List<Guid> ColorFabricIds { get; set; } = new();
    public List<Guid> FormIds { get; set; } = new();
    public List<Guid> SpecificationIds { get; set; } = new();
}

public class CreateCollectionDto
{
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsActive { get; set; } = true;
    public List<Guid> MaterialIds { get; set; } = new();
    public List<Guid> ColorFabricIds { get; set; } = new();
    public List<Guid> FormIds { get; set; } = new();
    public List<Guid> SpecificationIds { get; set; } = new();
}

public class UpdateCollectionDto : CreateCollectionDto
{
    public Guid Id { get; set; }
}

public class ProductionDaysOptionDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int Days { get; set; }
    public bool IsActive { get; set; } = true;
}

public class CreateProductionDaysOptionDto
{
    public string Name { get; set; } = string.Empty;
    public int Days { get; set; }
    public bool IsActive { get; set; } = true;
}

public class UpdateProductionDaysOptionDto : CreateProductionDaysOptionDto
{
    public Guid Id { get; set; }
}

public class DepositTransactionDto
{
    public Guid Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string BankName { get; set; } = "Techcombank";
    public string? AccountNumber { get; set; }
    public string? Description { get; set; }
    public DateTime TransactionDate { get; set; }
    public string Source { get; set; } = "manual";
    public string? ExternalId { get; set; }
    public Guid? MatchedOrderId { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class CreateDepositTransactionDto
{
    public string Code { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string BankName { get; set; } = "Techcombank";
    public string? AccountNumber { get; set; }
    public string? Description { get; set; }
    public DateTime TransactionDate { get; set; } = DateTime.UtcNow;
}
