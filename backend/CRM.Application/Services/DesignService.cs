using AutoMapper;
using CRM.Application.DTOs.Common;
using CRM.Application.DTOs.Design;
using CRM.Application.Interfaces;
using CRM.Core.Entities;
using CRM.Core.Enums;
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
            filter.AssignedToUserId,
            filter.Status,
            filter.FromDate,
            filter.ToDate,
            filter.Page,
            filter.PageSize,
            filter.SortBy,
            filter.SortOrder);

        var dtos = _mapper.Map<List<DesignDto>>(items);
        return PaginatedResult<DesignDto>.Create(dtos, totalCount, filter.Page, filter.PageSize);
    }

    public async Task<IEnumerable<DesignDto>> GetAssignedToUserAsync(Guid userId, DesignStatus? status = null)
    {
        var designs = await _unitOfWork.Designs.GetAssignedToUserAsync(userId, status);
        return _mapper.Map<IEnumerable<DesignDto>>(designs);
    }

    public async Task<IEnumerable<DesignDto>> GetAvailableAsync()
    {
        var designs = await _unitOfWork.Designs.GetAvailableAsync();
        return _mapper.Map<IEnumerable<DesignDto>>(designs);
    }

    public async Task<DesignDto> CreateAssignmentAsync(CreateDesignAssignmentDto dto, Guid userId)
    {
        if (dto.AssignedToUserId == Guid.Empty)
            throw new ArgumentException("Phải chọn designer để giao.");

        var designer = await _unitOfWork.Users.GetByIdAsync(dto.AssignedToUserId);
        if (designer == null)
            throw new KeyNotFoundException("Không tìm thấy designer.");

        if (dto.OrderId.HasValue)
        {
            var order = await _unitOfWork.Orders.GetByIdAsync(dto.OrderId.Value);
            if (order == null) throw new KeyNotFoundException("Không tìm thấy đơn hàng.");
        }
        if (dto.ColorFabricId.HasValue)
        {
            var cf = await _unitOfWork.ColorFabrics.GetByIdAsync(dto.ColorFabricId.Value);
            if (cf == null) throw new KeyNotFoundException("Không tìm thấy màu áo chính.");
        }
        if (dto.AccentColorFabricId.HasValue)
        {
            var cf = await _unitOfWork.ColorFabrics.GetByIdAsync(dto.AccentColorFabricId.Value);
            if (cf == null) throw new KeyNotFoundException("Không tìm thấy màu phối.");
        }
        if (dto.ShirtFormId.HasValue)
        {
            var f = await _unitOfWork.ProductForms.GetByIdAsync(dto.ShirtFormId.Value);
            if (f == null) throw new KeyNotFoundException("Không tìm thấy mẫu áo.");
        }

        var design = new Design
        {
            DesignName = string.IsNullOrWhiteSpace(dto.DesignName) ? $"Design-{DateTime.UtcNow:yyyyMMddHHmm}" : dto.DesignName,
            AssignedToUserId = dto.AssignedToUserId,
            Status = DesignStatus.Assigned,
            ShirtFormId = dto.ShirtFormId,
            ColorFabricId = dto.ColorFabricId,
            AccentColorFabricId = dto.AccentColorFabricId,
            ChestLogoUrl = dto.ChestLogoUrl,
            BackLogoUrl = dto.BackLogoUrl,
            AssignmentNotes = dto.AssignmentNotes,
            OrderId = dto.OrderId,
            CustomerFullName = dto.CustomerFullName,
            CreatedByUserId = userId
        };

        await _unitOfWork.Designs.AddAsync(design);
        await _unitOfWork.SaveChangesAsync();

        var loaded = await _unitOfWork.Designs.GetByIdWithDetailsAsync(design.Id);
        return _mapper.Map<DesignDto>(loaded);
    }

    public async Task<DesignDto> UpdateAssignmentAsync(Guid id, CreateDesignAssignmentDto dto, Guid userId, bool isAdmin)
    {
        var design = await _unitOfWork.Designs.GetByIdAsync(id)
            ?? throw new KeyNotFoundException("Không tìm thấy thiết kế.");

        // Sale chỉ edit được assignment mình đã tạo; Admin/DesignManager thì mọi bản.
        if (!isAdmin && design.CreatedByUserId != userId)
            throw new UnauthorizedAccessException("Bạn không có quyền sửa thiết kế này.");

        // Không cho sửa khi designer đã hoàn thành — tránh lệch spec so với ảnh final.
        if (design.Status == DesignStatus.Completed)
            throw new InvalidOperationException("Thiết kế đã hoàn thành, không thể sửa assignment.");

        // Validate FK refs nếu thay đổi.
        if (dto.AssignedToUserId == Guid.Empty)
            throw new ArgumentException("Phải chọn designer để giao.");
        var designer = await _unitOfWork.Users.GetByIdAsync(dto.AssignedToUserId)
            ?? throw new KeyNotFoundException("Không tìm thấy designer.");

        if (dto.ColorFabricId.HasValue && await _unitOfWork.ColorFabrics.GetByIdAsync(dto.ColorFabricId.Value) == null)
            throw new KeyNotFoundException("Không tìm thấy màu áo chính.");
        if (dto.AccentColorFabricId.HasValue && await _unitOfWork.ColorFabrics.GetByIdAsync(dto.AccentColorFabricId.Value) == null)
            throw new KeyNotFoundException("Không tìm thấy màu phối.");
        if (dto.ShirtFormId.HasValue && await _unitOfWork.ProductForms.GetByIdAsync(dto.ShirtFormId.Value) == null)
            throw new KeyNotFoundException("Không tìm thấy mẫu áo.");

        design.DesignName = string.IsNullOrWhiteSpace(dto.DesignName) ? design.DesignName : dto.DesignName;
        design.AssignedToUserId = dto.AssignedToUserId;
        design.ShirtFormId = dto.ShirtFormId;
        design.ColorFabricId = dto.ColorFabricId;
        design.AccentColorFabricId = dto.AccentColorFabricId;
        design.ChestLogoUrl = dto.ChestLogoUrl;
        design.BackLogoUrl = dto.BackLogoUrl;
        design.AssignmentNotes = dto.AssignmentNotes;
        design.CustomerFullName = dto.CustomerFullName;
        if (dto.OrderId.HasValue) design.OrderId = dto.OrderId;

        _unitOfWork.Designs.Update(design);
        await _unitOfWork.SaveChangesAsync();

        var loaded = await _unitOfWork.Designs.GetByIdWithDetailsAsync(id);
        return _mapper.Map<DesignDto>(loaded);
    }

    public async Task<DesignDto> UpdateStatusAsync(Guid id, DesignStatus newStatus, Guid currentUserId, bool isAdmin)
    {
        var design = await _unitOfWork.Designs.GetByIdAsync(id)
            ?? throw new KeyNotFoundException("Không tìm thấy thiết kế.");

        // Chỉ designer được giao (hoặc Admin/DesignManager) được đổi trạng thái.
        if (!isAdmin && design.AssignedToUserId != currentUserId)
            throw new UnauthorizedAccessException("Bạn không phải designer được giao thiết kế này.");

        if (design.Status == newStatus) return _mapper.Map<DesignDto>(design);

        ValidateStatusTransition(design.Status, newStatus, isAdmin);

        design.Status = newStatus;
        if (newStatus == DesignStatus.Completed && design.CompletedAt == null)
            design.CompletedAt = DateTime.UtcNow;
        if (newStatus != DesignStatus.Completed)
            design.CompletedAt = null;

        _unitOfWork.Designs.Update(design);
        await _unitOfWork.SaveChangesAsync();

        var loaded = await _unitOfWork.Designs.GetByIdWithDetailsAsync(id);
        return _mapper.Map<DesignDto>(loaded);
    }

    private static void ValidateStatusTransition(DesignStatus current, DesignStatus next, bool isAdmin)
    {
        // Admin có thể nhảy bất kỳ trạng thái nào (unstick khi designer sai).
        if (isAdmin) return;

        var allowed = current switch
        {
            DesignStatus.Assigned   => new[] { DesignStatus.InProgress, DesignStatus.Cancelled },
            DesignStatus.InProgress => new[] { DesignStatus.Completed, DesignStatus.Cancelled, DesignStatus.Assigned },
            DesignStatus.Completed  => Array.Empty<DesignStatus>(),
            DesignStatus.Cancelled  => Array.Empty<DesignStatus>(),
            _ => Array.Empty<DesignStatus>()
        };

        if (!allowed.Contains(next))
            throw new InvalidOperationException($"Không thể chuyển từ '{current}' sang '{next}'.");
    }

    public async Task<DesignDto> CompleteAsync(Guid id, CompleteDesignDto dto, Guid currentUserId, bool isAdmin)
    {
        if (string.IsNullOrWhiteSpace(dto.CompletedImageUrl))
            throw new ArgumentException("Thiếu URL ảnh hoàn thành.");

        var design = await _unitOfWork.Designs.GetByIdAsync(id)
            ?? throw new KeyNotFoundException("Không tìm thấy thiết kế.");

        // Chỉ designer được assign (hoặc admin) mới có thể complete.
        if (!isAdmin && design.AssignedToUserId != currentUserId)
            throw new UnauthorizedAccessException("Bạn không phải designer được giao thiết kế này.");

        design.CompletedImageUrl = dto.CompletedImageUrl;
        design.Status = DesignStatus.Completed;
        design.CompletedAt = DateTime.UtcNow;
        if (!string.IsNullOrWhiteSpace(dto.Note))
            design.NoteOther = string.IsNullOrWhiteSpace(design.NoteOther) ? dto.Note : $"{design.NoteOther}\n{dto.Note}";

        _unitOfWork.Designs.Update(design);
        await _unitOfWork.SaveChangesAsync();

        var loaded = await _unitOfWork.Designs.GetByIdWithDetailsAsync(id);
        return _mapper.Map<DesignDto>(loaded);
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
