using System.Security.Claims;
using CRM.Application.DTOs.Common;
using CRM.Application.DTOs.Design;
using CRM.Application.Interfaces;
using CRM.API.Authorization;
using CRM.Core.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CRM.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class DesignsController : ControllerBase
{
    private readonly IDesignService _designService;

    public DesignsController(IDesignService designService)
    {
        _designService = designService;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<PaginatedResult<DesignDto>>>> GetAll([FromQuery] DesignFilterDto filter)
    {
        var result = await _designService.GetPagedAsync(filter);
        return Ok(ApiResponse<PaginatedResult<DesignDto>>.Ok(result));
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ApiResponse<DesignDetailDto>>> GetById(Guid id)
    {
        var design = await _designService.GetByIdAsync(id);

        if (design == null)
        {
            return NotFound(ApiResponse<DesignDetailDto>.Fail("Không tìm thấy thiết kế."));
        }

        // Designer chỉ xem được design được giao cho mình (Admin/DesignManager/Sale vẫn xem tất).
        var isDesignerOnly = User.IsInRole("Designer")
            && !User.IsInRole("Admin")
            && !User.IsInRole("DesignManager");
        if (isDesignerOnly && design.AssignedToUserId != GetCurrentUserId())
            return StatusCode(403, ApiResponse<DesignDetailDto>.Fail("Bạn không có quyền xem thiết kế này."));

        return Ok(ApiResponse<DesignDetailDto>.Ok(design));
    }

    [HttpGet("by-order/{orderId}")]
    public async Task<ActionResult<ApiResponse<IEnumerable<DesignDto>>>> GetByOrder(Guid orderId)
    {
        var designs = await _designService.GetByOrderAsync(orderId);
        return Ok(ApiResponse<IEnumerable<DesignDto>>.Ok(designs));
    }

    [HttpGet("my-designs")]
    public async Task<ActionResult<ApiResponse<IEnumerable<DesignDto>>>> GetMyDesigns()
    {
        var userId = GetCurrentUserId();
        var designs = await _designService.GetByUserAsync(userId);
        return Ok(ApiResponse<IEnumerable<DesignDto>>.Ok(designs));
    }

    /// <summary>Designer xem các thiết kế được giao cho mình. Chỉ trả về designs có AssignedToUserId = current user.</summary>
    [HttpGet("my-tasks")]
    public async Task<ActionResult<ApiResponse<IEnumerable<DesignDto>>>> GetMyTasks([FromQuery] DesignStatus? status = null)
    {
        var userId = GetCurrentUserId();
        var designs = await _designService.GetAssignedToUserAsync(userId, status);
        return Ok(ApiResponse<IEnumerable<DesignDto>>.Ok(designs));
    }

    /// <summary>Danh sách design đã hoàn thành, dùng cho dropdown "Design có sẵn" ở order form.</summary>
    [HttpGet("available")]
    public async Task<ActionResult<ApiResponse<IEnumerable<DesignDto>>>> GetAvailable()
    {
        var designs = await _designService.GetAvailableAsync();
        return Ok(ApiResponse<IEnumerable<DesignDto>>.Ok(designs));
    }

    /// <summary>Sale tạo design assignment cho designer.</summary>
    [HttpPost("assign")]
    public async Task<ActionResult<ApiResponse<DesignDto>>> Assign([FromBody] CreateDesignAssignmentDto dto)
    {
        var userId = GetCurrentUserId();
        var design = await _designService.CreateAssignmentAsync(dto, userId);
        return Ok(ApiResponse<DesignDto>.Ok(design, "Đã giao thiết kế cho designer."));
    }

    /// <summary>Sale cập nhật assignment (chỉ người tạo hoặc admin). Không sửa được khi đã Completed.</summary>
    [HttpPut("{id}/assignment")]
    public async Task<ActionResult<ApiResponse<DesignDto>>> UpdateAssignment(Guid id, [FromBody] CreateDesignAssignmentDto dto)
    {
        try
        {
            var userId = GetCurrentUserId();
            var isAdmin = User.IsInRole("Admin") || User.IsInRole("DesignManager");
            var design = await _designService.UpdateAssignmentAsync(id, dto, userId, isAdmin);
            return Ok(ApiResponse<DesignDto>.Ok(design, "Cập nhật assignment thành công."));
        }
        catch (UnauthorizedAccessException ex)
        {
            return StatusCode(403, ApiResponse<DesignDto>.Fail(ex.Message));
        }
    }

    /// <summary>Designer đổi trạng thái thiết kế được giao cho mình (Bắt đầu làm / Hoàn thành / Huỷ).</summary>
    [HttpPut("{id}/status")]
    public async Task<ActionResult<ApiResponse<DesignDto>>> UpdateStatus(Guid id, [FromBody] UpdateDesignStatusRequest req)
    {
        try
        {
            var userId = GetCurrentUserId();
            var isAdmin = User.IsInRole("Admin") || User.IsInRole("DesignManager");
            var design = await _designService.UpdateStatusAsync(id, req.Status, userId, isAdmin);
            return Ok(ApiResponse<DesignDto>.Ok(design, "Đã cập nhật trạng thái."));
        }
        catch (UnauthorizedAccessException ex)
        {
            return StatusCode(403, ApiResponse<DesignDto>.Fail(ex.Message));
        }
    }

    public class UpdateDesignStatusRequest
    {
        public DesignStatus Status { get; set; }
    }

    /// <summary>[Legacy] Designer upload ảnh hoàn thành. Giữ cho backward-compat.</summary>
    [HttpPost("{id}/complete")]
    public async Task<ActionResult<ApiResponse<DesignDto>>> Complete(Guid id, [FromBody] CompleteDesignDto dto)
    {
        try
        {
            var userId = GetCurrentUserId();
            var isAdmin = User.IsInRole("Admin") || User.IsInRole("DesignManager");
            var design = await _designService.CompleteAsync(id, dto, userId, isAdmin);
            return Ok(ApiResponse<DesignDto>.Ok(design, "Đã hoàn thành thiết kế."));
        }
        catch (UnauthorizedAccessException ex)
        {
            return StatusCode(403, ApiResponse<DesignDto>.Fail(ex.Message));
        }
    }

    /// <summary>Upload ảnh cho Design (logo ngực/lưng, ảnh hoàn thành). Trả về URL để frontend gán vào form.</summary>
    [HttpPost("upload-image")]
    public async Task<ActionResult<ApiResponse<UploadImageResultDto>>> UploadImage(IFormFile file)
    {
        if (file == null || file.Length == 0)
            return BadRequest(ApiResponse<UploadImageResultDto>.Fail("Không có file."));

        var allowed = new[] { ".jpg", ".jpeg", ".png", ".webp", ".svg" };
        var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (!allowed.Contains(ext))
            return BadRequest(ApiResponse<UploadImageResultDto>.Fail("Định dạng ảnh không hỗ trợ."));

        var uploadsRoot = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "designs");
        Directory.CreateDirectory(uploadsRoot);
        var fileName = $"design_{Guid.NewGuid():N}{ext}";
        var fullPath = Path.Combine(uploadsRoot, fileName);
        await using (var stream = System.IO.File.Create(fullPath))
            await file.CopyToAsync(stream);

        return Ok(ApiResponse<UploadImageResultDto>.Ok(new UploadImageResultDto
        {
            Url = $"/uploads/designs/{fileName}"
        }));
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<DesignDto>>> Create([FromBody] CreateDesignDto dto)
    {
        try
        {
            var userId = GetCurrentUserId();
            var design = await _designService.CreateAsync(dto, userId);
            return CreatedAtAction(nameof(GetById), new { id = design.Id },
                ApiResponse<DesignDto>.Ok(design, "Tạo thiết kế thành công."));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<DesignDto>.Fail(ex.Message));
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse<DesignDto>.Fail(ex.Message));
        }
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<ApiResponse<DesignDto>>> Update(Guid id, [FromBody] UpdateDesignDto dto)
    {
        if (id != dto.Id)
        {
            return BadRequest(ApiResponse<DesignDto>.Fail("ID không khớp."));
        }

        try
        {
            var userId = GetCurrentUserId();
            var design = await _designService.UpdateAsync(dto, userId);
            return Ok(ApiResponse<DesignDto>.Ok(design, "Cập nhật thiết kế thành công."));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<DesignDto>.Fail(ex.Message));
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse<DesignDto>.Fail(ex.Message));
        }
    }

    [HttpDelete("{id}")]
    [Authorize(Policy = Policies.CanDeleteDesigns)]
    public async Task<ActionResult<ApiResponse>> Delete(Guid id)
    {
        try
        {
            var userId = GetCurrentUserId();
            await _designService.DeleteAsync(id, userId);
            return Ok(ApiResponse.Ok("Xóa thiết kế thành công."));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse.Fail(ex.Message));
        }
    }

    [HttpPost("{id}/duplicate")]
    public async Task<ActionResult<ApiResponse<DesignDto>>> Duplicate(Guid id, [FromBody] DuplicateDesignDto dto)
    {
        try
        {
            var userId = GetCurrentUserId();
            var design = await _designService.DuplicateAsync(id, dto, userId);
            return CreatedAtAction(nameof(GetById), new { id = design.Id },
                ApiResponse<DesignDto>.Ok(design, "Nhân bản thiết kế thành công."));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<DesignDto>.Fail(ex.Message));
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse<DesignDto>.Fail(ex.Message));
        }
    }

    private Guid GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return Guid.Parse(userIdClaim ?? throw new UnauthorizedAccessException());
    }
}
