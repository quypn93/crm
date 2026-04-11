using CRM.Core.Entities;

namespace CRM.Core.Interfaces.Repositories;

public interface IProductAttributeRepository : IRepository<ProductAttribute>
{
    Task<IEnumerable<ProductAttribute>> GetByTypeAsync(string type, bool activeOnly = true);
}
