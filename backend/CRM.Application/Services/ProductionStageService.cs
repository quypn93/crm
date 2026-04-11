using AutoMapper;
using CRM.Application.DTOs.Production;
using CRM.Application.Interfaces;
using CRM.Core.Entities;
using CRM.Core.Interfaces;

namespace CRM.Application.Services;

public class ProductionStageService : IProductionStageService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public ProductionStageService(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<IEnumerable<ProductionStageDto>> GetAllActiveAsync()
    {
        var stages = await _unitOfWork.ProductionStages.GetActiveStagesOrderedAsync();
        return _mapper.Map<IEnumerable<ProductionStageDto>>(stages);
    }

    public async Task<IEnumerable<ProductionStageDto>> GetAllAsync()
    {
        var stages = await _unitOfWork.ProductionStages.GetAllAsync();
        return _mapper.Map<IEnumerable<ProductionStageDto>>(stages.OrderBy(s => s.StageOrder));
    }

    public async Task<ProductionStageDto?> GetByIdAsync(Guid id)
    {
        var stage = await _unitOfWork.ProductionStages.GetByIdAsync(id);
        return stage == null ? null : _mapper.Map<ProductionStageDto>(stage);
    }

    public async Task<ProductionStageDto> CreateAsync(CreateProductionStageDto dto)
    {
        var stage = _mapper.Map<ProductionStage>(dto);
        await _unitOfWork.ProductionStages.AddAsync(stage);
        await _unitOfWork.SaveChangesAsync();
        return _mapper.Map<ProductionStageDto>(stage);
    }

    public async Task<ProductionStageDto> UpdateAsync(Guid id, UpdateProductionStageDto dto)
    {
        var stage = await _unitOfWork.ProductionStages.GetByIdAsync(id)
            ?? throw new KeyNotFoundException($"Không tìm thấy khâu sản xuất '{id}'.");

        _mapper.Map(dto, stage);
        _unitOfWork.ProductionStages.Update(stage);
        await _unitOfWork.SaveChangesAsync();
        return _mapper.Map<ProductionStageDto>(stage);
    }

    public async Task DeleteAsync(Guid id)
    {
        var stage = await _unitOfWork.ProductionStages.GetByIdAsync(id)
            ?? throw new KeyNotFoundException($"Không tìm thấy khâu sản xuất '{id}'.");

        _unitOfWork.ProductionStages.Remove(stage);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task ReorderAsync(ReorderProductionStagesDto dto)
    {
        foreach (var item in dto.Stages)
        {
            var stage = await _unitOfWork.ProductionStages.GetByIdAsync(item.Id);
            if (stage != null)
            {
                stage.StageOrder = item.NewOrder;
                _unitOfWork.ProductionStages.Update(stage);
            }
        }
        await _unitOfWork.SaveChangesAsync();
    }
}
