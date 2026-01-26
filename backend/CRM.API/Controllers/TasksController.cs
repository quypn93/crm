using System.Security.Claims;
using CRM.Application.DTOs.Common;
using CRM.Application.DTOs.Task;
using CRM.Core.Interfaces.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CRM.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class TasksController : ControllerBase
{
    private readonly ITaskService _taskService;

    public TasksController(ITaskService taskService)
    {
        _taskService = taskService;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<PaginatedResult<TaskDto>>>> GetAll([FromQuery] TaskFilterDto filter)
    {
        var result = await _taskService.GetPagedAsync(filter);
        return Ok(ApiResponse<PaginatedResult<TaskDto>>.Ok(result));
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ApiResponse<TaskDto>>> GetById(Guid id)
    {
        var task = await _taskService.GetByIdAsync(id);

        if (task == null)
        {
            return NotFound(ApiResponse<TaskDto>.Fail("Không tìm thấy tác vụ."));
        }

        return Ok(ApiResponse<TaskDto>.Ok(task));
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<TaskDto>>> Create([FromBody] CreateTaskDto dto)
    {
        try
        {
            var userId = GetCurrentUserId();
            var task = await _taskService.CreateAsync(dto, userId);
            return CreatedAtAction(nameof(GetById), new { id = task.Id },
                ApiResponse<TaskDto>.Ok(task, "Tạo tác vụ thành công."));
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse<TaskDto>.Fail(ex.Message));
        }
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<ApiResponse<TaskDto>>> Update(Guid id, [FromBody] UpdateTaskDto dto)
    {
        if (id != dto.Id)
        {
            return BadRequest(ApiResponse<TaskDto>.Fail("ID không khớp."));
        }

        try
        {
            var userId = GetCurrentUserId();
            var task = await _taskService.UpdateAsync(dto, userId);
            return Ok(ApiResponse<TaskDto>.Ok(task, "Cập nhật tác vụ thành công."));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<TaskDto>.Fail(ex.Message));
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse<TaskDto>.Fail(ex.Message));
        }
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult<ApiResponse>> Delete(Guid id)
    {
        try
        {
            var userId = GetCurrentUserId();
            await _taskService.DeleteAsync(id, userId);
            return Ok(ApiResponse.Ok("Xóa tác vụ thành công."));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse.Fail(ex.Message));
        }
    }

    [HttpPut("{id}/status")]
    public async Task<ActionResult<ApiResponse<TaskDto>>> UpdateStatus(Guid id, [FromBody] UpdateTaskStatusDto dto)
    {
        if (id != dto.TaskId)
        {
            return BadRequest(ApiResponse<TaskDto>.Fail("ID không khớp."));
        }

        try
        {
            var userId = GetCurrentUserId();
            var task = await _taskService.UpdateStatusAsync(dto, userId);
            return Ok(ApiResponse<TaskDto>.Ok(task, "Cập nhật trạng thái thành công."));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<TaskDto>.Fail(ex.Message));
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse<TaskDto>.Fail(ex.Message));
        }
    }

    [HttpGet("my-tasks")]
    public async Task<ActionResult<ApiResponse<IEnumerable<TaskDto>>>> GetMyTasks()
    {
        var userId = GetCurrentUserId();
        var tasks = await _taskService.GetMyTasksAsync(userId);
        return Ok(ApiResponse<IEnumerable<TaskDto>>.Ok(tasks));
    }

    [HttpGet("overdue")]
    public async Task<ActionResult<ApiResponse<IEnumerable<TaskDto>>>> GetOverdue()
    {
        var tasks = await _taskService.GetOverdueTasksAsync();
        return Ok(ApiResponse<IEnumerable<TaskDto>>.Ok(tasks));
    }

    [HttpGet("due-today")]
    public async Task<ActionResult<ApiResponse<IEnumerable<TaskDto>>>> GetDueToday()
    {
        var tasks = await _taskService.GetTasksDueTodayAsync();
        return Ok(ApiResponse<IEnumerable<TaskDto>>.Ok(tasks));
    }

    [HttpGet("customer/{customerId}")]
    public async Task<ActionResult<ApiResponse<IEnumerable<TaskDto>>>> GetByCustomer(Guid customerId)
    {
        var tasks = await _taskService.GetByCustomerAsync(customerId);
        return Ok(ApiResponse<IEnumerable<TaskDto>>.Ok(tasks));
    }

    [HttpGet("deal/{dealId}")]
    public async Task<ActionResult<ApiResponse<IEnumerable<TaskDto>>>> GetByDeal(Guid dealId)
    {
        var tasks = await _taskService.GetByDealAsync(dealId);
        return Ok(ApiResponse<IEnumerable<TaskDto>>.Ok(tasks));
    }

    private Guid GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return Guid.Parse(userIdClaim ?? throw new UnauthorizedAccessException());
    }
}
