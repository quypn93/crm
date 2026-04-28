using CRM.Application.DTOs.Common;
using CRM.Application.DTOs.Design;
using CRM.Core.Enums;

namespace CRM.Application.Interfaces;

public interface IDesignService
{
    Task<DesignDetailDto?> GetByIdAsync(Guid id);
    Task<PaginatedResult<DesignDto>> GetPagedAsync(DesignFilterDto filter);
    Task<IEnumerable<DesignDto>> GetByOrderAsync(Guid orderId);
    Task<IEnumerable<DesignDto>> GetByUserAsync(Guid userId);
    Task<IEnumerable<DesignDto>> GetAssignedToUserAsync(Guid userId, DesignStatus? status = null);
    Task<IEnumerable<DesignDto>> GetAvailableAsync();
    Task<DesignDto> CreateAsync(CreateDesignDto dto, Guid userId);
    Task<DesignDto> CreateAssignmentAsync(CreateDesignAssignmentDto dto, Guid userId);
    Task<DesignDto> UpdateAssignmentAsync(Guid id, CreateDesignAssignmentDto dto, Guid userId, bool isAdmin);
    Task<DesignDto> UpdateAsync(UpdateDesignDto dto, Guid userId);
    Task<DesignDto> CompleteAsync(Guid id, CompleteDesignDto dto, Guid currentUserId, bool isAdmin);
    Task<DesignDto> UpdateStatusAsync(Guid id, DesignStatus newStatus, Guid currentUserId, bool isAdmin);
    Task DeleteAsync(Guid id, Guid userId);
    Task<DesignDto> DuplicateAsync(Guid id, DuplicateDesignDto dto, Guid userId);
}
