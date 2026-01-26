# CRM Website Implementation Plan

## Cấu trúc thư mục cần tạo

```
d:\Task\CRM\
├── plan/                          # Thư mục chứa các file plan
│   └── CRM-Implementation-Plan.md
│
├── backend/                       # .NET Backend
│   ├── CRM.API/
│   │   ├── Controllers/
│   │   ├── Middlewares/
│   │   ├── Filters/
│   │   └── Extensions/
│   ├── CRM.Core/
│   │   ├── Entities/
│   │   ├── Interfaces/
│   │   │   ├── Repositories/
│   │   │   └── Services/
│   │   ├── Enums/
│   │   └── Exceptions/
│   ├── CRM.Application/
│   │   ├── Services/
│   │   ├── DTOs/
│   │   │   ├── Auth/
│   │   │   ├── Customer/
│   │   │   ├── Deal/
│   │   │   ├── Task/
│   │   │   ├── Report/
│   │   │   ├── User/
│   │   │   └── Common/
│   │   ├── Mappings/
│   │   └── Validators/
│   ├── CRM.Infrastructure/
│   │   ├── Data/
│   │   │   ├── Configurations/
│   │   │   └── Migrations/
│   │   ├── Repositories/
│   │   ├── Services/
│   │   └── Extensions/
│   └── CRM.Tests/
│       ├── CRM.UnitTests/
│       └── CRM.IntegrationTests/
│
└── frontend/                      # Angular Frontend
    └── crm-app/
        └── src/
            ├── app/
            │   ├── core/
            │   │   ├── services/
            │   │   ├── guards/
            │   │   ├── interceptors/
            │   │   └── models/
            │   ├── shared/
            │   │   ├── components/
            │   │   ├── directives/
            │   │   └── pipes/
            │   ├── features/
            │   │   ├── auth/
            │   │   │   ├── components/
            │   │   │   └── services/
            │   │   ├── dashboard/
            │   │   │   ├── components/
            │   │   │   └── services/
            │   │   ├── customers/
            │   │   │   ├── components/
            │   │   │   └── services/
            │   │   ├── deals/
            │   │   │   ├── components/
            │   │   │   └── services/
            │   │   ├── tasks/
            │   │   │   ├── components/
            │   │   │   └── services/
            │   │   ├── reports/
            │   │   │   ├── components/
            │   │   │   └── services/
            │   │   └── settings/
            │   │       ├── components/
            │   │       └── services/
            │   └── layout/
            ├── assets/
            │   ├── images/
            │   ├── icons/
            │   └── i18n/
            ├── environments/
            └── styles/
```

---

## Tech Stack
- **Backend**: ASP.NET Core 8 Web API
- **Frontend**: Angular 17
- **Database**: Microsoft SQL Server
- **Authentication**: JWT với Refresh Token

---

## Cấu trúc Project

### Backend (.NET)
```
CRM.Backend/
├── CRM.API/              # Controllers, Middlewares
├── CRM.Core/             # Entities, Interfaces, Enums
├── CRM.Application/      # Services, DTOs, Validators
├── CRM.Infrastructure/   # DbContext, Repositories
└── CRM.Tests/            # Unit & Integration tests
```

### Frontend (Angular)
```
crm-frontend/
├── src/app/
│   ├── core/             # Services, Guards, Interceptors
│   ├── shared/           # Shared components, pipes
│   ├── features/         # Feature modules (lazy-loaded)
│   │   ├── auth/
│   │   ├── dashboard/
│   │   ├── customers/
│   │   ├── deals/
│   │   ├── tasks/
│   │   └── reports/
│   └── layout/           # Main layout, sidebar
```

---

## Database Schema

### Bảng chính:
1. **Users** - Người dùng (Email, Password, FirstName, LastName)
2. **Roles** - Vai trò (Admin, SalesManager, SalesRep)
3. **Customers** - Khách hàng (Name, Email, Phone, Company, Address)
4. **DealStages** - Các giai đoạn deal (Lead, Qualified, Proposal, Negotiation, Won, Lost)
5. **Deals** - Giao dịch (Title, Value, CustomerId, StageId, ExpectedCloseDate)
6. **Tasks** - Tác vụ (Title, DueDate, Priority, Status, CustomerId, DealId)
7. **ActivityLogs** - Lịch sử hoạt động

### Quan hệ:
- User ↔ Roles (nhiều-nhiều)
- Customer → User (AssignedTo)
- Deal → Customer, DealStage, User
- Task → Customer, Deal, User

---

## API Endpoints

### Authentication
- `POST /api/auth/register` - Đăng ký
- `POST /api/auth/login` - Đăng nhập
- `POST /api/auth/refresh-token` - Làm mới token
- `GET /api/auth/me` - Lấy thông tin user hiện tại

### Customers
- `GET /api/customers` - Danh sách (phân trang, filter)
- `GET /api/customers/{id}` - Chi tiết
- `POST /api/customers` - Tạo mới
- `PUT /api/customers/{id}` - Cập nhật
- `DELETE /api/customers/{id}` - Xóa

### Deals
- `GET /api/deals` - Danh sách
- `GET /api/deals/pipeline` - Dữ liệu Kanban board
- `PUT /api/deals/{id}/stage` - Cập nhật stage (drag-drop)
- `POST /api/deals/{id}/won` - Đánh dấu thắng
- `POST /api/deals/{id}/lost` - Đánh dấu thua

### Tasks
- `GET /api/tasks` - Danh sách
- `GET /api/tasks/my-tasks` - Tasks của user hiện tại
- `GET /api/tasks/overdue` - Tasks quá hạn
- CRUD endpoints chuẩn

### Dashboard & Reports
- `GET /api/dashboard` - Tổng quan
- `GET /api/reports/revenue` - Báo cáo doanh thu
- `GET /api/reports/deals/by-stage` - Thống kê deals theo stage

---

## Các Phase triển khai

### Phase 1: Foundation & Authentication
- Setup .NET solution với Clean Architecture
- Cấu hình Entity Framework + SQL Server
- Implement JWT authentication
- Tạo Angular project + core services
- Login/Register UI

### Phase 2: Customer Management
- Customer CRUD API
- Customer list với search, filter, pagination
- Customer form (add/edit)
- Customer detail view

### Phase 3: Deal Management
- Deal CRUD API
- Kanban board với drag-and-drop
- Pipeline stages management
- Deal-Customer relationship

### Phase 4: Task Management
- Task CRUD API
- Task list với filters
- Due date tracking
- Task-Customer-Deal relationships

### Phase 5: Dashboard & Reports
- Dashboard overview với stats cards
- Revenue charts (Chart.js)
- Deal conversion charts
- Export to Excel/PDF

### Phase 6: Polish & Deployment
- Error handling & loading states
- Responsive design
- Unit tests
- Deployment configuration

---

## Key Packages

### Backend (.NET)
- `Microsoft.EntityFrameworkCore.SqlServer` - ORM
- `Microsoft.AspNetCore.Authentication.JwtBearer` - JWT Auth
- `AutoMapper` - Object mapping
- `FluentValidation` - Validation
- `Serilog` - Logging
- `ClosedXML` - Excel export

### Frontend (Angular)
- `@angular/material` hoặc `primeng` - UI components
- `@angular/cdk/drag-drop` - Kanban drag-drop
- `ng2-charts` + `chart.js` - Charts
- `ngx-toastr` - Notifications
- `ngx-pagination` - Pagination

---

## Verification Plan

1. **Authentication**:
   - Đăng ký user mới → Login → Verify JWT token
   - Test refresh token flow

2. **Customers**:
   - Tạo, sửa, xóa customer
   - Test search và filter
   - Verify pagination

3. **Deals**:
   - Tạo deal mới liên kết với customer
   - Test drag-drop trên Kanban board
   - Verify stage transitions

4. **Tasks**:
   - Tạo task gắn với customer/deal
   - Test status updates
   - Verify due date filtering

5. **Dashboard**:
   - Verify số liệu thống kê chính xác
   - Test date range filters trên reports
   - Export report và verify data

---

## Files quan trọng cần tạo

### Backend
- `CRM.Infrastructure/Data/CrmDbContext.cs`
- `CRM.Core/Entities/*.cs` (User, Customer, Deal, Task)
- `CRM.Application/Services/AuthService.cs`
- `CRM.API/Controllers/*.cs`

### Frontend
- `src/app/core/services/auth.service.ts`
- `src/app/core/interceptors/auth.interceptor.ts`
- `src/app/features/deals/components/deal-kanban/`
- `src/app/features/dashboard/components/`
