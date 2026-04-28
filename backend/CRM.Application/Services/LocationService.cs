using AutoMapper;
using CRM.Application.DTOs.Location;
using CRM.Application.Interfaces;
using CRM.Core.Interfaces;

namespace CRM.Application.Services;

public class LocationService : ILocationService
{
    private readonly IUnitOfWork _uow;
    private readonly IMapper _mapper;

    public LocationService(IUnitOfWork uow, IMapper mapper)
    {
        _uow = uow;
        _mapper = mapper;
    }

    public async Task<IEnumerable<ProvinceDto>> GetProvincesAsync()
    {
        var provinces = await _uow.Provinces.GetAllAsync();
        return _mapper.Map<IEnumerable<ProvinceDto>>(
            provinces.OrderBy(p => p.SortOrder).ThenBy(p => p.Name));
    }

    public async Task<IEnumerable<WardDto>> GetWardsByProvinceAsync(string provinceCode)
    {
        var wards = await _uow.Wards.GetByProvinceAsync(provinceCode);
        return _mapper.Map<IEnumerable<WardDto>>(wards.OrderBy(w => w.Name));
    }
}
