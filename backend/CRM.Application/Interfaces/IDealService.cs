using CRM.Application.DTOs.Common;
using CRM.Application.DTOs.Deal;

namespace CRM.Application.Interfaces;

public interface IDealService
{
    Task<DealDto?> GetByIdAsync(Guid id);
    Task<PaginatedResult<DealDto>> GetPagedAsync(DealFilterDto filter);
    Task<DealDto> CreateAsync(CreateDealDto dto, Guid userId);
    Task<DealDto> UpdateAsync(UpdateDealDto dto, Guid userId);
    Task DeleteAsync(Guid id, Guid userId);
    Task<DealDto> UpdateStageAsync(UpdateDealStageDto dto, Guid userId);
    Task<DealDto> MarkAsWonAsync(Guid dealId, MarkDealWonDto dto, Guid userId);
    Task<DealDto> MarkAsLostAsync(Guid dealId, MarkDealLostDto dto, Guid userId);
    Task<IEnumerable<DealStageDto>> GetAllStagesAsync();
    Task<IEnumerable<DealsByStageDto>> GetDealsByStageAsync();
    Task<IEnumerable<DealDto>> GetByCustomerAsync(Guid customerId);
}
