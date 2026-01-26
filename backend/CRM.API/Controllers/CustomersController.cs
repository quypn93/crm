using System.Security.Claims;
using CRM.Application.DTOs.Common;
using CRM.Application.DTOs.Customer;
using CRM.Core.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CRM.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class CustomersController : ControllerBase
{
    private readonly ICustomerService _customerService;

    public CustomersController(ICustomerService customerService)
    {
        _customerService = customerService;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<PaginatedResult<CustomerDto>>>> GetAll([FromQuery] CustomerFilterDto filter)
    {
        var result = await _customerService.GetPagedAsync(filter);
        return Ok(ApiResponse<PaginatedResult<CustomerDto>>.Ok(result));
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ApiResponse<CustomerDto>>> GetById(Guid id)
    {
        var customer = await _customerService.GetByIdAsync(id);

        if (customer == null)
        {
            return NotFound(ApiResponse<CustomerDto>.Fail("Không tìm thấy khách hàng."));
        }

        return Ok(ApiResponse<CustomerDto>.Ok(customer));
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<CustomerDto>>> Create([FromBody] CreateCustomerDto dto)
    {
        try
        {
            var userId = GetCurrentUserId();
            var customer = await _customerService.CreateAsync(dto, userId);
            return CreatedAtAction(nameof(GetById), new { id = customer.Id },
                ApiResponse<CustomerDto>.Ok(customer, "Tạo khách hàng thành công."));
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse<CustomerDto>.Fail(ex.Message));
        }
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<ApiResponse<CustomerDto>>> Update(Guid id, [FromBody] UpdateCustomerDto dto)
    {
        if (id != dto.Id)
        {
            return BadRequest(ApiResponse<CustomerDto>.Fail("ID không khớp."));
        }

        try
        {
            var userId = GetCurrentUserId();
            var customer = await _customerService.UpdateAsync(dto, userId);
            return Ok(ApiResponse<CustomerDto>.Ok(customer, "Cập nhật khách hàng thành công."));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<CustomerDto>.Fail(ex.Message));
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse<CustomerDto>.Fail(ex.Message));
        }
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult<ApiResponse>> Delete(Guid id)
    {
        try
        {
            var userId = GetCurrentUserId();
            await _customerService.DeleteAsync(id, userId);
            return Ok(ApiResponse.Ok("Xóa khách hàng thành công."));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse.Fail(ex.Message));
        }
    }

    [HttpGet("my-customers")]
    public async Task<ActionResult<ApiResponse<IEnumerable<CustomerDto>>>> GetMyCustomers()
    {
        var userId = GetCurrentUserId();
        var customers = await _customerService.GetByAssignedUserAsync(userId);
        return Ok(ApiResponse<IEnumerable<CustomerDto>>.Ok(customers));
    }

    [HttpGet("industries")]
    public ActionResult<ApiResponse<List<string>>> GetIndustries()
    {
        return Ok(ApiResponse<List<string>>.Ok(CustomerIndustries.All));
    }

    private Guid GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return Guid.Parse(userIdClaim ?? throw new UnauthorizedAccessException());
    }
}
