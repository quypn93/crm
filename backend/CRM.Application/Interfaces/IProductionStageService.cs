using CRM.Application.DTOs.Production;

namespace CRM.Application.Interfaces;

public interface IProductionStageService
{
    Task<IEnumerable<ProductionStageDto>> GetAllActiveAsync();
    Task<IEnumerable<ProductionStageDto>> GetAllAsync();
    Task<ProductionStageDto?> GetByIdAsync(Guid id);
    Task<ProductionStageDto> CreateAsync(CreateProductionStageDto dto);
    Task<ProductionStageDto> UpdateAsync(Guid id, UpdateProductionStageDto dto);
    Task DeleteAsync(Guid id);
    Task ReorderAsync(ReorderProductionStagesDto dto);
}
