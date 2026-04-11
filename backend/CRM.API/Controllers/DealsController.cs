using System.Security.Claims;
using CRM.Application.DTOs.Common;
using CRM.Application.DTOs.Deal;
using CRM.Application.Interfaces;
using CRM.API.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CRM.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class DealsController : ControllerBase
{
    private readonly IDealService _dealService;

    public DealsController(IDealService dealService)
    {
        _dealService = dealService;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<PaginatedResult<DealDto>>>> GetAll([FromQuery] DealFilterDto filter)
    {
        var result = await _dealService.GetPagedAsync(filter);
        return Ok(ApiResponse<PaginatedResult<DealDto>>.Ok(result));
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ApiResponse<DealDto>>> GetById(Guid id)
    {
        var deal = await _dealService.GetByIdAsync(id);

        if (deal == null)
        {
            return NotFound(ApiResponse<DealDto>.Fail("Không tìm thấy giao dịch."));
        }

        return Ok(ApiResponse<DealDto>.Ok(deal));
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<DealDto>>> Create([FromBody] CreateDealDto dto)
    {
        try
        {
            var userId = GetCurrentUserId();
            var deal = await _dealService.CreateAsync(dto, userId);
            return CreatedAtAction(nameof(GetById), new { id = deal.Id },
                ApiResponse<DealDto>.Ok(deal, "Tạo giao dịch thành công."));
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse<DealDto>.Fail(ex.Message));
        }
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<ApiResponse<DealDto>>> Update(Guid id, [FromBody] UpdateDealDto dto)
    {
        if (id != dto.Id)
        {
            return BadRequest(ApiResponse<DealDto>.Fail("ID không khớp."));
        }

        try
        {
            var userId = GetCurrentUserId();
            var deal = await _dealService.UpdateAsync(dto, userId);
            return Ok(ApiResponse<DealDto>.Ok(deal, "Cập nhật giao dịch thành công."));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<DealDto>.Fail(ex.Message));
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse<DealDto>.Fail(ex.Message));
        }
    }

    [HttpDelete("{id}")]
    [Authorize(Policy = Policies.CanDeleteDeals)]
    public async Task<ActionResult<ApiResponse>> Delete(Guid id)
    {
        try
        {
            var userId = GetCurrentUserId();
            await _dealService.DeleteAsync(id, userId);
            return Ok(ApiResponse.Ok("Xóa giao dịch thành công."));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse.Fail(ex.Message));
        }
    }

    [HttpPut("{id}/stage")]
    public async Task<ActionResult<ApiResponse<DealDto>>> UpdateStage(Guid id, [FromBody] UpdateDealStageDto dto)
    {
        if (id != dto.DealId)
        {
            return BadRequest(ApiResponse<DealDto>.Fail("ID không khớp."));
        }

        try
        {
            var userId = GetCurrentUserId();
            var deal = await _dealService.UpdateStageAsync(dto, userId);
            return Ok(ApiResponse<DealDto>.Ok(deal, "Cập nhật giai đoạn thành công."));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<DealDto>.Fail(ex.Message));
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse<DealDto>.Fail(ex.Message));
        }
    }

    [HttpPost("{id}/won")]
    [Authorize(Policy = Policies.CanCloseDeal)]
    public async Task<ActionResult<ApiResponse<DealDto>>> MarkAsWon(Guid id, [FromBody] MarkDealWonDto dto)
    {
        try
        {
            var userId = GetCurrentUserId();
            var deal = await _dealService.MarkAsWonAsync(id, dto, userId);
            return Ok(ApiResponse<DealDto>.Ok(deal, "Đánh dấu thắng thành công."));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<DealDto>.Fail(ex.Message));
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse<DealDto>.Fail(ex.Message));
        }
    }

    [HttpPost("{id}/lost")]
    [Authorize(Policy = Policies.CanCloseDeal)]
    public async Task<ActionResult<ApiResponse<DealDto>>> MarkAsLost(Guid id, [FromBody] MarkDealLostDto dto)
    {
        try
        {
            var userId = GetCurrentUserId();
            var deal = await _dealService.MarkAsLostAsync(id, dto, userId);
            return Ok(ApiResponse<DealDto>.Ok(deal, "Đánh dấu thua thành công."));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<DealDto>.Fail(ex.Message));
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse<DealDto>.Fail(ex.Message));
        }
    }

    [HttpGet("stages")]
    public async Task<ActionResult<ApiResponse<IEnumerable<DealStageDto>>>> GetStages()
    {
        var stages = await _dealService.GetAllStagesAsync();
        return Ok(ApiResponse<IEnumerable<DealStageDto>>.Ok(stages));
    }

    [HttpGet("pipeline")]
    public async Task<ActionResult<ApiResponse<IEnumerable<DealsByStageDto>>>> GetPipeline()
    {
        var pipeline = await _dealService.GetDealsByStageAsync();
        return Ok(ApiResponse<IEnumerable<DealsByStageDto>>.Ok(pipeline));
    }

    [HttpGet("customer/{customerId}")]
    public async Task<ActionResult<ApiResponse<IEnumerable<DealDto>>>> GetByCustomer(Guid customerId)
    {
        var deals = await _dealService.GetByCustomerAsync(customerId);
        return Ok(ApiResponse<IEnumerable<DealDto>>.Ok(deals));
    }

    private Guid GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return Guid.Parse(userIdClaim ?? throw new UnauthorizedAccessException());
    }
}
