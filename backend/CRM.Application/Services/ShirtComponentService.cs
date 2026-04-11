using AutoMapper;
using CRM.Application.DTOs.Common;
using CRM.Application.DTOs.Design;
using CRM.Application.Interfaces;
using CRM.Core.Entities;
using CRM.Core.Enums;
using CRM.Core.Interfaces;

namespace CRM.Application.Services;

public class ShirtComponentService : IShirtComponentService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public ShirtComponentService(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<ShirtComponentDto?> GetByIdAsync(Guid id)
    {
        var component = await _unitOfWork.ShirtComponents.GetByIdAsync(id);
        return component != null ? _mapper.Map<ShirtComponentDto>(component) : null;
    }

    public async Task<PaginatedResult<ShirtComponentDto>> GetPagedAsync(ShirtComponentFilterDto filter)
    {
        var (items, totalCount) = await _unitOfWork.ShirtComponents.GetPagedAsync(
            filter.Search,
            filter.Type,
            filter.ColorFabricId,
            filter.IncludeDeleted,
            filter.Page,
            filter.PageSize,
            filter.SortBy,
            filter.SortOrder);

        var dtos = _mapper.Map<List<ShirtComponentDto>>(items);
        return PaginatedResult<ShirtComponentDto>.Create(dtos, totalCount, filter.Page, filter.PageSize);
    }

    public async Task<IEnumerable<ShirtComponentDto>> GetByTypeAsync(ComponentType type)
    {
        var components = await _unitOfWork.ShirtComponents.GetByTypeAsync(type);
        return _mapper.Map<IEnumerable<ShirtComponentDto>>(components);
    }

    public async Task<IEnumerable<ShirtComponentDto>> GetActiveByTypeAsync(ComponentType type)
    {
        var components = await _unitOfWork.ShirtComponents.GetActiveByTypeAsync(type);
        return _mapper.Map<IEnumerable<ShirtComponentDto>>(components);
    }

    public async Task<IEnumerable<ShirtComponentDto>> GetByColorFabricIdAsync(Guid colorFabricId)
    {
        var components = await _unitOfWork.ShirtComponents.GetByColorFabricIdAsync(colorFabricId);
        return _mapper.Map<IEnumerable<ShirtComponentDto>>(components);
    }

    public async Task<ShirtComponentDto> CreateAsync(CreateShirtComponentDto dto)
    {
        var component = _mapper.Map<ShirtComponent>(dto);
        await _unitOfWork.ShirtComponents.AddAsync(component);
        await _unitOfWork.SaveChangesAsync();

        return _mapper.Map<ShirtComponentDto>(component);
    }

    public async Task<ShirtComponentDto> UpdateAsync(UpdateShirtComponentDto dto)
    {
        var component = await _unitOfWork.ShirtComponents.GetByIdAsync(dto.Id);
        if (component == null)
        {
            throw new KeyNotFoundException("Không tìm thấy thành phần áo.");
        }

        _mapper.Map(dto, component);
        _unitOfWork.ShirtComponents.Update(component);
        await _unitOfWork.SaveChangesAsync();

        return _mapper.Map<ShirtComponentDto>(component);
    }

    public async Task DeleteAsync(Guid id)
    {
        var component = await _unitOfWork.ShirtComponents.GetByIdAsync(id);
        if (component == null)
        {
            throw new KeyNotFoundException("Không tìm thấy thành phần áo.");
        }

        // Soft delete
        component.IsDeleted = true;
        _unitOfWork.ShirtComponents.Update(component);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task RestoreAsync(Guid id)
    {
        var component = await _unitOfWork.ShirtComponents.GetByIdAsync(id);
        if (component == null)
        {
            throw new KeyNotFoundException("Không tìm thấy thành phần áo.");
        }

        component.IsDeleted = false;
        _unitOfWork.ShirtComponents.Update(component);
        await _unitOfWork.SaveChangesAsync();
    }
}
