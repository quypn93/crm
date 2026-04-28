using CRM.Application.DTOs.Location;

namespace CRM.Application.Interfaces;

public interface ILocationService
{
    Task<IEnumerable<ProvinceDto>> GetProvincesAsync();
    Task<IEnumerable<WardDto>> GetWardsByProvinceAsync(string provinceCode);
}
