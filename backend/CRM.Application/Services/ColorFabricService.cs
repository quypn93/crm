using AutoMapper;
using CRM.Application.DTOs.Common;
using CRM.Application.DTOs.Design;
using CRM.Application.Interfaces;
using CRM.Core.Entities;
using CRM.Core.Interfaces;

namespace CRM.Application.Services;

public class ColorFabricService : IColorFabricService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public ColorFabricService(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<ColorFabricDto?> GetByIdAsync(Guid id)
    {
        var colorFabric = await _unitOfWork.ColorFabrics.GetByIdAsync(id);
        return colorFabric != null ? _mapper.Map<ColorFabricDto>(colorFabric) : null;
    }

    public async Task<PaginatedResult<ColorFabricDto>> GetPagedAsync(ColorFabricFilterDto filter)
    {
        var (items, totalCount) = await _unitOfWork.ColorFabrics.GetPagedAsync(
            filter.Search,
            filter.Page,
            filter.PageSize,
            filter.SortBy,
            filter.SortOrder);

        var dtos = _mapper.Map<List<ColorFabricDto>>(items);
        return PaginatedResult<ColorFabricDto>.Create(dtos, totalCount, filter.Page, filter.PageSize);
    }

    public async Task<IEnumerable<ColorFabricDto>> GetAllAsync()
    {
        var colorFabrics = await _unitOfWork.ColorFabrics.GetAllAsync();
        return _mapper.Map<IEnumerable<ColorFabricDto>>(colorFabrics);
    }

    public async Task<ColorFabricDto> CreateAsync(CreateColorFabricDto dto)
    {
        // Check if name already exists
        var existing = await _unitOfWork.ColorFabrics.GetByNameAsync(dto.Name);
        if (existing != null)
        {
            throw new InvalidOperationException($"Màu vải '{dto.Name}' đã tồn tại.");
        }

        var colorFabric = _mapper.Map<ColorFabric>(dto);
        await _unitOfWork.ColorFabrics.AddAsync(colorFabric);
        await _unitOfWork.SaveChangesAsync();

        return _mapper.Map<ColorFabricDto>(colorFabric);
    }

    public async Task<ColorFabricDto> UpdateAsync(UpdateColorFabricDto dto)
    {
        var colorFabric = await _unitOfWork.ColorFabrics.GetByIdAsync(dto.Id);
        if (colorFabric == null)
        {
            throw new KeyNotFoundException("Không tìm thấy màu vải.");
        }

        // Check if new name already exists for another record
        var existing = await _unitOfWork.ColorFabrics.GetByNameAsync(dto.Name);
        if (existing != null && existing.Id != dto.Id)
        {
            throw new InvalidOperationException($"Màu vải '{dto.Name}' đã tồn tại.");
        }

        _mapper.Map(dto, colorFabric);
        _unitOfWork.ColorFabrics.Update(colorFabric);
        await _unitOfWork.SaveChangesAsync();

        return _mapper.Map<ColorFabricDto>(colorFabric);
    }

    public async Task DeleteAsync(Guid id)
    {
        var colorFabric = await _unitOfWork.ColorFabrics.GetByIdAsync(id);
        if (colorFabric == null)
        {
            throw new KeyNotFoundException("Không tìm thấy màu vải.");
        }

        _unitOfWork.ColorFabrics.Delete(colorFabric);
        await _unitOfWork.SaveChangesAsync();
    }
}
