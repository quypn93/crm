using CRM.Core.Entities;
using CRM.Core.Interfaces.Repositories;
using CRM.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace CRM.Infrastructure.Repositories;

public class ProvinceRepository : IProvinceRepository
{
    private readonly CrmDbContext _ctx;
    public ProvinceRepository(CrmDbContext ctx) { _ctx = ctx; }

    public async Task<IEnumerable<Province>> GetAllAsync() =>
        await _ctx.Provinces.AsNoTracking().ToListAsync();

    public async Task<Province?> GetByCodeAsync(string code) =>
        await _ctx.Provinces.AsNoTracking().FirstOrDefaultAsync(p => p.Code == code);

    public async Task<bool> AnyAsync() =>
        await _ctx.Provinces.AnyAsync();

    public async Task AddRangeAsync(IEnumerable<Province> items) =>
        await _ctx.Provinces.AddRangeAsync(items);
}

public class WardRepository : IWardRepository
{
    private readonly CrmDbContext _ctx;
    public WardRepository(CrmDbContext ctx) { _ctx = ctx; }

    public async Task<IEnumerable<Ward>> GetByProvinceAsync(string provinceCode) =>
        await _ctx.Wards.AsNoTracking().Where(w => w.ProvinceCode == provinceCode).ToListAsync();

    public async Task<Ward?> GetByCodeAsync(string code) =>
        await _ctx.Wards.AsNoTracking().FirstOrDefaultAsync(w => w.Code == code);

    public async Task<bool> AnyAsync() =>
        await _ctx.Wards.AnyAsync();

    public async Task AddRangeAsync(IEnumerable<Ward> items) =>
        await _ctx.Wards.AddRangeAsync(items);
}
