using System.Security.Claims;
using CRM.Application.DTOs.Common;
using CRM.Application.DTOs.Finance;
using CRM.Application.Interfaces;
using CRM.Core.Entities;
using CRM.Infrastructure.Services.Finance;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CRM.API.Controllers;

/// <summary>
/// Base cho các controller tài chính — dữ liệu giá vốn/lãi lỗ chỉ Admin + Kế toán được xem.
/// </summary>
[ApiController]
[Authorize(Roles = RoleNames.FinanceRolesCsv)]
public abstract class FinanceControllerBase : ControllerBase
{
    protected Guid GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return Guid.Parse(userIdClaim ?? throw new UnauthorizedAccessException());
    }

    protected bool IsAdmin() => User.IsInRole(RoleNames.Admin);
}

// ── Chi phí sản xuất hàng hóa ─────────────────────────────────────────────
[Route("api/finance/order-costs")]
public class OrderCostsController : FinanceControllerBase
{
    private const long MaxImportFileBytes = 5 * 1024 * 1024;

    private readonly IOrderCostService _svc;
    public OrderCostsController(IOrderCostService svc) { _svc = svc; }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<OrderCostListResultDto>>> GetList([FromQuery] OrderCostFilterDto filter)
        => Ok(ApiResponse<OrderCostListResultDto>.Ok(await _svc.GetListAsync(filter)));

    [HttpGet("{orderId}")]
    public async Task<ActionResult<ApiResponse<OrderCostListItemDto>>> GetByOrder(Guid orderId)
    {
        var x = await _svc.GetByOrderIdAsync(orderId);
        return x == null
            ? NotFound(ApiResponse<OrderCostListItemDto>.Fail("Không tìm thấy đơn hàng."))
            : Ok(ApiResponse<OrderCostListItemDto>.Ok(x));
    }

    [HttpPut("{orderId}")]
    public async Task<ActionResult<ApiResponse<OrderCostListItemDto>>> Upsert(Guid orderId, [FromBody] UpsertOrderCostDto dto)
    {
        var result = await _svc.UpsertAsync(orderId, dto, GetCurrentUserId(), IsAdmin());
        return Ok(ApiResponse<OrderCostListItemDto>.Ok(result, "Lưu chi phí thành công."));
    }

    [HttpPost("bulk")]
    public async Task<ActionResult<ApiResponse<int>>> BulkUpsert([FromBody] BulkUpsertOrderCostDto dto)
    {
        var saved = await _svc.BulkUpsertAsync(dto, GetCurrentUserId(), IsAdmin());
        return Ok(ApiResponse<int>.Ok(saved, $"Đã lưu chi phí cho {saved} đơn."));
    }

    /// <summary>Import file giá cost (.xlsx hoặc .csv). Ô trống giữ nguyên giá trị cũ.</summary>
    [HttpPost("import")]
    public async Task<ActionResult<ApiResponse<CostImportResultDto>>> Import(IFormFile file)
    {
        if (file == null || file.Length == 0)
            return BadRequest(ApiResponse<CostImportResultDto>.Fail("Chưa chọn file."));
        if (file.Length > MaxImportFileBytes)
            return BadRequest(ApiResponse<CostImportResultDto>.Fail("File vượt quá 5 MB."));

        var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (ext is not (".xlsx" or ".csv" or ".txt"))
            return BadRequest(ApiResponse<CostImportResultDto>.Fail("Chỉ hỗ trợ file .xlsx hoặc .csv."));

        await using var stream = file.OpenReadStream();
        var result = await _svc.ImportAsync(stream, file.FileName, GetCurrentUserId(), IsAdmin());

        var message = result.Errors.Count == 0
            ? $"Import thành công {result.SuccessCount}/{result.TotalRows} dòng."
            : $"Import {result.SuccessCount}/{result.TotalRows} dòng, {result.Errors.Count} dòng lỗi.";
        return Ok(ApiResponse<CostImportResultDto>.Ok(result, message));
    }

    [HttpGet("import-template")]
    public IActionResult DownloadTemplate()
        => File(OrderCostService.BuildImportTemplate(),
            "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
            "mau-gia-cost.xlsx");

    /// <summary>Đính kèm file giá cost gốc cho 1 đơn (chỉ lưu tham chiếu, không parse).</summary>
    [HttpPost("{orderId}/attachment")]
    public async Task<ActionResult<ApiResponse<OrderCostListItemDto>>> UploadAttachment(Guid orderId, IFormFile file)
    {
        if (file == null || file.Length == 0)
            return BadRequest(ApiResponse<OrderCostListItemDto>.Fail("Chưa chọn file."));
        if (file.Length > MaxImportFileBytes)
            return BadRequest(ApiResponse<OrderCostListItemDto>.Fail("File vượt quá 5 MB."));

        var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (ext is not (".xlsx" or ".xls" or ".csv" or ".pdf" or ".png" or ".jpg" or ".jpeg"))
            return BadRequest(ApiResponse<OrderCostListItemDto>.Fail("Định dạng file không hỗ trợ."));

        var uploadsRoot = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "finance");
        Directory.CreateDirectory(uploadsRoot);
        var fileName = $"{Guid.NewGuid():N}{ext}";
        var fullPath = Path.Combine(uploadsRoot, fileName);
        await using (var fs = new FileStream(fullPath, FileMode.Create))
        {
            await file.CopyToAsync(fs);
        }

        var result = await _svc.SetAttachmentAsync(orderId, $"/uploads/finance/{fileName}", file.FileName, GetCurrentUserId());
        return Ok(ApiResponse<OrderCostListItemDto>.Ok(result, "Đính kèm file thành công."));
    }
}

// ── Đầu mục chi phí cố định ───────────────────────────────────────────────
[Route("api/finance/expense-categories")]
public class ExpenseCategoriesController : FinanceControllerBase
{
    private readonly IExpenseCategoryService _svc;
    public ExpenseCategoriesController(IExpenseCategoryService svc) { _svc = svc; }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<IEnumerable<ExpenseCategoryDto>>>> GetAll([FromQuery] bool activeOnly = false)
        => Ok(ApiResponse<IEnumerable<ExpenseCategoryDto>>.Ok(await _svc.GetAllAsync(activeOnly)));

    [HttpPost]
    public async Task<ActionResult<ApiResponse<ExpenseCategoryDto>>> Create([FromBody] CreateExpenseCategoryDto dto)
        => Ok(ApiResponse<ExpenseCategoryDto>.Ok(await _svc.CreateAsync(dto), "Thêm đầu mục thành công."));

    [HttpPut("{id}")]
    public async Task<ActionResult<ApiResponse<ExpenseCategoryDto>>> Update(Guid id, [FromBody] UpdateExpenseCategoryDto dto)
    {
        if (id != dto.Id) return BadRequest(ApiResponse<ExpenseCategoryDto>.Fail("ID không khớp."));
        return Ok(ApiResponse<ExpenseCategoryDto>.Ok(await _svc.UpdateAsync(dto), "Cập nhật thành công."));
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult<ApiResponse>> Delete(Guid id)
    {
        await _svc.DeleteAsync(id);
        return Ok(ApiResponse.Ok("Xóa đầu mục thành công."));
    }
}

// ── Chi phí cố định (theo ngày) ───────────────────────────────────────────
[Route("api/finance/fixed-expenses")]
public class FixedExpensesController : FinanceControllerBase
{
    private readonly IFixedExpenseService _svc;
    public FixedExpensesController(IFixedExpenseService svc) { _svc = svc; }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<FixedExpenseListResultDto>>> GetList([FromQuery] FixedExpenseFilterDto filter)
        => Ok(ApiResponse<FixedExpenseListResultDto>.Ok(await _svc.GetListAsync(filter)));

    [HttpPost]
    public async Task<ActionResult<ApiResponse<FixedExpenseDto>>> Create([FromBody] CreateFixedExpenseDto dto)
        => Ok(ApiResponse<FixedExpenseDto>.Ok(await _svc.CreateAsync(dto, GetCurrentUserId()), "Thêm chi phí thành công."));

    [HttpPut("{id}")]
    public async Task<ActionResult<ApiResponse<FixedExpenseDto>>> Update(Guid id, [FromBody] UpdateFixedExpenseDto dto)
    {
        if (id != dto.Id) return BadRequest(ApiResponse<FixedExpenseDto>.Fail("ID không khớp."));
        return Ok(ApiResponse<FixedExpenseDto>.Ok(await _svc.UpdateAsync(dto), "Cập nhật thành công."));
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult<ApiResponse>> Delete(Guid id)
    {
        await _svc.DeleteAsync(id);
        return Ok(ApiResponse.Ok("Xóa chi phí thành công."));
    }
}

// ── Chi phí nhân sự (theo tháng) ──────────────────────────────────────────
[Route("api/finance/payroll")]
public class PayrollController : FinanceControllerBase
{
    private readonly IPayrollService _svc;
    public PayrollController(IPayrollService svc) { _svc = svc; }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<PayrollPeriodDto>>> GetPeriod([FromQuery] int year, [FromQuery] int month)
    {
        if (month is < 1 or > 12) return BadRequest(ApiResponse<PayrollPeriodDto>.Fail("Tháng không hợp lệ."));
        return Ok(ApiResponse<PayrollPeriodDto>.Ok(await _svc.GetPeriodAsync(year, month)));
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<PayrollEntryDto>>> Create([FromBody] CreatePayrollEntryDto dto)
        => Ok(ApiResponse<PayrollEntryDto>.Ok(await _svc.CreateAsync(dto, GetCurrentUserId()), "Thêm dòng lương thành công."));

    [HttpPut("{id}")]
    public async Task<ActionResult<ApiResponse<PayrollEntryDto>>> Update(Guid id, [FromBody] UpdatePayrollEntryDto dto)
    {
        if (id != dto.Id) return BadRequest(ApiResponse<PayrollEntryDto>.Fail("ID không khớp."));
        return Ok(ApiResponse<PayrollEntryDto>.Ok(await _svc.UpdateAsync(dto), "Cập nhật thành công."));
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult<ApiResponse>> Delete(Guid id)
    {
        await _svc.DeleteAsync(id);
        return Ok(ApiResponse.Ok("Xóa dòng lương thành công."));
    }

    [HttpPost("copy-from-previous")]
    public async Task<ActionResult<ApiResponse<int>>> CopyFromPrevious([FromQuery] int year, [FromQuery] int month)
    {
        var added = await _svc.CopyFromPreviousMonthAsync(year, month, GetCurrentUserId());
        return Ok(ApiResponse<int>.Ok(added, $"Đã sao chép {added} dòng lương từ tháng trước."));
    }
}

// ── Báo cáo lãi/lỗ ────────────────────────────────────────────────────────
[Route("api/finance/reports")]
public class ProfitReportController : FinanceControllerBase
{
    private readonly IProfitReportService _svc;
    public ProfitReportController(IProfitReportService svc) { _svc = svc; }

    [HttpGet("order-profit")]
    public async Task<ActionResult<ApiResponse<OrderProfitResultDto>>> GetOrderProfit([FromQuery] ProfitFilterDto filter)
        => Ok(ApiResponse<OrderProfitResultDto>.Ok(await _svc.GetOrderProfitAsync(filter)));

    [HttpGet("monthly-profit")]
    public async Task<ActionResult<ApiResponse<MonthlyProfitResultDto>>> GetMonthlyProfit(
        [FromQuery] int? year, [FromQuery] string? revenueBasis)
    {
        var y = year ?? DateTime.UtcNow.Year;
        return Ok(ApiResponse<MonthlyProfitResultDto>.Ok(await _svc.GetMonthlyProfitAsync(y, revenueBasis)));
    }

    [HttpGet("monthly-profit/{year}/{month}/detail")]
    public async Task<ActionResult<ApiResponse<MonthlyProfitDetailDto>>> GetMonthDetail(
        int year, int month, [FromQuery] string? revenueBasis)
        => Ok(ApiResponse<MonthlyProfitDetailDto>.Ok(await _svc.GetMonthDetailAsync(year, month, revenueBasis)));
}
