using CRM.Application.DTOs.Common;
using CRM.Application.DTOs.Location;
using CRM.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CRM.API.Controllers;

[ApiController]
[Route("api/locations")]
[Authorize]
public class LocationsController : ControllerBase
{
    private readonly ILocationService _svc;

    public LocationsController(ILocationService svc)
    {
        _svc = svc;
    }

    [HttpGet("provinces")]
    public async Task<ActionResult<ApiResponse<IEnumerable<ProvinceDto>>>> GetProvinces()
        => Ok(ApiResponse<IEnumerable<ProvinceDto>>.Ok(await _svc.GetProvincesAsync()));

    [HttpGet("wards")]
    public async Task<ActionResult<ApiResponse<IEnumerable<WardDto>>>> GetWards([FromQuery] string provinceCode)
    {
        if (string.IsNullOrWhiteSpace(provinceCode))
            return BadRequest(ApiResponse<IEnumerable<WardDto>>.Fail("Thiếu mã tỉnh/thành phố."));
        return Ok(ApiResponse<IEnumerable<WardDto>>.Ok(await _svc.GetWardsByProvinceAsync(provinceCode)));
    }
}
