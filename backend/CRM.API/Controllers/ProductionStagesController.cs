using CRM.Application.DTOs.Common;
using CRM.Application.DTOs.Production;
using CRM.Application.Interfaces;
using CRM.Core.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CRM.API.Controllers;

[ApiController]
[Route("api/production-stages")]
[Authorize]
public class ProductionStagesController : ControllerBase
{
    private readonly IProductionStageService _service;

    public ProductionStagesController(IProductionStageService service)
    {
        _service = service;
    }

    /// <summary>Lấy tất cả khâu sản xuất (kể cả inactive)</summary>
    [HttpGet]
    public async Task<ActionResult<ApiResponse<IEnumerable<ProductionStageDto>>>> GetAll()
    {
        var stages = await _service.GetAllAsync();
        return Ok(ApiResponse<IEnumerable<ProductionStageDto>>.Ok(stages));
    }

    /// <summary>Lấy các khâu đang active</summary>
    [HttpGet("active")]
    public async Task<ActionResult<ApiResponse<IEnumerable<ProductionStageDto>>>> GetActive()
    {
        var stages = await _service.GetAllActiveAsync();
        return Ok(ApiResponse<IEnumerable<ProductionStageDto>>.Ok(stages));
    }

    /// <summary>Lấy chi tiết một khâu</summary>
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ApiResponse<ProductionStageDto>>> GetById(Guid id)
    {
        var stage = await _service.GetByIdAsync(id);
        if (stage == null) return NotFound(ApiResponse<ProductionStageDto>.Fail("Không tìm thấy khâu sản xuất."));
        return Ok(ApiResponse<ProductionStageDto>.Ok(stage));
    }

    /// <summary>Tạo khâu mới</summary>
    [HttpPost]
    [Authorize(Roles = $"{RoleNames.Admin},{RoleNames.ProductionManager}")]
    public async Task<ActionResult<ApiResponse<ProductionStageDto>>> Create([FromBody] CreateProductionStageDto dto)
    {
        var stage = await _service.CreateAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = stage.Id }, ApiResponse<ProductionStageDto>.Ok(stage));
    }

    /// <summary>Cập nhật khâu</summary>
    [HttpPut("{id:guid}")]
    [Authorize(Roles = $"{RoleNames.Admin},{RoleNames.ProductionManager}")]
    public async Task<ActionResult<ApiResponse<ProductionStageDto>>> Update(Guid id, [FromBody] UpdateProductionStageDto dto)
    {
        try
        {
            var stage = await _service.UpdateAsync(id, dto);
            return Ok(ApiResponse<ProductionStageDto>.Ok(stage));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<ProductionStageDto>.Fail(ex.Message));
        }
    }

    /// <summary>Xóa khâu</summary>
    [HttpDelete("{id:guid}")]
    [Authorize(Roles = RoleNames.Admin)]
    public async Task<ActionResult<ApiResponse>> Delete(Guid id)
    {
        try
        {
            await _service.DeleteAsync(id);
            return Ok(ApiResponse.Ok("Đã xóa khâu sản xuất."));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse.Fail(ex.Message));
        }
    }

    /// <summary>Sắp xếp lại thứ tự khâu</summary>
    [HttpPut("reorder")]
    [Authorize(Roles = $"{RoleNames.Admin},{RoleNames.ProductionManager}")]
    public async Task<ActionResult<ApiResponse>> Reorder([FromBody] ReorderProductionStagesDto dto)
    {
        await _service.ReorderAsync(dto);
        return Ok(ApiResponse.Ok("Đã cập nhật thứ tự."));
    }
}
