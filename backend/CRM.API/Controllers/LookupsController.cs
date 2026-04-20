using CRM.Application.DTOs.Common;
using CRM.Application.DTOs.Lookup;
using CRM.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CRM.API.Controllers;

[ApiController]
[Route("api/collections")]
[Authorize]
public class CollectionsController : ControllerBase
{
    private readonly ICollectionService _svc;
    public CollectionsController(ICollectionService svc) { _svc = svc; }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<IEnumerable<CollectionDto>>>> GetAll()
        => Ok(ApiResponse<IEnumerable<CollectionDto>>.Ok(await _svc.GetAllAsync()));

    [HttpGet("{id}")]
    public async Task<ActionResult<ApiResponse<CollectionDto>>> GetById(Guid id)
    {
        var c = await _svc.GetByIdAsync(id);
        return c == null ? NotFound(ApiResponse<CollectionDto>.Fail("Không tìm thấy.")) : Ok(ApiResponse<CollectionDto>.Ok(c));
    }

    [HttpPost]
    public async Task<ActionResult<ApiResponse<CollectionDto>>> Create([FromBody] CreateCollectionDto dto)
        => Ok(ApiResponse<CollectionDto>.Ok(await _svc.CreateAsync(dto), "Tạo bộ sưu tập thành công."));

    [HttpPut("{id}")]
    public async Task<ActionResult<ApiResponse<CollectionDto>>> Update(Guid id, [FromBody] UpdateCollectionDto dto)
    {
        if (id != dto.Id) return BadRequest(ApiResponse<CollectionDto>.Fail("ID không khớp."));
        return Ok(ApiResponse<CollectionDto>.Ok(await _svc.UpdateAsync(dto), "Cập nhật thành công."));
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult<ApiResponse>> Delete(Guid id)
    {
        await _svc.DeleteAsync(id);
        return Ok(ApiResponse.Ok("Xóa thành công."));
    }
}

public abstract class SimpleLookupControllerBase<TService> : ControllerBase where TService : class
{
    protected abstract Task<IEnumerable<LookupItemDto>> GetAllCore();
    protected abstract Task<LookupItemDto> CreateCore(CreateLookupItemDto dto);
    protected abstract Task<LookupItemDto> UpdateCore(UpdateLookupItemDto dto);
    protected abstract Task DeleteCore(Guid id);

    [HttpGet]
    public async Task<ActionResult<ApiResponse<IEnumerable<LookupItemDto>>>> GetAll()
        => Ok(ApiResponse<IEnumerable<LookupItemDto>>.Ok(await GetAllCore()));

    [HttpPost]
    public async Task<ActionResult<ApiResponse<LookupItemDto>>> Create([FromBody] CreateLookupItemDto dto)
        => Ok(ApiResponse<LookupItemDto>.Ok(await CreateCore(dto), "Tạo thành công."));

    [HttpPut("{id}")]
    public async Task<ActionResult<ApiResponse<LookupItemDto>>> Update(Guid id, [FromBody] UpdateLookupItemDto dto)
    {
        if (id != dto.Id) return BadRequest(ApiResponse<LookupItemDto>.Fail("ID không khớp."));
        return Ok(ApiResponse<LookupItemDto>.Ok(await UpdateCore(dto), "Cập nhật thành công."));
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult<ApiResponse>> Delete(Guid id)
    {
        await DeleteCore(id);
        return Ok(ApiResponse.Ok("Xóa thành công."));
    }
}

[ApiController]
[Route("api/materials")]
[Authorize]
public class MaterialsController : SimpleLookupControllerBase<IMaterialService>
{
    private readonly IMaterialService _svc;
    public MaterialsController(IMaterialService svc) { _svc = svc; }
    protected override Task<IEnumerable<LookupItemDto>> GetAllCore() => _svc.GetAllAsync();
    protected override Task<LookupItemDto> CreateCore(CreateLookupItemDto dto) => _svc.CreateAsync(dto);
    protected override Task<LookupItemDto> UpdateCore(UpdateLookupItemDto dto) => _svc.UpdateAsync(dto);
    protected override Task DeleteCore(Guid id) => _svc.DeleteAsync(id);
}

[ApiController]
[Route("api/product-forms")]
[Authorize]
public class ProductFormsController : SimpleLookupControllerBase<IProductFormService>
{
    private readonly IProductFormService _svc;
    public ProductFormsController(IProductFormService svc) { _svc = svc; }
    protected override Task<IEnumerable<LookupItemDto>> GetAllCore() => _svc.GetAllAsync();
    protected override Task<LookupItemDto> CreateCore(CreateLookupItemDto dto) => _svc.CreateAsync(dto);
    protected override Task<LookupItemDto> UpdateCore(UpdateLookupItemDto dto) => _svc.UpdateAsync(dto);
    protected override Task DeleteCore(Guid id) => _svc.DeleteAsync(id);
}

[ApiController]
[Route("api/product-specifications")]
[Authorize]
public class ProductSpecificationsController : SimpleLookupControllerBase<IProductSpecificationService>
{
    private readonly IProductSpecificationService _svc;
    public ProductSpecificationsController(IProductSpecificationService svc) { _svc = svc; }
    protected override Task<IEnumerable<LookupItemDto>> GetAllCore() => _svc.GetAllAsync();
    protected override Task<LookupItemDto> CreateCore(CreateLookupItemDto dto) => _svc.CreateAsync(dto);
    protected override Task<LookupItemDto> UpdateCore(UpdateLookupItemDto dto) => _svc.UpdateAsync(dto);
    protected override Task DeleteCore(Guid id) => _svc.DeleteAsync(id);
}

[ApiController]
[Route("api/production-days-options")]
[Authorize]
public class ProductionDaysOptionsController : ControllerBase
{
    private readonly IProductionDaysOptionService _svc;
    public ProductionDaysOptionsController(IProductionDaysOptionService svc) { _svc = svc; }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<IEnumerable<ProductionDaysOptionDto>>>> GetAll()
        => Ok(ApiResponse<IEnumerable<ProductionDaysOptionDto>>.Ok(await _svc.GetAllAsync()));

    [HttpPost]
    public async Task<ActionResult<ApiResponse<ProductionDaysOptionDto>>> Create([FromBody] CreateProductionDaysOptionDto dto)
        => Ok(ApiResponse<ProductionDaysOptionDto>.Ok(await _svc.CreateAsync(dto), "Tạo thành công."));

    [HttpPut("{id}")]
    public async Task<ActionResult<ApiResponse<ProductionDaysOptionDto>>> Update(Guid id, [FromBody] UpdateProductionDaysOptionDto dto)
    {
        if (id != dto.Id) return BadRequest(ApiResponse<ProductionDaysOptionDto>.Fail("ID không khớp."));
        return Ok(ApiResponse<ProductionDaysOptionDto>.Ok(await _svc.UpdateAsync(dto), "Cập nhật thành công."));
    }

    [HttpDelete("{id}")]
    public async Task<ActionResult<ApiResponse>> Delete(Guid id)
    {
        await _svc.DeleteAsync(id);
        return Ok(ApiResponse.Ok("Xóa thành công."));
    }
}

[ApiController]
[Route("api/deposits")]
public class DepositsController : ControllerBase
{
    private readonly IDepositTransactionService _svc;
    private readonly IConfiguration _config;
    public DepositsController(IDepositTransactionService svc, IConfiguration config) { _svc = svc; _config = config; }

    [HttpGet]
    [Authorize]
    public async Task<ActionResult<ApiResponse<IEnumerable<DepositTransactionDto>>>> GetAll()
        => Ok(ApiResponse<IEnumerable<DepositTransactionDto>>.Ok(await _svc.GetAllAsync()));

    [HttpPost]
    [Authorize]
    public async Task<ActionResult<ApiResponse<DepositTransactionDto>>> Create([FromBody] CreateDepositTransactionDto dto)
        => Ok(ApiResponse<DepositTransactionDto>.Ok(await _svc.CreateAsync(dto), "Tạo thành công."));

    [HttpDelete("{id}")]
    [Authorize]
    public async Task<ActionResult<ApiResponse>> Delete(Guid id)
    {
        await _svc.DeleteAsync(id);
        return Ok(ApiResponse.Ok("Xóa thành công."));
    }

    // SePay webhook: POST /api/deposits/sepay-webhook
    // SePay gửi header `Authorization: Apikey <YOUR_API_KEY>` — config tại SePay:ApiKey
    [HttpPost("sepay-webhook")]
    [AllowAnonymous]
    public async Task<IActionResult> SePayWebhook([FromBody] SePayWebhookPayload payload, [FromHeader(Name = "Authorization")] string? authorization)
    {
        var expectedKey = _config["SePay:ApiKey"];
        if (!string.IsNullOrEmpty(expectedKey))
        {
            var expected = $"Apikey {expectedKey}";
            if (authorization != expected)
                return Unauthorized(new { error = "Invalid API key" });
        }

        await _svc.HandleSePayWebhookAsync(payload);
        return Ok(new { success = true });
    }
}
