using CRM.Application.DTOs.Common;
using CRM.Application.DTOs.Design;

namespace CRM.Application.Interfaces;

public interface IColorFabricService
{
    Task<ColorFabricDto?> GetByIdAsync(Guid id);
    Task<PaginatedResult<ColorFabricDto>> GetPagedAsync(ColorFabricFilterDto filter);
    Task<IEnumerable<ColorFabricDto>> GetAllAsync();
    Task<ColorFabricDto> CreateAsync(CreateColorFabricDto dto);
    Task<ColorFabricDto> UpdateAsync(UpdateColorFabricDto dto);
    Task DeleteAsync(Guid id);
}
