using CRM.Core.Entities;

namespace CRM.Core.Interfaces.Repositories;

public interface IDealRepository : IRepository<Deal>
{
    Task<Deal?> GetByIdWithDetailsAsync(Guid id);
    Task<(IEnumerable<Deal> Items, int TotalCount)> GetPagedAsync(
        string? search,
        Guid? stageId,
        Guid? customerId,
        Guid? assignedTo,
        decimal? minValue,
        decimal? maxValue,
        DateTime? closeDateFrom,
        DateTime? closeDateTo,
        int page,
        int pageSize,
        string? sortBy,
        string sortOrder);
    Task<IEnumerable<Deal>> GetByCustomerAsync(Guid customerId);
    Task<IEnumerable<Deal>> GetByStageAsync(Guid stageId);
    Task<IEnumerable<DealStage>> GetAllStagesAsync();
    Task<DealStage?> GetDefaultStageAsync();
    Task<DealStage?> GetWonStageAsync();
    Task<DealStage?> GetLostStageAsync();
    Task<decimal> GetTotalRevenueAsync(DateTime? from, DateTime? to);
    Task<IEnumerable<(DealStage Stage, int Count, decimal TotalValue)>> GetDealsByStageAsync();
}
