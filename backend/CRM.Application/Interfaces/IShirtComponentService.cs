using CRM.Application.DTOs.Common;
using CRM.Application.DTOs.Design;
using CRM.Core.Enums;

namespace CRM.Application.Interfaces;

public interface IShirtComponentService
{
    Task<ShirtComponentDto?> GetByIdAsync(Guid id);
    Task<PaginatedResult<ShirtComponentDto>> GetPagedAsync(ShirtComponentFilterDto filter);
    Task<IEnumerable<ShirtComponentDto>> GetByTypeAsync(ComponentType type);
    Task<IEnumerable<ShirtComponentDto>> GetActiveByTypeAsync(ComponentType type);
    Task<IEnumerable<ShirtComponentDto>> GetByColorFabricIdAsync(Guid colorFabricId);
    Task<ShirtComponentDto> CreateAsync(CreateShirtComponentDto dto);
    Task<ShirtComponentDto> UpdateAsync(UpdateShirtComponentDto dto);
    Task DeleteAsync(Guid id); // Soft delete
    Task RestoreAsync(Guid id); // Restore soft deleted
}
