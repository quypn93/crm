# Kế hoạch: Production Order Flow

**Ngày lập:** 2026-04-05  
**Stack:** ASP.NET Core 8 (Clean Architecture) + Angular 17 + SQL Server

---

## Tổng quan flow

```
Sale tạo đơn
    → Chọn designer + điền template (chất liệu, màu, size, tên riêng, quà tặng)
    → Confirm đơn
         ↓
Designer upload design (mặt trước / mặt sau)
    → Chuyển trạng thái → InProduction
    → QR code tự động sinh
         ↓
Bộ phận sản xuất (6 khâu)
    Worker quét QR → login nếu chưa → xem đơn → ấn "Hoàn thành"
    Sau mỗi khâu → ghi log (ai, lúc nào)
    Hoàn thành khâu cuối → tự động chuyển → QualityCheck
         ↓
QC → Shipping → Done
```

---

## Phase 1 — Database Entities mới

### Entity: `ProductionStage` (cấu hình master)
```
Id, StageOrder (int), StageName, Description, ResponsibleRole, IsActive
```

### Entity: `OrderProductionStep` (log từng đơn)
```
Id, OrderId(FK), ProductionStageId(FK), IsCompleted, CompletedByUserId, CompletedAt, Notes
Unique constraint: (OrderId, ProductionStageId)
```

### Thêm vào `Design`:
```
FrontImageUrl, BackImageUrl
SizeQuantities (JSON: {"S":0,"M":5,"L":10,"XL":8,"XXL":3,"NC1":1,"NC2":5,"NC3":0})
PersonNamesBySize (JSON: {"L":["Nguyễn A"],"M":["Trần B"]})
MaterialText, ColorText, StyleText
ReturnDate (NGÀY TRẢ)
GiftItems (JSON: [{"imageUrl":"...","description":"Cờ: 1M x 1,5M"}])
```

### Thêm vào `Order`:
```
QrCodeToken (string, 22 ký tự URL-safe Base64)
QrCodeImageBase64 (string, PNG)
DesignerUserId (Guid FK → Users)
```

### 6 Stage mặc định (seed):
| # | Tên | Role |
|---|-----|------|
| 1 | Cắt vải | ProductionManager |
| 2 | May | ProductionManager |
| 3 | In / Thêu logo | ProductionManager |
| 4 | Hoàn thiện (vệ sinh, cắt chỉ) | ProductionManager |
| 5 | Kiểm tra chất lượng | QualityControl |
| 6 | Đóng gói | ProductionManager |

---

## Phase 2 — Repository & UnitOfWork

**Files mới:**
- `CRM.Core/Interfaces/Repositories/IProductionStageRepository.cs`
- `CRM.Core/Interfaces/Repositories/IOrderProductionStepRepository.cs`
- `CRM.Infrastructure/Repositories/ProductionStageRepository.cs`
- `CRM.Infrastructure/Repositories/OrderProductionStepRepository.cs`
- Update `IUnitOfWork` + `UnitOfWork` với 2 property mới

---

## Phase 3 — Application Layer

### DTOs mới:
- `DTOs/Production/ProductionStageDtos.cs`
- `DTOs/Production/OrderProductionStepDtos.cs`  
  (gồm `OrderProductionProgressDto` — tổng hợp cho 1 đơn)

### Interfaces mới:
- `IProductionStageService` — CRUD + reorder stages
- `IOrderProductionService` — GetProgress, CompleteStep, InitializeSteps
- `IQrCodeService` — GenerateToken + GenerateQrCodeAsync (NuGet: QRCoder)
- `IOrderTemplateService` — GenerateHtmlTemplateAsync

### Services mới:
- `QrCodeService` — dùng QRCoder, encode URL: `{frontend}/scan/{token}`
- `ProductionStageService` — CRUD
- `OrderProductionService`:
  - `InitializeStepsAsync`: tạo OrderProductionStep cho tất cả stage
  - `CompleteStepAsync`: set IsCompleted, nếu xong hết → tự chuyển status → QualityCheck
- `OrderTemplateService` — render HTML phiếu sản xuất

### Sửa `OrderService`:
- Khi `Confirmed → InProduction`:
  1. Sinh `QrCodeToken`
  2. Sinh QR image (base64)
  3. Gọi `InitializeStepsAsync(orderId)`

---

## Phase 4 — API Controllers

### Mới:
```
GET/POST/PUT/DELETE  /api/production-stages          → quản lý khâu sản xuất
GET                  /api/orders/{id}/production      → tiến độ sản xuất đơn
POST                 /api/orders/{id}/production/steps/{stageId}/complete
GET                  /api/production/scan/{token}     → đọc đơn qua QR
POST                 /api/production/scan/{token}/steps/{stageId}/complete
GET                  /api/orders/{id}/template/html   → HTML phiếu in
```

### Sửa `OrdersController`:
- `GET /api/orders/{id}/qr` → trả về `{ qrCodeImageBase64, qrCodeToken }`

### Policies mới:
```csharp
CanManageProductionStages → Admin, ProductionManager
CanCompleteProductionStep → Admin, ProductionManager, QualityControl
```

---

## Phase 5 — Frontend

### Cấu trúc thư mục:

```
features/
├── production/
│   ├── production-dashboard/          ← danh sách đơn đang sản xuất + progress bar
│   ├── production-stage-list/         ← quản lý khâu (Admin/ProductionManager)
│   ├── production-stage-form/         ← tạo/sửa khâu
│   ├── production-order-progress/     ← chi tiết tiến độ 1 đơn (desktop)
│   ├── production.module.ts
│   └── production-routing.module.ts
├── scan/                              ← NGOÀI MainLayoutComponent (mobile)
│   ├── qr-scan-landing/               ← trang chính sau quét QR
│   ├── scan.module.ts
│   └── scan-routing.module.ts
└── orders/
    └── order-template/                ← phiếu sản xuất (in ấn)
```

### Service mới:
- `core/services/production.service.ts`

### Routes cần thêm vào `app-routing.module.ts`:
```typescript
// NGOÀI MainLayoutComponent — mobile QR scan
{ path: 'scan/:token', canActivate: [AuthGuard], loadChildren: () => ScanModule }

// TRONG MainLayoutComponent children
{ path: 'production', loadChildren: () => ProductionModule }
{ path: 'orders/:id/template', component: OrderTemplateComponent }  // hoặc trong OrdersModule
```

---

## Phase 6 — Trang Quét QR (Mobile-first, QUAN TRỌNG)

**Route:** `/scan/:token`  
**Không có sidebar/header** — nằm ngoài `MainLayoutComponent`

**Flow khi worker quét QR:**
1. Browser mở `localhost:4200/scan/AAABBB...`
2. `AuthGuard` kiểm tra → chưa login → redirect `/auth/login?returnUrl=/scan/AAABBB...`
3. Login xong → quay lại `/scan/AAABBB...`
4. Component gọi API `GET /api/production/scan/{token}`
5. Hiển thị:
   - Tên đơn, khách hàng, ngày giao
   - Danh sách khâu (✓ hoàn thành / ⏳ chờ)
   - Khâu đang chờ có nút **"Xác nhận hoàn thành"** (chỉ hiện nếu role phù hợp)
6. Ấn xác nhận → gọi `POST .../complete` → reload tiến độ

---

## Phase 7 — Phiếu Sản Xuất (Order Template)

Backend render HTML có:
- Barcode text (OrderNumber)
- KH / DESIGN / SALE
- Ngày lên đơn
- Ảnh mặt trước / sau
- Bảng tên riêng theo size
- Chất liệu, màu sắc, kiểu dáng
- Bảng size số lượng (S, M, L, XL, XXL, NC1, NC2, NC3, TỔNG)
- Ngày xong, ngày trả, ghi chú
- Quà tặng (ảnh + mô tả)
- QR code (góc phải)

Frontend dùng `[innerHTML]` + `SafeHtmlPipe`, nút "In phiếu" gọi `window.print()`.

---

## Phase 8 — Design Form (thêm field mới)

Sửa `design-form.component.ts` thêm:
- Upload ảnh mặt trước / mặt sau
- Bảng nhập số lượng theo size (S,M,L,XL,XXL,NC1,NC2,NC3)
- Nhập tên riêng theo size (textarea hoặc tag per size)
- Chất liệu, màu sắc, kiểu dáng
- Ngày trả
- Thêm/bớt quà tặng (dynamic list)

---

## Thứ tự xây dựng (tránh lỗi compile)

```
1. Core entities → 2. EF Config → 3. Migration → 4. Seed stages
5. Repository interfaces → 6. Repository impl → 7. UnitOfWork
8. DTOs → 9. Service interfaces → 10. QrCodeService (độc lập trước)
11. ProductionStageService → 12. OrderProductionService
13. OrderTemplateService → 14. Sửa OrderService
15. AutoMapper → 16. Controllers → 17. Program.cs
18. Frontend models → 19. Frontend service
20. Scan module (ưu tiên cao nhất cho mobile)
21. Production module → 22. OrderTemplate component
23. Design form fields → 24. Order detail QR panel
25. Routing + Sidebar
```

---

## Các quyết định kỹ thuật chính

| Vấn đề | Quyết định |
|--------|-----------|
| QR token | Base64Url(Guid bytes) = 22 ký tự, không expire, lưu trong DB |
| QR library | `QRCoder` NuGet (pure .NET, không native dep) |
| Mobile scan auth | Dùng `AuthGuard` hiện có với `returnUrl` query param |
| Template print | Backend render HTML → frontend `[innerHTML]` → `window.print()` |
| Upload ảnh design | `wwwroot/uploads/designs/{id}/` via IFormFile |
| Auto-advance status | `OrderProductionService.CompleteStepAsync` tự gọi UpdateStatus khi xong hết |
| Concurrency | Unique constraint `(OrderId, ProductionStageId)` + check `IsCompleted` trước update |
