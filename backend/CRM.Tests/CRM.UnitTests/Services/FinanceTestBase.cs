using CRM.Core.Entities;
using CRM.Core.Enums;
using CRM.Infrastructure.Data;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace CRM.UnitTests.Services;

/// <summary>
/// DB SQLite in-memory cho các test tài chính — provider quan hệ nên giữ được index/precision
/// gần với PostgreSQL thật hơn là InMemory provider.
/// </summary>
public abstract class FinanceTestBase : IDisposable
{
    private readonly SqliteConnection _connection;
    protected readonly CrmDbContext Db;

    protected static readonly Guid AccountantId = Guid.Parse("aaaaaaaa-0000-0000-0000-000000000001");

    protected FinanceTestBase()
    {
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();

        var options = new DbContextOptionsBuilder<CrmDbContext>()
            .UseSqlite(_connection)
            .Options;

        Db = new CrmDbContext(options);
        Db.Database.EnsureCreated();

        Db.Users.Add(NewUser(AccountantId, "Ke", "Toan"));
        Db.SaveChanges();
    }

    protected static User NewUser(Guid id, string first, string last) => new()
    {
        Id = id,
        Email = $"{id:N}@crm.com",
        PasswordHash = "x",
        FirstName = first,
        LastName = last,
        IsActive = true
    };

    /// <summary>Đơn hàng mặc định: đã lên sản xuất (được tính vào báo cáo chi phí).</summary>
    protected Order AddOrder(
        string orderNumber,
        decimal total,
        DateTime confirmedDate,
        OrderStatus status = OrderStatus.InProduction,
        string customerName = "Khách A")
    {
        var order = new Order
        {
            OrderNumber = orderNumber,
            CustomerName = customerName,
            Status = status,
            TotalAmount = total,
            SubTotal = total,
            OrderDate = confirmedDate,
            ConfirmedDate = confirmedDate,
            CreatedByUserId = AccountantId
        };
        Db.Orders.Add(order);
        Db.SaveChanges();
        return order;
    }

    protected OrderCost AddCost(Guid orderId, decimal cost, decimal ship = 0, decimal outbound = 0, decimal other = 0)
    {
        var entity = new OrderCost
        {
            OrderId = orderId,
            CostAmount = cost,
            ShippingCost = ship,
            OutboundShippingCost = outbound,
            OtherCost = other,
            EnteredByUserId = AccountantId,
            EnteredAt = DateTime.UtcNow
        };
        entity.RecalculateTotal();
        Db.OrderCosts.Add(entity);
        Db.SaveChanges();
        return entity;
    }

    protected ExpenseCategory AddCategory(string name, bool isSystem = false)
    {
        var c = new ExpenseCategory { Name = name, IsActive = true, IsSystem = isSystem };
        Db.ExpenseCategories.Add(c);
        Db.SaveChanges();
        return c;
    }

    protected FixedExpense AddFixedExpense(ExpenseCategory category, DateOnly date, decimal amount)
    {
        var e = new FixedExpense
        {
            ExpenseDate = date,
            ExpenseCategoryId = category.Id,
            CategoryName = category.Name,
            Amount = amount,
            CreatedByUserId = AccountantId
        };
        Db.FixedExpenses.Add(e);
        Db.SaveChanges();
        return e;
    }

    protected PayrollEntry AddPayroll(int year, int month, string name, decimal salary, decimal allowance = 0,
        decimal insurance = 0, decimal other = 0, Guid? userId = null)
    {
        var e = new PayrollEntry
        {
            Year = year,
            Month = month,
            UserId = userId,
            EmployeeName = name,
            Salary = salary,
            Allowance = allowance,
            Insurance = insurance,
            OtherCost = other
        };
        e.RecalculateTotal();
        Db.PayrollEntries.Add(e);
        Db.SaveChanges();
        return e;
    }

    /// <summary>Mốc UTC tương ứng 12h trưa giờ VN — nằm chắc trong ngày dù lệch múi giờ.</summary>
    protected static DateTime VnNoonUtc(int year, int month, int day)
        => new DateTime(year, month, day, 12, 0, 0, DateTimeKind.Utc).AddHours(-7);

    public void Dispose()
    {
        Db.Dispose();
        _connection.Dispose();
        GC.SuppressFinalize(this);
    }
}
