using CRM.Application.DTOs.Common;
using CRM.Application.DTOs.Design;

namespace CRM.Application.Interfaces;

public interface IDesignService
{
    Task<DesignDetailDto?> GetByIdAsync(Guid id);
    Task<PaginatedResult<DesignDto>> GetPagedAsync(DesignFilterDto filter);
    Task<IEnumerable<DesignDto>> GetByOrderAsync(Guid orderId);
    Task<IEnumerable<DesignDto>> GetByUserAsync(Guid userId);
    Task<DesignDto> CreateAsync(CreateDesignDto dto, Guid userId);
    Task<DesignDto> UpdateAsync(UpdateDesignDto dto, Guid userId);
    Task DeleteAsync(Guid id, Guid userId);
    Task<DesignDto> DuplicateAsync(Guid id, DuplicateDesignDto dto, Guid userId);
}
