using System.Security.Claims;
using CRM.Application.DTOs.Common;
using CRM.Application.DTOs.Order;
using CRM.Application.Interfaces;
using CRM.API.Authorization;
using CRM.Core.Entities;
using CRM.Core.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CRM.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class OrdersController : ControllerBase
{
    private readonly IOrderService _orderService;

    public OrdersController(IOrderService orderService)
    {
        _orderService = orderService;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<PaginatedResult<OrderDto>>>> GetAll([FromQuery] OrderFilterDto filter)
    {
        // SalesRep chỉ xem đơn mình tạo
        var userRoles = GetCurrentUserRoles().ToList();
        var isSalesRepOnly = userRoles.Contains(RoleNames.SalesRep)
            && !userRoles.Contains(RoleNames.Admin)
            && !userRoles.Contains(RoleNames.SalesManager);

        if (isSalesRepOnly)
            filter.CreatedBy = GetCurrentUserId();

        var result = await _orderService.GetPagedAsync(filter);
        return Ok(ApiResponse<PaginatedResult<OrderDto>>.Ok(result));
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ApiResponse<OrderDto>>> GetById(Guid id)
    {
        var order = await _orderService.GetByIdAsync(id);

        if (order == null)
            return NotFound(ApiResponse<OrderDto>.Fail("Không tìm thấy đơn hàng."));

        // SalesRep chỉ xem đơn mình tạo
        var userRoles = GetCurrentUserRoles().ToList();
        var isSalesRepOnly = userRoles.Contains(RoleNames.SalesRep)
            && !userRoles.Contains(RoleNames.Admin)
            && !userRoles.Contains(RoleNames.SalesManager);

        if (isSalesRepOnly && order.CreatedByUserId != GetCurrentUserId())
            return Forbid();

        return Ok(ApiResponse<OrderDto>.Ok(order));
    }

    [HttpGet("number/{orderNumber}")]
    public async Task<ActionResult<ApiResponse<OrderDto>>> GetByOrderNumber(string orderNumber)
    {
        var order = await _orderService.GetByOrderNumberAsync(orderNumber);

        if (order == null)
        {
            return NotFound(ApiResponse<OrderDto>.Fail("Không tìm thấy đơn hàng."));
        }

        return Ok(ApiResponse<OrderDto>.Ok(order));
    }

    [HttpGet("customer/{customerId}")]
    public async Task<ActionResult<ApiResponse<IEnumerable<OrderDto>>>> GetByCustomer(Guid customerId)
    {
        var orders = await _orderService.GetByCustomerAsync(customerId);
        return Ok(ApiResponse<IEnumerable<OrderDto>>.Ok(orders));
    }

    [HttpGet("deal/{dealId}")]
    public async Task<ActionResult<ApiResponse<IEnumerable<OrderDto>>>> GetByDeal(Guid dealId)
    {
        var orders = await _orderService.GetByDealAsync(dealId);
        return Ok(ApiResponse<IEnumerable<OrderDto>>.Ok(orders));
    }

    [HttpGet("my-orders")]
    public async Task<ActionResult<ApiResponse<IEnumerable<OrderDto>>>> GetMyOrders()
    {
        var userId = GetCurrentUserId();
        var orders = await _orderService.GetMyOrdersAsync(userId);
        return Ok(ApiResponse<IEnumerable<OrderDto>>.Ok(orders));
    }

    [HttpGet("summary")]
    [Authorize(Policy = Policies.CanViewOrderSummary)]
    public async Task<ActionResult<ApiResponse<OrderSummaryDto>>> GetSummary()
    {
        var summary = await _orderService.GetSummaryAsync();
        return Ok(ApiResponse<OrderSummaryDto>.Ok(summary));
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<OrderDto>>> Create([FromBody] CreateOrderDto dto)
    {
        try
        {
            var userId = GetCurrentUserId();
            var order = await _orderService.CreateAsync(dto, userId);
            return CreatedAtAction(nameof(GetById), new { id = order.Id },
                ApiResponse<OrderDto>.Ok(order, "Tạo đơn hàng thành công."));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<OrderDto>.Fail(ex.Message));
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse<OrderDto>.Fail(ex.Message));
        }
    }

    [HttpPost("from-deal/{dealId}")]
    public async Task<ActionResult<ApiResponse<OrderDto>>> CreateFromDeal(Guid dealId)
    {
        try
        {
            var userId = GetCurrentUserId();
            var order = await _orderService.CreateFromDealAsync(dealId, userId);
            return CreatedAtAction(nameof(GetById), new { id = order.Id },
                ApiResponse<OrderDto>.Ok(order, "Tạo đơn hàng từ giao dịch thành công."));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<OrderDto>.Fail(ex.Message));
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse<OrderDto>.Fail(ex.Message));
        }
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<ApiResponse<OrderDto>>> Update(Guid id, [FromBody] UpdateOrderDto dto)
    {
        if (id != dto.Id)
        {
            return BadRequest(ApiResponse<OrderDto>.Fail("ID không khớp."));
        }

        try
        {
            var userId = GetCurrentUserId();
            var order = await _orderService.UpdateAsync(dto, userId);
            return Ok(ApiResponse<OrderDto>.Ok(order, "Cập nhật đơn hàng thành công."));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<OrderDto>.Fail(ex.Message));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<OrderDto>.Fail(ex.Message));
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse<OrderDto>.Fail(ex.Message));
        }
    }

    [HttpPut("{id}/status")]
    public async Task<ActionResult<ApiResponse<OrderDto>>> UpdateStatus(Guid id, [FromBody] UpdateOrderStatusDto dto)
    {
        try
        {
            // Get current order to validate status transition
            var currentOrder = await _orderService.GetByIdAsync(id);
            if (currentOrder == null)
            {
                return NotFound(ApiResponse<OrderDto>.Fail("Không tìm thấy đơn hàng."));
            }

            // Get user roles and validate transition permission
            var userRoles = GetCurrentUserRoles();
            var currentStatus = (OrderStatus)currentOrder.Status;
            var newStatus = dto.Status;

            if (!OrderStatusTransitionValidator.CanTransition(currentStatus, newStatus, userRoles))
            {
                var errorMessage = OrderStatusTransitionValidator.GetTransitionErrorMessage(currentStatus, newStatus, userRoles);
                return StatusCode(403, ApiResponse<OrderDto>.Fail(errorMessage));
            }

            var userId = GetCurrentUserId();
            var order = await _orderService.UpdateStatusAsync(id, dto, userId);
            return Ok(ApiResponse<OrderDto>.Ok(order, "Cập nhật trạng thái thành công."));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<OrderDto>.Fail(ex.Message));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<OrderDto>.Fail(ex.Message));
        }
    }

    [HttpGet("{id}/allowed-statuses")]
    public async Task<ActionResult<ApiResponse<IEnumerable<OrderStatus>>>> GetAllowedStatuses(Guid id)
    {
        var order = await _orderService.GetByIdAsync(id);
        if (order == null)
        {
            return NotFound(ApiResponse<IEnumerable<OrderStatus>>.Fail("Không tìm thấy đơn hàng."));
        }

        var userRoles = GetCurrentUserRoles();
        var currentStatus = (OrderStatus)order.Status;
        var allowedStatuses = OrderStatusTransitionValidator.GetAllowedNextStatuses(currentStatus, userRoles);

        return Ok(ApiResponse<IEnumerable<OrderStatus>>.Ok(allowedStatuses));
    }

    [HttpPut("{id}/payment")]
    [Authorize(Policy = Policies.CanUpdatePayment)]
    public async Task<ActionResult<ApiResponse<OrderDto>>> UpdatePayment(Guid id, [FromBody] UpdatePaymentDto dto)
    {
        try
        {
            var userId = GetCurrentUserId();
            var order = await _orderService.UpdatePaymentAsync(id, dto, userId);
            return Ok(ApiResponse<OrderDto>.Ok(order, "Cập nhật thanh toán thành công."));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<OrderDto>.Fail(ex.Message));
        }
    }

    [HttpDelete("{id}")]
    [Authorize(Policy = Policies.CanDeleteOrders)]
    public async Task<ActionResult<ApiResponse>> Delete(Guid id)
    {
        try
        {
            var userId = GetCurrentUserId();
            await _orderService.DeleteAsync(id, userId);
            return Ok(ApiResponse.Ok("Xóa đơn hàng thành công."));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse.Fail(ex.Message));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse.Fail(ex.Message));
        }
    }

    /// <summary>Sinh lại QR cho đơn hàng (backfill các đơn cũ)</summary>
    [HttpPost("{id}/generate-qr")]
    public async Task<ActionResult<ApiResponse<OrderDto>>> GenerateQr(Guid id)
    {
        try
        {
            var result = await _orderService.GenerateQrAsync(id);
            return Ok(ApiResponse<OrderDto>.Ok(result));
        }
        catch (KeyNotFoundException ex) { return NotFound(ApiResponse<OrderDto>.Fail(ex.Message)); }
    }

    private Guid GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return Guid.Parse(userIdClaim ?? throw new UnauthorizedAccessException());
    }

    private IEnumerable<string> GetCurrentUserRoles()
    {
        return User.FindAll(ClaimTypes.Role).Select(c => c.Value);
    }
}
