using System.Security.Claims;
using CRM.Application.DTOs.Common;
using CRM.Application.DTOs.Design;
using CRM.Application.Interfaces;
using CRM.API.Authorization;
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
