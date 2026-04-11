# Huong dan He thong Phan quyen theo Role

## Tong quan

He thong CRM su dung phan quyen dua tren Role (Role-Based Access Control - RBAC) de kiem soat quyen truy cap cua nguoi dung vao cac chuc nang khac nhau.

---

## Danh sach Roles

| Role | Mo ta | Pham vi |
|------|-------|---------|
| **Admin** | Quan tri vien | Toan quyen he thong |
| **SalesManager** | Quan ly kinh doanh | Quan ly sales team, bao cao, thanh toan |
| **SalesRep** | Nhan vien sales | Tao/sua don hang, khach hang |
| **ProductionManager** | Quan ly san xuat | Cap nhat trang thai san xuat |
| **QualityControl** | Kiem soat chat luong | Xac nhan/tu choi QC |
| **DeliveryManager** | Quan ly giao hang | Cap nhat trang thai giao hang |

---

## Tai khoan Test

| Role | Email | Mat khau |
|------|-------|----------|
| Admin | admin@dongphucbonmua.com | Admin@123 |
| SalesManager | manager@dongphucbonmua.com | Manager@123 |
| SalesRep | sales@dongphucbonmua.com | Sales@123 |
| ProductionManager | production@dongphucbonmua.com | Production@123 |
| QualityControl | qc@dongphucbonmua.com | QC@123456 |
| DeliveryManager | delivery@dongphucbonmua.com | Delivery@123 |

---

## Ma tran Phan quyen Don hang

### Chuyen doi Trang thai Don hang

```
Trang thai hien tai -> Trang thai moi : Roles duoc phep

Draft -> Confirmed       : Admin, SalesManager, SalesRep
Draft -> Cancelled       : Admin, SalesManager, SalesRep
Confirmed -> InProduction: Admin, SalesManager, ProductionManager
Confirmed -> Cancelled   : Admin, SalesManager
InProduction -> QualityCheck: Admin, ProductionManager
InProduction -> Cancelled   : Admin, SalesManager
QualityCheck -> ReadyToShip : Admin, QualityControl
QualityCheck -> InProduction: Admin, ProductionManager, QualityControl (lam lai)
ReadyToShip -> Shipping     : Admin, DeliveryManager
Shipping -> Delivered       : Admin, DeliveryManager
Delivered -> Completed      : Admin, SalesManager, SalesRep
```

### Cac thao tac khac

| Thao tac | Roles duoc phep |
|----------|-----------------|
| Xoa don hang | Admin, SalesManager |
| Cap nhat thanh toan | Admin, SalesManager |
| Xem tong hop don hang | Admin, SalesManager |
| Xoa khach hang | Admin, SalesManager |
| Xoa giao dich | Admin, SalesManager |
| Danh dau thang/thua giao dich | Admin, SalesManager, SalesRep |

---

## Dashboard theo Role

### Sales Dashboard (Admin, SalesManager, SalesRep)
- Tong khach hang
- Giao dich dang xu ly
- Doanh thu
- Cong viec
- Ty le chuyen doi

### Production Dashboard (Admin, ProductionManager)
- Don hang cho san xuat (Confirmed)
- Don hang dang san xuat (InProduction)
- Don hoan thanh hom nay
- Tong san pham trong quy trinh

### Quality Dashboard (Admin, QualityControl)
- Don hang cho kiem tra (QualityCheck)
- Don dat chuan hom nay
- Don can lam lai
- Ty le dat chuan

### Delivery Dashboard (Admin, DeliveryManager)
- Don san sang giao (ReadyToShip)
- Don dang giao (Shipping)
- Don da giao hom nay
- Tong gia tri dang giao

---

## API Endpoints

### Order Endpoints co phan quyen

```
GET    /api/orders                    - Tat ca roles (xem don hang)
GET    /api/orders/{id}               - Tat ca roles
POST   /api/orders                    - Tat ca roles (tao don hang)
PUT    /api/orders/{id}               - Tat ca roles (cap nhat thong tin)
PUT    /api/orders/{id}/status        - Tuy thuoc vao transition (xem ma tran)
PUT    /api/orders/{id}/payment       - Admin, SalesManager
DELETE /api/orders/{id}               - Admin, SalesManager
GET    /api/orders/{id}/allowed-statuses - Tat ca roles (lay danh sach trang thai hop le)
GET    /api/orders/summary            - Admin, SalesManager
```

### Dashboard Endpoints

```
GET /api/dashboard                    - Tat ca roles (sales dashboard)
GET /api/dashboard/my-stats           - Tat ca roles
GET /api/dashboard/production         - Admin, ProductionManager
GET /api/dashboard/quality            - Admin, QualityControl
GET /api/dashboard/delivery           - Admin, DeliveryManager
GET /api/dashboard/sales-performance  - Admin, SalesManager
```

---

## Cau truc Code

### Backend

#### Authorization Policies
```
backend/CRM.API/Authorization/
├── Policies.cs                      # Dinh nghia policy constants
└── OrderStatusTransitionValidator.cs # Validate role cho status transitions
```

#### Dang ky trong Program.cs
```csharp
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy(Policies.CanManageOrders, policy =>
        policy.RequireRole(RoleNames.AllRoles));

    options.AddPolicy(Policies.CanUpdatePayment, policy =>
        policy.RequireRole(RoleNames.Admin, RoleNames.SalesManager));

    options.AddPolicy(Policies.CanDeleteOrders, policy =>
        policy.RequireRole(RoleNames.Admin, RoleNames.SalesManager));
    // ...
});
```

#### Su dung trong Controller
```csharp
[HttpDelete("{id}")]
[Authorize(Policy = Policies.CanDeleteOrders)]
public async Task<ActionResult<ApiResponse>> Delete(Guid id)
{
    // ...
}
```

### Frontend

#### Role Constants
```typescript
// auth.service.ts
export const RoleNames = {
  Admin: 'Admin',
  SalesManager: 'SalesManager',
  SalesRep: 'SalesRep',
  ProductionManager: 'ProductionManager',
  QualityControl: 'QualityControl',
  DeliveryManager: 'DeliveryManager'
} as const;

export const RoleGroups = {
  AllRoles: Object.values(RoleNames),
  SalesRoles: [RoleNames.Admin, RoleNames.SalesManager, RoleNames.SalesRep],
  ManagerRoles: [RoleNames.Admin, RoleNames.SalesManager],
  ProductionRoles: [RoleNames.Admin, RoleNames.ProductionManager],
  QualityRoles: [RoleNames.Admin, RoleNames.QualityControl],
  DeliveryRoles: [RoleNames.Admin, RoleNames.DeliveryManager],
};
```

#### Kiem tra quyen trong Component
```typescript
// Trong component
canDeleteOrder(): boolean {
  return this.authService.canDeleteOrders();
}

canUpdatePayment(): boolean {
  return this.authService.canUpdatePayment();
}
```

#### Kiem tra quyen trong Template
```html
<button *ngIf="canDeleteOrder()" (click)="deleteOrder()">
  Xoa don hang
</button>
```

#### Filter menu theo role
```typescript
// sidebar.component.ts
private allMenuItems: MenuItem[] = [
  { label: 'Tong quan', icon: 'dashboard', route: '/dashboard' },
  { label: 'Khach hang', icon: 'users', route: '/customers', roles: RoleGroups.SalesRoles },
  { label: 'Bao cao', icon: 'chart', route: '/reports', roles: RoleGroups.ManagerRoles },
];

private filterMenuItems(): void {
  this.menuItems = this.allMenuItems.filter(item => {
    if (!item.roles || item.roles.length === 0) return true;
    return this.authService.hasAnyRole(item.roles);
  });
}
```

---

## Quy trinh Don hang theo Role

```
1. SalesRep tao don hang (Draft)
           |
           v
2. SalesRep/Manager xac nhan (Confirmed)
           |
           v
3. ProductionManager bat dau san xuat (InProduction)
           |
           v
4. ProductionManager hoan thanh san xuat -> QualityCheck
           |
           v
5. QualityControl kiem tra:
   - Dat chuan -> ReadyToShip
   - Khong dat -> InProduction (lam lai)
           |
           v
6. DeliveryManager bat dau giao (Shipping)
           |
           v
7. DeliveryManager xac nhan da giao (Delivered)
           |
           v
8. SalesRep/Manager hoan thanh don hang (Completed)
```

---

## Xu ly loi

### API tra ve 403 Forbidden
- Nguoi dung khong co quyen thuc hien thao tac
- Kiem tra role cua nguoi dung
- Kiem tra policy yeu cau

### Khong thay menu/nut bam
- Menu/nut bi filter theo role
- Kiem tra role cua nguoi dung trong AuthService

### Khong the chuyen trang thai don hang
- Kiem tra trang thai hien tai va trang thai muon chuyen
- Kiem tra role co trong danh sach cho phep khong
- Su dung API `/api/orders/{id}/allowed-statuses` de lay danh sach trang thai hop le

---

## Mo rong he thong

### Them Role moi

1. **Backend:**
   - Them constant trong `RoleNames` (Role.cs)
   - Them vao `DataSeeder.cs` de seed role
   - Tao user mau trong DataSeeder
   - Them policy neu can trong Program.cs

2. **Frontend:**
   - Them constant trong `RoleNames` (auth.service.ts)
   - Them vao `RoleGroups` neu can
   - Cap nhat sidebar menu items
   - Cap nhat permission check methods

### Them Policy moi

1. **Backend:**
   - Them constant trong `Policies.cs`
   - Dang ky trong `Program.cs`
   - Su dung `[Authorize(Policy = Policies.NewPolicy)]` trong controller

2. **Frontend:**
   - Them permission check method trong `AuthService`
   - Su dung trong component de show/hide UI elements

---

## Luu y quan trong

1. **Admin luon co toan quyen** - Khong can check rieng cho Admin trong code
2. **Frontend chi la UI** - Backend luon validate lai quyen truoc khi thuc hien
3. **JWT token chua roles** - Roles duoc lay tu token khi dang nhap
4. **Allowed statuses tu API** - Frontend goi API de lay danh sach trang thai hop le, khong tinh toan local
