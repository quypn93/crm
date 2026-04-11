using AutoMapper;
using CRM.Application.DTOs.Common;
using CRM.Application.DTOs.Design;
using CRM.Application.Interfaces;
using CRM.Core.Entities;
using CRM.Core.Interfaces;

namespace CRM.Application.Services;

public class DesignService : IDesignService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public DesignService(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<DesignDetailDto?> GetByIdAsync(Guid id)
    {
        var design = await _unitOfWork.Designs.GetByIdWithDetailsAsync(id);
        return design != null ? _mapper.Map<DesignDetailDto>(design) : null;
    }

    public async Task<PaginatedResult<DesignDto>> GetPagedAsync(DesignFilterDto filter)
    {
        var (items, totalCount) = await _unitOfWork.Designs.GetPagedAsync(
            filter.Search,
            filter.OrderId,
            filter.ColorFabricId,
            filter.CreatedByUserId,
            filter.FromDate,
            filter.ToDate,
            filter.Page,
            filter.PageSize,
            filter.SortBy,
            filter.SortOrder);

        var dtos = _mapper.Map<List<DesignDto>>(items);
        return PaginatedResult<DesignDto>.Create(dtos, totalCount, filter.Page, filter.PageSize);
    }

    public async Task<IEnumerable<DesignDto>> GetByOrderAsync(Guid orderId)
    {
        var designs = await _unitOfWork.Designs.GetByOrderAsync(orderId);
        return _mapper.Map<IEnumerable<DesignDto>>(designs);
    }

    public async Task<IEnumerable<DesignDto>> GetByUserAsync(Guid userId)
    {
        var designs = await _unitOfWork.Designs.GetByUserAsync(userId);
        return _mapper.Map<IEnumerable<DesignDto>>(designs);
    }

    public async Task<DesignDto> CreateAsync(CreateDesignDto dto, Guid userId)
    {
        // Validate OrderId if provided
        if (dto.OrderId.HasValue)
        {
            var order = await _unitOfWork.Orders.GetByIdAsync(dto.OrderId.Value);
            if (order == null)
            {
                throw new KeyNotFoundException("Không tìm thấy đơn hàng.");
            }
        }

        // Validate ColorFabricId if provided
        if (dto.ColorFabricId.HasValue)
        {
            var colorFabric = await _unitOfWork.ColorFabrics.GetByIdAsync(dto.ColorFabricId.Value);
            if (colorFabric == null)
            {
                throw new KeyNotFoundException("Không tìm thấy màu vải.");
            }
        }

        var design = _mapper.Map<Design>(dto);
        design.CreatedByUserId = userId;

        await _unitOfWork.Designs.AddAsync(design);
        await _unitOfWork.SaveChangesAsync();

        return await GetByIdAsync(design.Id) != null
            ? _mapper.Map<DesignDto>(await _unitOfWork.Designs.GetByIdWithDetailsAsync(design.Id))
            : throw new InvalidOperationException("Không thể tạo thiết kế.");
    }

    public async Task<DesignDto> UpdateAsync(UpdateDesignDto dto, Guid userId)
    {
        var design = await _unitOfWork.Designs.GetByIdAsync(dto.Id);
        if (design == null)
        {
            throw new KeyNotFoundException("Không tìm thấy thiết kế.");
        }

        // Validate OrderId if provided
        if (dto.OrderId.HasValue)
        {
            var order = await _unitOfWork.Orders.GetByIdAsync(dto.OrderId.Value);
            if (order == null)
            {
                throw new KeyNotFoundException("Không tìm thấy đơn hàng.");
            }
        }

        // Validate ColorFabricId if provided
        if (dto.ColorFabricId.HasValue)
        {
            var colorFabric = await _unitOfWork.ColorFabrics.GetByIdAsync(dto.ColorFabricId.Value);
            if (colorFabric == null)
            {
                throw new KeyNotFoundException("Không tìm thấy màu vải.");
            }
        }

        _mapper.Map(dto, design);
        _unitOfWork.Designs.Update(design);
        await _unitOfWork.SaveChangesAsync();

        var updated = await _unitOfWork.Designs.GetByIdWithDetailsAsync(design.Id);
        return _mapper.Map<DesignDto>(updated);
    }

    public async Task DeleteAsync(Guid id, Guid userId)
    {
        var design = await _unitOfWork.Designs.GetByIdAsync(id);
        if (design == null)
        {
            throw new KeyNotFoundException("Không tìm thấy thiết kế.");
        }

        _unitOfWork.Designs.Delete(design);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task<DesignDto> DuplicateAsync(Guid id, DuplicateDesignDto dto, Guid userId)
    {
        var original = await _unitOfWork.Designs.GetByIdAsync(id);
        if (original == null)
        {
            throw new KeyNotFoundException("Không tìm thấy thiết kế gốc.");
        }

        // Validate new OrderId if provided
        if (dto.NewOrderId.HasValue)
        {
            var order = await _unitOfWork.Orders.GetByIdAsync(dto.NewOrderId.Value);
            if (order == null)
            {
                throw new KeyNotFoundException("Không tìm thấy đơn hàng mới.");
            }
        }

        var newDesign = new Design
        {
            DesignName = dto.NewDesignName ?? $"{original.DesignName} (Bản sao)",
            DesignData = original.DesignData,
            SelectedComponents = original.SelectedComponents,
            Designer = original.Designer,
            CustomerFullName = original.CustomerFullName,
            Total = original.Total,
            SizeMan = original.SizeMan,
            SizeWomen = original.SizeWomen,
            SizeKid = original.SizeKid,
            Oversized = original.Oversized,
            NoteConfection = original.NoteConfection,
            NoteOldCodeOrder = original.NoteOldCodeOrder,
            NoteAttachTagLabel = original.NoteAttachTagLabel,
            NoteOther = original.NoteOther,
            SaleStaff = original.SaleStaff,
            ColorFabricId = original.ColorFabricId,
            OrderId = dto.NewOrderId ?? original.OrderId,
            CreatedByUserId = userId
        };

        await _unitOfWork.Designs.AddAsync(newDesign);
        await _unitOfWork.SaveChangesAsync();

        var created = await _unitOfWork.Designs.GetByIdWithDetailsAsync(newDesign.Id);
        return _mapper.Map<DesignDto>(created);
    }
}
