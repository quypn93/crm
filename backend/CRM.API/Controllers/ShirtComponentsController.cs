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
public class ShirtComponentsController : ControllerBase
{
    private readonly IShirtComponentService _shirtComponentService;

    public ShirtComponentsController(IShirtComponentService shirtComponentService)
    {
        _shirtComponentService = shirtComponentService;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<PaginatedResult<ShirtComponentDto>>>> GetAll([FromQuery] ShirtComponentFilterDto filter)
    {
        var result = await _shirtComponentService.GetPagedAsync(filter);
        return Ok(ApiResponse<PaginatedResult<ShirtComponentDto>>.Ok(result));
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ApiResponse<ShirtComponentDto>>> GetById(Guid id)
    {
        var component = await _shirtComponentService.GetByIdAsync(id);

        if (component == null)
        {
            return NotFound(ApiResponse<ShirtComponentDto>.Fail("Không tìm thấy thành phần áo."));
        }

        return Ok(ApiResponse<ShirtComponentDto>.Ok(component));
    }

    [HttpGet("by-type/{type}")]
    public async Task<ActionResult<ApiResponse<IEnumerable<ShirtComponentDto>>>> GetByType(ComponentType type)
    {
        var components = await _shirtComponentService.GetByTypeAsync(type);
        return Ok(ApiResponse<IEnumerable<ShirtComponentDto>>.Ok(components));
    }

    [HttpGet("active/by-type/{type}")]
    public async Task<ActionResult<ApiResponse<IEnumerable<ShirtComponentDto>>>> GetActiveByType(ComponentType type)
    {
        var components = await _shirtComponentService.GetActiveByTypeAsync(type);
        return Ok(ApiResponse<IEnumerable<ShirtComponentDto>>.Ok(components));
    }

    [HttpGet("by-colorfabric/{colorFabricId}")]
    public async Task<ActionResult<ApiResponse<IEnumerable<ShirtComponentDto>>>> GetByColorFabric(Guid colorFabricId)
    {
        var components = await _shirtComponentService.GetByColorFabricIdAsync(colorFabricId);
        return Ok(ApiResponse<IEnumerable<ShirtComponentDto>>.Ok(components));
    }

    [HttpGet("types")]
    public ActionResult<ApiResponse<Dictionary<int, string>>> GetComponentTypes()
    {
        var types = Enum.GetValues<ComponentType>()
            .ToDictionary(
                t => (int)t,
                t => ComponentTypeHelper.GetDisplayName(t));
        return Ok(ApiResponse<Dictionary<int, string>>.Ok(types));
    }

    [HttpPost]
    [Authorize(Policy = Policies.CanManageShirtComponents)]
    public async Task<ActionResult<ApiResponse<ShirtComponentDto>>> Create([FromBody] CreateShirtComponentDto dto)
    {
        try
        {
            var component = await _shirtComponentService.CreateAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = component.Id },
                ApiResponse<ShirtComponentDto>.Ok(component, "Tạo thành phần áo thành công."));
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse<ShirtComponentDto>.Fail(ex.Message));
        }
    }

    [HttpPut("{id}")]
    [Authorize(Policy = Policies.CanManageShirtComponents)]
    public async Task<ActionResult<ApiResponse<ShirtComponentDto>>> Update(Guid id, [FromBody] UpdateShirtComponentDto dto)
    {
        if (id != dto.Id)
        {
            return BadRequest(ApiResponse<ShirtComponentDto>.Fail("ID không khớp."));
        }

        try
        {
            var component = await _shirtComponentService.UpdateAsync(dto);
            return Ok(ApiResponse<ShirtComponentDto>.Ok(component, "Cập nhật thành phần áo thành công."));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<ShirtComponentDto>.Fail(ex.Message));
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse<ShirtComponentDto>.Fail(ex.Message));
        }
    }

    [HttpDelete("{id}")]
    [Authorize(Policy = Policies.CanManageShirtComponents)]
    public async Task<ActionResult<ApiResponse>> Delete(Guid id)
    {
        try
        {
            await _shirtComponentService.DeleteAsync(id);
            return Ok(ApiResponse.Ok("Xóa thành phần áo thành công."));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse.Fail(ex.Message));
        }
    }

    [HttpPost("{id}/restore")]
    [Authorize(Policy = Policies.CanManageShirtComponents)]
    public async Task<ActionResult<ApiResponse>> Restore(Guid id)
    {
        try
        {
            await _shirtComponentService.RestoreAsync(id);
            return Ok(ApiResponse.Ok("Khôi phục thành phần áo thành công."));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse.Fail(ex.Message));
        }
    }
}
