using CRM.Core.Entities;

namespace CRM.Core.Interfaces.Repositories;

public interface IProvinceRepository
{
    Task<IEnumerable<Province>> GetAllAsync();
    Task<Province?> GetByCodeAsync(string code);
    Task<bool> AnyAsync();
    Task AddRangeAsync(IEnumerable<Province> items);
}

public interface IWardRepository
{
    Task<IEnumerable<Ward>> GetByProvinceAsync(string provinceCode);
    Task<Ward?> GetByCodeAsync(string code);
    Task<bool> AnyAsync();
    Task AddRangeAsync(IEnumerable<Ward> items);
}
