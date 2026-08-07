# Kế hoạch: Module Chi phí & Báo cáo Lãi/Lỗ

**Ngày lập:** 2026-08-01
**Trạng thái:** ✅ ĐÃ TRIỂN KHAI (2026-08-01) — xem §12 để biết khác biệt so với plan gốc
**Stack thực tế:** ASP.NET Core (net9.0, Clean Architecture) + EF Core 9 / PostgreSQL + Angular 17

---

## 1. Yêu cầu gốc (từ chat khách hàng)

1. **Chi phí sản xuất hàng hóa** — đơn hàng lên mục sản xuất ở trạng thái *đang SX* sẽ tự nhảy vào đây để kế toán nhập, hoặc tải file giá cost của sản phẩm lên.
   Cột: **giá cost**, **chi phí ship hàng**, **chi phí gửi hàng đi**.
2. **Chi phí nhân sự** — Lương, phụ cấp, bảo hiểm, chi phí khác. **Nhập theo tháng.**
3. **Chi phí cố định** — tiền thuê nhà, điện nước, mạng internet, ăn uống, gửi xe, ship nội bộ, chi phí khác. **Đầu mục phải cấu hình được để kế toán tự thêm.** **Nhập theo ngày.**
4. **Báo cáo lãi/lỗ** — tài khoản Admin và Kế toán xem được: lãi/lỗ **theo từng đơn hàng** và **tổng theo tháng**.

---

## 2. Mô hình dữ liệu

```
Order (đã có)  ──1:1──  OrderCost          ← chi phí gắn trực tiếp vào đơn
                          └─0:N── OrderCostItem  (tùy chọn, cost theo dòng SP)

PayrollEntry        ← chi phí nhân sự, khóa (Year, Month, nhân sự)
ExpenseCategory     ← đầu mục chi phí cố định (kế toán tự thêm)
FixedExpense        ← 1 dòng chi = 1 ngày + 1 đầu mục + số tiền
```

Nguyên tắc: **chi phí trực tiếp** (OrderCost) phân bổ được về từng đơn → tính lãi/lỗ đơn hàng.
**Chi phí gián tiếp** (nhân sự + cố định) không phân bổ về đơn → chỉ trừ ở báo cáo tháng.

### 2.1 `OrderCost` — CRM.Core/Entities/OrderCost.cs

| Field | Kiểu | Ghi chú |
|---|---|---|
| `OrderId` | Guid, FK unique → Orders | 1 đơn 1 bản ghi cost |
| `CostAmount` | decimal(18,2) | Giá cost (giá vốn hàng hóa) |
| `ShippingCost` | decimal(18,2) | Chi phí ship hàng |
| `OutboundShippingCost` | decimal(18,2) | Chi phí gửi hàng đi |
| `OtherCost` | decimal(18,2) | Phát sinh khác (đệm cho các khoản lặt vặt) |
| `TotalCost` | decimal(18,2) | Cột tính sẵn = tổng 4 cột trên, ghi khi save |
| `CostFileUrl` / `CostFileName` | string? | File giá cost kế toán upload đính kèm đơn |
| `Notes` | string?(1000) | |
| `EnteredByUserId` | Guid? FK → Users | Ai nhập gần nhất |
| `EnteredAt` | DateTime? | |
| `IsFinalized` | bool | Chốt sổ — khóa không cho sửa (trừ Admin) |

Kế thừa `BaseEntity` (Id, CreatedAt, UpdatedAt…) như các entity khác.
Index: unique trên `OrderId`.

### 2.2 `OrderCostItem` (tùy chọn — Phase 6, chỉ làm nếu cần cost theo từng dòng SP)

`OrderCostId`, `OrderItemId` (FK), `UnitCost` decimal(18,2), `Quantity` (snapshot), `LineCost`.
Khi có dòng chi tiết → `OrderCost.CostAmount` = Σ `LineCost` (readonly ở UI).

### 2.3 `PayrollEntry` — chi phí nhân sự theo tháng

| Field | Kiểu | Ghi chú |
|---|---|---|
| `Year`, `Month` | int | Kỳ lương |
| `UserId` | Guid? FK → Users | Null nếu nhân sự không có tài khoản |
| `EmployeeName` | string(200) | Snapshot tên (bắt buộc) |
| `Position` | string?(100) | Chức danh / bộ phận |
| `Salary` | decimal(18,2) | Lương |
| `Allowance` | decimal(18,2) | Phụ cấp |
| `Insurance` | decimal(18,2) | Bảo hiểm |
| `OtherCost` | decimal(18,2) | Chi phí khác |
| `TotalAmount` | decimal(18,2) | Tổng, ghi khi save |
| `Notes` | string?(500) | |

Index unique: `(Year, Month, UserId)` khi `UserId != null`; `(Year, Month, EmployeeName)` cho dòng free-text.
Tiện ích: nút **"Sao chép từ tháng trước"** — copy toàn bộ bảng lương tháng N-1 sang tháng N (kế toán chỉ sửa chênh lệch).

### 2.4 `ExpenseCategory` — đầu mục chi phí cố định (kế toán tự thêm)

`Name` (string 200, unique), `Description`, `SortOrder` int, `IsActive` bool, `IsSystem` bool (đầu mục seed sẵn, không cho xóa — chỉ ẩn).

**Seed mặc định** (theo yêu cầu khách): Tiền thuê nhà · Tiền điện nước · Tiền mạng internet · Chi phí ăn uống · Chi phí gửi xe · Chi phí ship nội bộ · Chi phí khác.
Theo mẫu `ProductionDaysOption` / `Material` đã có sẵn trong repo (`LookupServices.cs`, `lookups-admin`).

### 2.5 `FixedExpense` — chi phí cố định theo ngày

`ExpenseDate` (DateTime, date-only về mặt logic), `ExpenseCategoryId` (FK), `CategoryName` (snapshot), `Amount` decimal(18,2), `Notes` string?(500), `AttachmentUrl` string? (ảnh hóa đơn), `CreatedByUserId` (FK → Users).
Index: `(ExpenseDate)`, `(ExpenseCategoryId, ExpenseDate)`.

---

## 3. Quyền — role `Accountant` (Kế toán)

Hiện repo **chưa có role Kế toán** (`CRM.Core/Entities/Role.cs` → `RoleNames`).

Cần thêm:
- `backend/CRM.Core/Entities/Role.cs`: `public const string Accountant = "Accountant";`
  - thêm vào `AllRoles`
  - thêm nhóm mới `public static readonly string[] FinanceRoles = { Admin, Accountant };`
- `backend/CRM.Infrastructure/Data/DataSeeder.cs`: seed role `Accountant` (mô tả "Kế toán") + tài khoản mẫu nếu cần
- `frontend/crm-app/src/app/core/services/auth.service.ts`: `RoleNames.Accountant` + `RoleGroups.FinanceRoles`
- Quản lý tài khoản: Admin tạo user Kế toán (không gán vào `DepartmentStaff` vì kế toán không thuộc phòng nào)

**Ma trận quyền:**

| Chức năng | Admin | Accountant | Khác |
|---|---|---|---|
| Xem/nhập chi phí đơn hàng | ✅ | ✅ | ❌ |
| Chi phí nhân sự | ✅ | ✅ | ❌ |
| Chi phí cố định | ✅ | ✅ | ❌ |
| Thêm/sửa đầu mục chi phí | ✅ | ✅ (theo yêu cầu "kế toán tự thêm") | ❌ |
| Báo cáo lãi/lỗ | ✅ | ✅ | ❌ |
| Bỏ khóa dòng đã chốt (`IsFinalized`) | ✅ | ❌ | ❌ |

⚠️ **Bảo mật:** giá cost là dữ liệu nhạy cảm. Tất cả DTO cost/lợi nhuận **chỉ** trả qua endpoint có `[Authorize(Roles = ...FinanceRoles)]`. **Không** nhồi field cost vào `OrderDto` hiện tại (Sale/SX đang đọc DTO đó).

---

## 4. Backend — danh sách file

### 4.1 Core
```
CRM.Core/Entities/OrderCost.cs
CRM.Core/Entities/OrderCostItem.cs        (Phase 6)
CRM.Core/Entities/PayrollEntry.cs
CRM.Core/Entities/ExpenseCategory.cs
CRM.Core/Entities/FixedExpense.cs
CRM.Core/Entities/Role.cs                 (sửa: + Accountant, FinanceRoles)
CRM.Core/Interfaces/Repositories/IOrderCostRepository.cs
CRM.Core/Interfaces/Repositories/IPayrollRepository.cs
CRM.Core/Interfaces/Repositories/IFixedExpenseRepository.cs
CRM.Core/Interfaces/Repositories/IExpenseCategoryRepository.cs
CRM.Core/Interfaces/IUnitOfWork.cs        (sửa: + 4 property)
```

### 4.2 Infrastructure
```
CRM.Infrastructure/Data/CrmDbContext.cs              (sửa: + 4 DbSet)
CRM.Infrastructure/Data/Configurations/OrderCostConfiguration.cs
CRM.Infrastructure/Data/Configurations/PayrollEntryConfiguration.cs
CRM.Infrastructure/Data/Configurations/ExpenseCategoryConfiguration.cs
CRM.Infrastructure/Data/Configurations/FixedExpenseConfiguration.cs
CRM.Infrastructure/Repositories/{OrderCost,Payroll,FixedExpense,ExpenseCategory}Repository.cs
CRM.Infrastructure/Repositories/UnitOfWork.cs        (sửa)
CRM.Infrastructure/Data/DataSeeder.cs                (sửa: role Accountant + 7 ExpenseCategory)
```
Nhớ `HasPrecision(18, 2)` cho mọi cột decimal (theo `OrderConfiguration.cs:26`).

### 4.3 Application
```
CRM.Application/DTOs/Finance/OrderCostDto.cs, UpdateOrderCostDto.cs, OrderCostListItemDto.cs
CRM.Application/DTOs/Finance/PayrollEntryDto.cs, UpsertPayrollEntryDto.cs
CRM.Application/DTOs/Finance/ExpenseCategoryDto.cs, Create/UpdateExpenseCategoryDto.cs
CRM.Application/DTOs/Finance/FixedExpenseDto.cs, Create/UpdateFixedExpenseDto.cs
CRM.Application/DTOs/Finance/OrderProfitDto.cs, MonthlyProfitDto.cs, ProfitFilterDto.cs
CRM.Application/DTOs/Finance/CostImportResultDto.cs
CRM.Application/Interfaces/IOrderCostService.cs, IPayrollService.cs,
                            IFixedExpenseService.cs, IExpenseCategoryService.cs, IProfitReportService.cs
CRM.Application/Services/OrderCostService.cs
CRM.Application/Services/PayrollService.cs
CRM.Application/Services/FixedExpenseService.cs
CRM.Application/Services/ExpenseCategoryService.cs
CRM.Application/Services/ProfitReportService.cs
CRM.Application/Validators/… (FluentValidation: số tiền >= 0, Month 1-12, Year hợp lệ)
CRM.Application/Mappings/…   (AutoMapper profile)
```

### 4.4 API
```
CRM.API/Controllers/OrderCostsController.cs      → /api/finance/order-costs
CRM.API/Controllers/PayrollController.cs         → /api/finance/payroll
CRM.API/Controllers/FixedExpensesController.cs   → /api/finance/fixed-expenses
CRM.API/Controllers/ExpenseCategoriesController.cs → /api/finance/expense-categories
CRM.API/Controllers/ProfitReportController.cs    → /api/finance/reports
CRM.API/Program.cs                               (sửa: đăng ký DI 5 service)
```

### 4.5 API contract

| Method | Route | Mô tả |
|---|---|---|
| GET | `/api/finance/order-costs` | Danh sách đơn + cost. Query: `dateFrom`, `dateTo`, `status`, `hasCost` (true/false — lọc đơn *chưa* nhập cost), `search`, `page`, `pageSize`. Mặc định: đơn có `Status >= InProduction` và `!= Cancelled`, tháng hiện tại |
| GET | `/api/finance/order-costs/{orderId}` | Chi tiết cost 1 đơn |
| PUT | `/api/finance/order-costs/{orderId}` | Nhập/sửa cost (upsert) |
| POST | `/api/finance/order-costs/bulk` | Lưu nhiều dòng cùng lúc (edit inline cả bảng rồi bấm Lưu) |
| POST | `/api/finance/order-costs/import` | Upload Excel/CSV → parse & áp cost hàng loạt. Trả `CostImportResultDto` (số dòng OK / lỗi / mã đơn không tìm thấy) |
| GET | `/api/finance/order-costs/import-template` | Tải file mẫu |
| POST | `/api/finance/order-costs/{orderId}/attachment` | Upload file giá cost đính kèm đơn (theo pattern `OrdersController.cs:439`) |
| GET/POST/PUT/DELETE | `/api/finance/payroll` | `?year=&month=`; POST `copy-from-previous` |
| GET/POST/PUT/DELETE | `/api/finance/fixed-expenses` | `?dateFrom=&dateTo=&categoryId=` |
| GET/POST/PUT/DELETE | `/api/finance/expense-categories` | CRUD đầu mục |
| GET | `/api/finance/reports/order-profit` | Lãi/lỗ theo từng đơn (có phân trang + tổng) |
| GET | `/api/finance/reports/monthly-profit` | Lãi/lỗ tổng theo tháng (`?year=` trả 12 tháng) |
| GET | `/api/finance/reports/monthly-profit/{year}/{month}/detail` | Bóc tách 1 tháng: doanh thu, COGS, nhân sự, cố định theo đầu mục |
| GET | `/api/finance/reports/export` | Xuất Excel báo cáo (Phase 6) |

---

## 5. Công thức báo cáo lãi/lỗ

### 5.1 Theo đơn hàng
```
DoanhThu(đơn)  = Order.TotalAmount            (đã trừ chiết khấu, gồm VAT)
ChiPhi(đơn)    = OrderCost.CostAmount
               + OrderCost.ShippingCost
               + OrderCost.OutboundShippingCost
               + OrderCost.OtherCost
LaiLo(đơn)     = DoanhThu − ChiPhi
BienLoiNhuan%  = LaiLo / DoanhThu × 100        (DoanhThu > 0)
```
Đơn `Cancelled` bị loại. Đơn chưa nhập cost → hiển thị badge **"Chưa nhập cost"**, tính lãi = doanh thu nhưng **không** cộng vào tổng "đã chốt" (báo cáo tách 2 số: *đã có cost* / *chưa có cost*) để tránh lãi ảo.

### 5.2 Theo tháng
```
DoanhThuThang   = Σ Order.TotalAmount        (đơn không hủy, mốc ngày = §5.3)
COGSThang       = Σ ChiPhi(đơn) của các đơn đó
ChiPhiNhanSu    = Σ PayrollEntry.TotalAmount  (Year=y, Month=m)
ChiPhiCoDinh    = Σ FixedExpense.Amount       (ExpenseDate trong tháng)
LaiGop          = DoanhThuThang − COGSThang
LaiRong         = LaiGop − ChiPhiNhanSu − ChiPhiCoDinh
```

### 5.3 Mốc ngày quy về tháng
`Order` có nhiều mốc: `OrderDate`, `ConfirmedDate`, `CompletionDate`, `ActualDeliveryDate`.
→ Dùng **`ConfirmedDate ?? OrderDate`** làm mặc định (thời điểm ghi nhận doanh thu), và cho phép đổi qua query param `revenueBasis=order|confirmed|completed|delivered` để kế toán tự chọn. Cần **chốt với khách** (§9).
Lưu ý múi giờ: DB lưu UTC — quy đổi về giờ VN (UTC+7) khi gom nhóm tháng, theo pattern `reports.component.ts:82`.

---

## 6. Frontend — feature `finance`

```
frontend/crm-app/src/app/features/finance/
├── finance.module.ts
├── finance-routing.module.ts
├── order-costs/            → /finance/order-costs      "Chi phí sản xuất hàng hóa"
├── payroll/                → /finance/payroll          "Chi phí nhân sự"
├── fixed-expenses/         → /finance/fixed-expenses   "Chi phí cố định"
├── expense-categories/     → /finance/expense-categories "Đầu mục chi phí"
└── profit-report/          → /finance/profit           "Báo cáo lãi/lỗ"
```
Sửa thêm:
- `app-routing.module.ts` — thêm route `finance` lazy-load
- `layout/sidebar/sidebar.component.ts` — 5 mục menu mới, `roles: RoleGroups.FinanceRoles`
- `core/services/finance.service.ts` — wrapper `ApiService`
- `core/services/auth.service.ts` — `RoleNames.Accountant`, `RoleGroups.FinanceRoles`

### 6.1 Màn "Chi phí sản xuất hàng hóa"
- Bảng: Mã đơn · Khách hàng · Ngày tạo đơn · Trạng thái · Doanh thu · **Giá cost** · **Chi phí ship hàng** · **Chi phí gửi hàng đi** · Khác · Tổng cost · Lãi/lỗ · Trạng thái nhập
- 4 cột chi phí **edit inline** (input số, format `vi-VN`), nút **Lưu tất cả** → gọi `bulk`
- Bộ lọc: khoảng ngày (mặc định tháng này), trạng thái đơn, checkbox *"Chỉ đơn chưa nhập cost"*, ô tìm mã đơn/khách
- Nút **Tải file giá cost** (import Excel/CSV) + **Tải file mẫu**; dialog kết quả import (bao nhiêu dòng OK, dòng nào lỗi)
- Nút đính kèm file cost cho từng đơn
- Link mã đơn → `/orders/:id` (mở tab mới)
- Mặc định 100 dòng/trang, lưu state theo pattern order-list hiện tại

### 6.2 Màn "Chi phí nhân sự"
- Chọn **Tháng/Năm** ở đầu trang
- Bảng: Nhân sự (autocomplete từ danh sách User, hoặc gõ tay) · Chức danh · Lương · Phụ cấp · Bảo hiểm · Chi phí khác · **Tổng**
- Thêm dòng / xóa dòng / edit inline; dòng tổng cuối bảng
- Nút **Sao chép từ tháng trước**

### 6.3 Màn "Chi phí cố định"
- Bộ lọc khoảng ngày (mặc định tháng này) + đầu mục
- Bảng: Ngày · Đầu mục (dropdown lấy từ `ExpenseCategory` active) · Số tiền · Ghi chú · Đính kèm
- Nút **+ Thêm chi phí** (form nhanh inline, mặc định ngày = hôm nay)
- Tổng theo đầu mục hiển thị bên cạnh (mini summary)
- Link nhanh sang trang **Đầu mục chi phí** để kế toán tự thêm mục mới

### 6.4 Màn "Đầu mục chi phí"
CRUD đơn giản theo mẫu `settings/production-days-admin` (Tên · Thứ tự · Hoạt động). Đầu mục `IsSystem` chỉ được ẩn, không xóa. Đầu mục đã phát sinh chi phí → chặn xóa, gợi ý ẩn.

### 6.5 Màn "Báo cáo lãi/lỗ" — 2 tab
**Tab "Theo đơn hàng"**: bảng đơn + doanh thu/chi phí/lãi/biên %, tô đỏ đơn lỗ; KPI đầu trang (tổng doanh thu · tổng cost · tổng lãi · biên TB · số đơn chưa nhập cost).

**Tab "Theo tháng"**: chọn năm → bảng 12 tháng + biểu đồ cột (tái dùng pattern CSS bar/donut sẵn có ở `reports.component.ts:136`, không thêm thư viện chart):

| Tháng | Doanh thu | Giá vốn | Lãi gộp | Nhân sự | Cố định | **Lãi ròng** | Biên % |
|---|---|---|---|---|---|---|---|

Click 1 tháng → panel bóc tách chi phí cố định theo từng đầu mục + danh sách đơn trong tháng.

---

## 7. Import file giá cost

- Thêm package **ClosedXML** (MIT) vào `CRM.Infrastructure` để đọc `.xlsx`; hỗ trợ luôn `.csv` bằng parser tay.
- Cột file mẫu: `Mã đơn hàng | Giá cost | Chi phí ship hàng | Chi phí gửi hàng đi | Chi phí khác | Ghi chú`
- Đối chiếu theo `Order.OrderNumber` (unique — `OrderConfiguration.cs:19`).
- Quy tắc: dòng không tìm thấy mã đơn → báo lỗi, **không** rollback cả file; ô trống → giữ giá trị cũ (không ghi đè bằng 0); đơn `IsFinalized` → bỏ qua và báo.
- Giới hạn file 5 MB, tối đa 5.000 dòng.

---

## 8. Migration & triển khai

Theo bài học đã ghi trong memory của dự án:

1. Tạo migration:
   `dotnet ef migrations add AddFinanceCostModule --project CRM.Infrastructure --startup-project CRM.API`
2. **Bắt buộc** `git add -f backend/CRM.Infrastructure/Migrations/*_AddFinanceCostModule.Designer.cs`
   (`.gitignore` đang loại trừ `*.Designer.cs` → thiếu file này thì `Migrate()` trên CI/prod **im lặng không apply**).
3. Kiểm tra `CrmDbContextModelSnapshot.cs` đã cập nhật và được commit.
4. Apply prod: truyền tên DB **tường minh** (`psql -d <db>`), không grep regex.
5. Seed chạy qua `DataSeeder` — viết idempotent (check tồn tại trước khi insert role + 7 đầu mục).

---

## 9. Điểm cần chốt với khách trước khi code

| # | Vấn đề | Đề xuất mặc định |
|---|---|---|
| 1 | "Tải file giá cost" nghĩa là **import số liệu** hay chỉ **đính kèm file**? | Làm **cả hai** (import Excel + đính kèm) — plan đã bao gồm |
| 2 | Giá cost nhập **theo cả đơn** hay **theo từng dòng sản phẩm**? | ✅ **ĐÃ CHỐT 2026-08-06**: giá cost là **đơn giá 1 sản phẩm**, tổng = SL × đơn giá — xem §13 |
| 3 | Doanh thu tính theo mốc ngày nào? | `ConfirmedDate ?? OrderDate`, có tùy chọn đổi |
| 4 | Doanh thu = `TotalAmount` (đã xuất hóa đơn) hay `PaidAmount` (thực thu)? | `TotalAmount`; hiển thị thêm cột đã thu để đối chiếu |
| 5 | Đơn hủy đã phát sinh cost thì tính sao? | Loại khỏi doanh thu, cost đưa vào mục "chi phí phát sinh khác" của tháng |
| 6 | Chi phí nhân sự/cố định có cần phân bổ về từng đơn không? | **Không** — chỉ trừ ở báo cáo tháng (khách chỉ yêu cầu "tổng theo tháng") |
| 7 | Kế toán có được xem toàn bộ đơn của mọi sale không? | Có (vai trò kế toán toàn công ty) |

---

## 10. Phân đoạn thực hiện

| Phase | Nội dung | Ước lượng |
|---|---|---|
| **1** | Role `Accountant` + seed + phân quyền FE/BE | 0.5 ngày |
| **2** | 4 entity + config + migration + repository + UnitOfWork + seed đầu mục | 1 ngày |
| **3** | Service + DTO + validator + controller cho OrderCost, Payroll, FixedExpense, ExpenseCategory | 1.5 ngày |
| **4** | `ProfitReportService` + 3 endpoint báo cáo | 1 ngày |
| **5** | Frontend: 5 màn hình + service + routing + sidebar | 2.5 ngày |
| **6** | Import/export Excel (ClosedXML) + file mẫu + dialog kết quả | 1 ngày |
| **7** | Test (unit cho công thức lãi/lỗ, integration cho import), rà soát rò rỉ dữ liệu cost | 1 ngày |
| | **Tổng** | **~8.5 ngày** |

Thứ tự ưu tiên nếu cần cắt gọt: Phase 1→2→3→5 (nhập liệu chạy được) → 4 (báo cáo) → 6 (import) → 7.

---

## 11. Test cần có

- **Unit** `ProfitReportService`: đơn có/không cost; đơn hủy; doanh thu = 0 (không chia cho 0); gom nhóm tháng đúng múi giờ VN.
- **Unit** `OrderCostService`: upsert không tạo trùng; `TotalCost` tính đúng; `IsFinalized` chặn sửa.
- **Integration**: import file có dòng lỗi → dòng hợp lệ vẫn lưu, dòng lỗi được báo.
- **Bảo mật**: gọi mọi endpoint `/api/finance/*` bằng token role `SalesRep` → **403**.

---

## 12. Ghi chú triển khai thực tế (2026-08-01)

### Khác biệt so với plan gốc

| Mục | Plan gốc | Thực tế |
|---|---|---|
| Vị trí service | `CRM.Application/Services` | `CRM.Infrastructure/Services/Finance/` — dùng thẳng `CrmDbContext` cho truy vấn gộp/nhóm, theo đúng mẫu `LookupServices.cs` / `DepositTransactionService` đã có |
| Repository + UnitOfWork | 4 repository mới | **Không cần** — service dùng DbContext trực tiếp, `IUnitOfWork` giữ nguyên |
| `OrderCostItem` (cost theo dòng SP) | Phase 6 tùy chọn | **Chưa làm** — chờ khách chốt câu hỏi §9.2 |
| Ngày chi phí cố định | `DateTime` | **`DateOnly`** → cột `date` trong Postgres, hết rủi ro lệch múi giờ khi gom tháng |
| Export Excel báo cáo | Phase 6 | **Chưa làm** — chỉ có import + file mẫu |
| Controller | 5 file riêng | Gộp trong `FinanceController.cs` với `FinanceControllerBase` mang sẵn `[Authorize(Roles = Admin,Accountant)]` |

### Đã kiểm chứng
- `dotnet build CRM.sln` — 0 error.
- `ng build --configuration production` — 0 error, chunk `features-finance-finance-module` 91 kB.
- **59/59 unit test pass** (26 test mới cho module tài chính, chạy trên SQLite in-memory qua chính `CrmDbContext`).
- Migration `20260801133413_AddFinanceCostModule` sinh DDL Postgres đúng: `numeric(18,2)`, `date`, unique index lọc `WHERE "UserId" IS NOT NULL`.
- File `.Designer.cs` đã `git add -f` (bị `.gitignore` loại — thiếu thì `Migrate()` im lặng không apply trên prod).

### Đã smoke-test trên DB dev thật (2026-08-02)
Migration đã apply vào `crm_dongphucbonmua`, 4 bảng tạo đủ. Kết quả gọi API thật:

| Kiểm tra | Kết quả |
|---|---|
| Gọi `/api/finance/*` khi chưa đăng nhập | 401 |
| Gọi 5 endpoint tài chính bằng token `SalesManager` | **403 cả 5** |
| Login `accountant@crm.com` | OK (seeder tạo role + tài khoản) |
| Seed đầu mục chi phí | đủ 7 mục, `isSystem=true` |
| `PUT order-costs/{id}` — DT 3.564.000, cost 1.255.000 | lãi 2.309.000, biên **64.79%** |
| Báo cáo tháng 04/2026 | DT 3.564.000 − vốn 1.255.000 = lãi ròng 2.309.000, cờ "1 đơn chưa nhập cost" |
| Báo cáo tháng 08/2026 (chỉ có nhân sự + cố định) | 0 − 12.000.000 − 2.000.000 = **−14.000.000** |

### Còn lại chưa phủ test
- `EF.Functions.ILike` (ô tìm kiếm) chỉ chạy trên Postgres — test SQLite không phủ nhánh này; smoke-test cũng chưa gõ từ khóa tìm kiếm.

### Bug có sẵn đã phát hiện & sửa kèm
`DataSeeder` hardcode `Id` cho role `WaybillStaff` = `15151515…` và `WarehouseManager` = `16161616…`,
nhưng **`RoleConfiguration.HasData` đã gán 2 GUID đó cho `MarketingManager` / `MediaMarketing`**
(migration `AddMarketingRoles`). Trên DB nào chưa có sẵn 2 role kia, seeder chết ngay lúc khởi động:
`23505 duplicate key value violates unique constraint "PK_Roles"` → API không boot được.
Đã bỏ hardcode `Id` ở cả 3 chỗ (kể cả `Accountant` mới thêm) — tra cứu vốn theo `Name` nên để EF tự sinh `Id`.

### Môi trường DB dev
DB dev nằm trong **container Docker `crm-postgres`** (`postgres:16-alpine`, volume `crm-postgres-data`),
không phải Postgres cài native. `postgres/postgres` @ `localhost:5432` — đúng như `appsettings.json`.
Container bind cứng cổng 5432, **đụng với container `giavang-db`** của project khác:
```bash
docker stop giavang-db && docker start crm-postgres   # chạy CRM
docker stop crm-postgres && docker start giavang-db   # quay lại project giavang
```

### Tài khoản mẫu do seeder tạo
`accountant@crm.com` / `Accountant@123` — role `Accountant`.

### Import file giá cost
Hỗ trợ **cả `.xlsx`** (qua ClosedXML 0.104.2, package mới thêm vào `CRM.Infrastructure`) **và `.csv`**
(tự đoán dấu phân cách `,` `;` `Tab`). Đọc được số kiểu VN `1.200.000` lẫn kiểu Mỹ `1,200,000`.
Ô để trống → **giữ nguyên giá trị cũ**, không ghi đè về 0.

---

## 13. Yêu cầu bổ sung từ kế toán (Trang Vũ — 2026-08-06) — CHƯA LÀM

Phản hồi sau khi kế toán dùng thật màn "Chi phí sản xuất hàng hóa" trên production.

### 13.1 Giá cost là ĐƠN GIÁ, không phải tổng
> *"em cần giá cost là giá của 1 sản phẩm, tổng giá cost sẽ là số lượng sản phẩm × giá cost"*

Hiện ô "Giá cost" đang nhập **tổng tiền cả đơn**. Cần đổi thành **đơn giá 1 sản phẩm**,
hệ thống tự nhân với số lượng để ra tổng.

**✅ ĐÃ CHỐT — công thức:** `Tổng giá cost = Đơn giá cost × TỔNG số lượng của đơn`

Kiểm chứng trên dữ liệu production (107 đơn, 2026-08-06):

| Kiểm tra | Kết quả |
|---|---|
| Số **loại sản phẩm** (Collection) mỗi đơn | **1 — cả 107/107 đơn** |
| Số **mức giá** (UnitPrice) mỗi đơn | **1 — cả 107/107 đơn** |
| Số **dòng** `OrderItems` mỗi đơn | 1 đến 15 (chỉ 17 đơn có đúng 1 dòng) |

Nhiều dòng **không phải nhiều sản phẩm** mà là **một sản phẩm tách theo size**.
Ví dụ `XA-000076`: 15 dòng đều là `POLO CỔ BẺ`, chất liệu `X-FIT`, giá `72.000` — chỉ khác
`NAM:S/M/L/XL/XXL/NC1/NC2` và `NU:S/M/L/...`.

→ Nên **KHÔNG cần** bảng `OrderCostItem` (§2.2). Chỉ cần thêm `UnitCost` vào `OrderCost`
và nhân với `SUM(OrderItems.Quantity)` của đơn.

⚠️ Khi code nhớ lấy **tổng số lượng của tất cả các dòng**, không phải `Quantity` của dòng đầu tiên —
84% đơn có nhiều hơn 1 dòng.

### 13.2 Thêm mục Quà tặng
> *"có thêm 1 phần quà tặng, giá quà tặng cũng nhân với sl như vậy"*

**✅ ĐÃ CHỐT (2026-08-06): số lượng quà tặng KHÁC số lượng áo** → phải nhập số lượng riêng,
không tái dùng `SUM(OrderItems.Quantity)` như §13.1.

`Tổng quà tặng = Đơn giá quà tặng × Số lượng quà tặng` (cả hai đều kế toán nhập tay).

Lý do quan trọng: tặng 1 lá cờ cho đơn 200 áo mà nhân với 200 thì tổng cost sai gấp 200 lần.

### 13.3 Trường Mã giao hàng
> *"cho em 1 trường em gắn lại mã giao hàng lên hệ thống"* — nhãn hiển thị: **"Mã giao hàng"**

`Order` đã có `GhtkLabel` / `ViettelPostLabel`, nhưng **chỉ được điền tự động** khi tạo vận đơn
qua API hãng vận chuyển. Đơn gửi ngoài hệ thống thì không có chỗ nhập.

**Đề xuất:** một cột "Mã giao hàng" trên màn chi phí, hiển thị `GhtkLabel ?? ViettelPostLabel`
khi đã tạo vận đơn qua API (chỉ đọc), và cho **nhập tay** khi cả hai đều trống — lưu vào field
riêng để lần sync tiếp theo của API không ghi đè mất giá trị kế toán nhập.

### 13.4 Cột số tiền thanh toán (đối soát) — TRƯỜNG ĐỘC LẬP
> *"em xin thêm 1 cột gắn số tiền thanh toán vào nữa, để xử lí phần đối soát"*
>
> *"nếu có lệnh chuyển khoản thì em gắn ck, còn nếu từ viettel thì em chỉ có lệnh tổng
> em phải nhập số tiền"*

**✅ ĐÃ CHỐT: trường độc lập, KHÔNG dùng lại `Order.PaidAmount`.**

Đã kiểm tra `Order.PaidAmount` — **đang được dùng thật và ở chỗ nhạy cảm**, không thể tái sử dụng:

| Nơi dùng | Chi tiết |
|---|---|
| `OrderService:761` | Gắn mã cọc → `PaidAmount += deposit.Amount` (gỡ mã thì trừ lại, dòng 727) |
| `OrderService:544` | Màn "Cập nhật thanh toán" ghi trực tiếp |
| `OrderService:393` | Đơn hoàn thành → tự set bằng `TotalAmount` |
| `OrderService:548,769` | Quyết định `PaymentStatus` (Pending/Partial/Paid) |
| **`GhtkShipmentService:89`** | **`PickMoney = TotalAmount − PaidAmount`** — số tiền GHTK thu hộ khách |
| **`ViettelPostShipmentService:45`** | **`MoneyCollection = TotalAmount − PaidAmount`** — tương tự |
| `order-list`, `order-detail` | Cột "Đã thanh toán" / "Còn nợ" |

Dữ liệu prod: **46/107 đơn có `PaidAmount > 0`**.

⚠️ Nếu kế toán ghi đè `PaidAmount` để đối soát thì **sẽ đổi số tiền hãng vận chuyển thu hộ khách**
ở các đơn COD tạo sau đó. Bắt buộc dùng field mới trên `OrderCost` (VD `SettlementAmount`),
không đụng `Order.PaidAmount`.

### 13.5 Đơn XA-000072 doanh thu 0 — đã xác minh
Không phải lỗi hiển thị: đơn có **7 dòng, tổng 10 sản phẩm, nhưng `TotalAmount = 0.00`**
(chưa điền đơn giá bán). Cần sale bổ sung giá, nếu không biên lợi nhuận của đơn này vô nghĩa
và kéo lệch báo cáo tháng.

**Đề xuất kèm theo:** cảnh báo trên màn chi phí với các đơn `TotalAmount = 0`, giống badge
"Chưa nhập cost" hiện có.

### 13.6 Tóm tắt thay đổi schema cần làm (gộp §13.1–13.4)

Thêm vào entity `OrderCost` — **cần EF migration**:

| Field mới | Kiểu | Ý nghĩa |
|---|---|---|
| `UnitCost` | decimal(18,2) | Đơn giá cost 1 sản phẩm (thay cách hiểu cũ của `CostAmount`) |
| `GiftUnitCost` | decimal(18,2) | Đơn giá 1 phần quà tặng |
| `GiftQuantity` | int | Số lượng quà tặng — nhập tay, KHÁC số lượng áo |
| `ShippingCode` | string?(100) | Mã giao hàng nhập tay (khi không tạo vận đơn qua API) |
| `SettlementAmount` | decimal(18,2) | Số tiền thanh toán để đối soát — **độc lập với `Order.PaidAmount`** |

Công thức `TotalCost` mới:
```
TotalCost = UnitCost      × SUM(OrderItems.Quantity)     ← tổng SL mọi dòng size
          + GiftUnitCost  × GiftQuantity                  ← số lượng riêng
          + ShippingCost
          + OutboundShippingCost
          + OtherCost
```

Xử lý dữ liệu cũ: `CostAmount` hiện đang lưu **tổng tiền**, không phải đơn giá. Khi migrate
cần quyết định — hoặc giữ `CostAmount` làm cột tổng (tính lại từ `UnitCost`), hoặc backfill
`UnitCost = CostAmount / SUM(Quantity)`. Số bản ghi hiện rất ít nên nhập lại tay cũng được.

Việc kèm theo: file mẫu import (.xlsx/.csv) và `OrderCostService.ImportAsync` phải đổi cột
"Giá cost" → "Đơn giá cost", thêm cột quà tặng, mã giao hàng, số tiền thanh toán.
