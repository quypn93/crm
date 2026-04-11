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
public class ColorFabricsController : ControllerBase
{
    private readonly IColorFabricService _colorFabricService;

    public ColorFabricsController(IColorFabricService colorFabricService)
    {
        _colorFabricService = colorFabricService;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<PaginatedResult<ColorFabricDto>>>> GetAll([FromQuery] ColorFabricFilterDto filter)
    {
        var result = await _colorFabricService.GetPagedAsync(filter);
        return Ok(ApiResponse<PaginatedResult<ColorFabricDto>>.Ok(result));
    }

    [HttpGet("all")]
    public async Task<ActionResult<ApiResponse<IEnumerable<ColorFabricDto>>>> GetAllList()
    {
        var result = await _colorFabricService.GetAllAsync();
        return Ok(ApiResponse<IEnumerable<ColorFabricDto>>.Ok(result));
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ApiResponse<ColorFabricDto>>> GetById(Guid id)
    {
        var colorFabric = await _colorFabricService.GetByIdAsync(id);

        if (colorFabric == null)
        {
            return NotFound(ApiResponse<ColorFabricDto>.Fail("Không tìm thấy màu vải."));
        }

        return Ok(ApiResponse<ColorFabricDto>.Ok(colorFabric));
    }

    [HttpPost]
    [Authorize(Policy = Policies.CanManageColorFabrics)]
    public async Task<ActionResult<ApiResponse<ColorFabricDto>>> Create([FromBody] CreateColorFabricDto dto)
    {
        try
        {
            var colorFabric = await _colorFabricService.CreateAsync(dto);
            return CreatedAtAction(nameof(GetById), new { id = colorFabric.Id },
                ApiResponse<ColorFabricDto>.Ok(colorFabric, "Tạo màu vải thành công."));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<ColorFabricDto>.Fail(ex.Message));
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse<ColorFabricDto>.Fail(ex.Message));
        }
    }

    [HttpPut("{id}")]
    [Authorize(Policy = Policies.CanManageColorFabrics)]
    public async Task<ActionResult<ApiResponse<ColorFabricDto>>> Update(Guid id, [FromBody] UpdateColorFabricDto dto)
    {
        if (id != dto.Id)
        {
            return BadRequest(ApiResponse<ColorFabricDto>.Fail("ID không khớp."));
        }

        try
        {
            var colorFabric = await _colorFabricService.UpdateAsync(dto);
            return Ok(ApiResponse<ColorFabricDto>.Ok(colorFabric, "Cập nhật màu vải thành công."));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<ColorFabricDto>.Fail(ex.Message));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<ColorFabricDto>.Fail(ex.Message));
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse<ColorFabricDto>.Fail(ex.Message));
        }
    }

    [HttpDelete("{id}")]
    [Authorize(Policy = Policies.CanManageColorFabrics)]
    public async Task<ActionResult<ApiResponse>> Delete(Guid id)
    {
        try
        {
            await _colorFabricService.DeleteAsync(id);
            return Ok(ApiResponse.Ok("Xóa màu vải thành công."));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse.Fail(ex.Message));
        }
    }
}
