using CRM.Application.DTOs.Finance;

namespace CRM.Application.Interfaces;

public interface IOrderCostService
{
    Task<OrderCostListResultDto> GetListAsync(OrderCostFilterDto filter);
    Task<OrderCostListItemDto?> GetByOrderIdAsync(Guid orderId);
    Task<OrderCostListItemDto> UpsertAsync(Guid orderId, UpsertOrderCostDto dto, Guid userId, bool isAdmin);
    Task<int> BulkUpsertAsync(BulkUpsertOrderCostDto dto, Guid userId, bool isAdmin);
    Task<OrderCostListItemDto> SetAttachmentAsync(Guid orderId, string url, string fileName, Guid userId);
    /// <summary>Đọc file CSV/TSV giá cost và áp vào các đơn khớp mã. Không rollback dòng lỗi.</summary>
    Task<CostImportResultDto> ImportAsync(Stream fileStream, string fileName, Guid userId, bool isAdmin);
}

public interface IExpenseCategoryService
{
    Task<IEnumerable<ExpenseCategoryDto>> GetAllAsync(bool activeOnly = false);
    Task<ExpenseCategoryDto> CreateAsync(CreateExpenseCategoryDto dto);
    Task<ExpenseCategoryDto> UpdateAsync(UpdateExpenseCategoryDto dto);
    Task DeleteAsync(Guid id);
}

public interface IFixedExpenseService
{
    Task<FixedExpenseListResultDto> GetListAsync(FixedExpenseFilterDto filter);
    Task<FixedExpenseDto> CreateAsync(CreateFixedExpenseDto dto, Guid userId);
    Task<FixedExpenseDto> UpdateAsync(UpdateFixedExpenseDto dto);
    Task DeleteAsync(Guid id);
}

public interface IPayrollService
{
    Task<PayrollPeriodDto> GetPeriodAsync(int year, int month);
    Task<PayrollEntryDto> CreateAsync(CreatePayrollEntryDto dto, Guid userId);
    Task<PayrollEntryDto> UpdateAsync(UpdatePayrollEntryDto dto);
    Task DeleteAsync(Guid id);
    /// <summary>Sao chép toàn bộ bảng lương tháng liền trước sang kỳ (year, month). Trả số dòng đã tạo.</summary>
    Task<int> CopyFromPreviousMonthAsync(int year, int month, Guid userId);
}

public interface IProfitReportService
{
    Task<OrderProfitResultDto> GetOrderProfitAsync(ProfitFilterDto filter);
    Task<MonthlyProfitResultDto> GetMonthlyProfitAsync(int year, string? revenueBasis);
    Task<MonthlyProfitDetailDto> GetMonthDetailAsync(int year, int month, string? revenueBasis);
}
