using AutoMapper;
using CRM.Application.DTOs.Common;
using CRM.Application.DTOs.Customer;
using CRM.Core.Entities;
using CRM.Core.Interfaces;
using CRM.Core.Interfaces.Services;

namespace CRM.Application.Services;

public class CustomerService : ICustomerService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public CustomerService(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<CustomerDto?> GetByIdAsync(Guid id)
    {
        var customer = await _unitOfWork.Customers.GetByIdWithDetailsAsync(id);
        return customer != null ? _mapper.Map<CustomerDto>(customer) : null;
    }

    public async Task<PaginatedResult<CustomerDto>> GetPagedAsync(CustomerFilterDto filter)
    {
        var (items, totalCount) = await _unitOfWork.Customers.GetPagedAsync(
            filter.Search,
            filter.AssignedTo,
            filter.IsActive,
            filter.Industry,
            filter.City,
            filter.CreatedFrom,
            filter.CreatedTo,
            filter.Page,
            filter.PageSize,
            filter.SortBy,
            filter.SortOrder);

        var dtos = _mapper.Map<List<CustomerDto>>(items);
        return PaginatedResult<CustomerDto>.Create(dtos, totalCount, filter.Page, filter.PageSize);
    }

    public async Task<CustomerDto> CreateAsync(CreateCustomerDto dto, Guid userId)
    {
        var customer = _mapper.Map<Customer>(dto);
        customer.CreatedByUserId = userId;

        // If no assigned user specified, assign to creator
        if (!dto.AssignedToUserId.HasValue)
        {
            customer.AssignedToUserId = userId;
        }

        await _unitOfWork.Customers.AddAsync(customer);
        await _unitOfWork.SaveChangesAsync();

        return await GetByIdAsync(customer.Id) ?? throw new InvalidOperationException("Không thể tạo khách hàng.");
    }

    public async Task<CustomerDto> UpdateAsync(UpdateCustomerDto dto, Guid userId)
    {
        var customer = await _unitOfWork.Customers.GetByIdAsync(dto.Id);

        if (customer == null)
        {
            throw new KeyNotFoundException("Không tìm thấy khách hàng.");
        }

        _mapper.Map(dto, customer);
        _unitOfWork.Customers.Update(customer);
        await _unitOfWork.SaveChangesAsync();

        return await GetByIdAsync(customer.Id) ?? throw new InvalidOperationException("Không thể cập nhật khách hàng.");
    }

    public async Task DeleteAsync(Guid id, Guid userId)
    {
        var customer = await _unitOfWork.Customers.GetByIdAsync(id);

        if (customer == null)
        {
            throw new KeyNotFoundException("Không tìm thấy khách hàng.");
        }

        // Soft delete - just set IsActive to false
        customer.IsActive = false;
        _unitOfWork.Customers.Update(customer);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task<IEnumerable<CustomerDto>> GetByAssignedUserAsync(Guid userId)
    {
        var customers = await _unitOfWork.Customers.GetByAssignedUserAsync(userId);
        return _mapper.Map<IEnumerable<CustomerDto>>(customers);
    }
}
