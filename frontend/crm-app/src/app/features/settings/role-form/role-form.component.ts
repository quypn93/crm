import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { UserManagementService, RoleItem } from '../../../core/services/user-management.service';

interface PermissionItem {
  key: string;
  label: string;
  description: string;
  checked: boolean;
}

const ROLE_PERMISSIONS: Record<string, string[]> = {
  'Admin':             ['manage_users', 'manage_roles', 'view_reports', 'export_reports', 'manage_customers', 'manage_deals', 'manage_orders', 'manage_tasks', 'manage_designs', 'manage_production', 'manage_quality', 'manage_delivery'],
  'SalesManager':      ['view_reports', 'export_reports', 'manage_customers', 'manage_deals', 'manage_orders', 'manage_tasks', 'manage_designs'],
  'SalesRep':          ['manage_customers', 'manage_deals', 'manage_orders', 'manage_tasks', 'manage_designs'],
  'ProductionManager': ['view_orders', 'manage_production', 'manage_designs'],
  'QualityControl':    ['view_orders', 'manage_quality'],
  'DeliveryManager':   ['view_orders', 'manage_delivery'],
};

const ALL_PERMISSIONS: PermissionItem[] = [
  { key: 'manage_users',      label: 'Quản lý người dùng',    description: 'Tạo, sửa, xóa tài khoản',          checked: false },
  { key: 'manage_roles',      label: 'Quản lý vai trò',       description: 'Chỉnh sửa quyền hạn vai trò',      checked: false },
  { key: 'view_reports',      label: 'Xem báo cáo',           description: 'Truy cập trang báo cáo & thống kê', checked: false },
  { key: 'export_reports',    label: 'Xuất báo cáo',          description: 'Tải xuống file báo cáo',           checked: false },
  { key: 'manage_customers',  label: 'Quản lý khách hàng',    description: 'CRUD danh sách khách hàng',         checked: false },
  { key: 'manage_deals',      label: 'Quản lý giao dịch',     description: 'Tạo và theo dõi giao dịch',        checked: false },
  { key: 'manage_orders',     label: 'Quản lý đơn hàng',      description: 'Tạo, duyệt, xử lý đơn hàng',      checked: false },
  { key: 'view_orders',       label: 'Xem đơn hàng',          description: 'Chỉ xem danh sách đơn hàng',       checked: false },
  { key: 'manage_tasks',      label: 'Quản lý công việc',      description: 'Tạo và giao công việc',            checked: false },
  { key: 'manage_designs',    label: 'Quản lý thiết kế',      description: 'Upload và duyệt thiết kế',         checked: false },
  { key: 'manage_production', label: 'Quản lý sản xuất',      description: 'Cập nhật tiến độ sản xuất',        checked: false },
  { key: 'manage_quality',    label: 'Kiểm tra chất lượng',   description: 'Đánh dấu QC sản phẩm',             checked: false },
  { key: 'manage_delivery',   label: 'Quản lý giao hàng',     description: 'Cập nhật trạng thái giao hàng',    checked: false },
];

@Component({
  selector: 'app-role-form',
  templateUrl: './role-form.component.html',
  styleUrls: ['./role-form.component.scss']
})
export class RoleFormComponent implements OnInit {
  roleForm: FormGroup;
  role: RoleItem | null = null;
  isEditMode = false;
  isLoading = false;
  isSaving = false;
  errorMessage = '';
  roleId = '';
  permissions: PermissionItem[] = ALL_PERMISSIONS.map(p => ({ ...p }));
  selectedPermissions: Set<string> = new Set();

  constructor(
    private fb: FormBuilder,
    private userService: UserManagementService,
    private router: Router,
    private route: ActivatedRoute
  ) {
    this.roleForm = this.fb.group({
      name: ['', [Validators.required, Validators.pattern(/^[A-Za-z][A-Za-z0-9]*$/)]],
      description: ['']
    });
  }

  ngOnInit(): void {
    const id = this.route.snapshot.paramMap.get('id');
    if (id && id !== 'new') {
      this.isEditMode = true;
      this.roleId = id;
      this.roleForm.get('name')?.disable();
      this.loadRole();
    }
  }

  loadRole(): void {
    this.isLoading = true;
    this.userService.getRole(this.roleId).subscribe({
      next: (role) => {
        this.role = role;
        this.roleForm.patchValue({ description: role.description || '' });
        this.initPermissions(role.name);
        this.isLoading = false;
      },
      error: () => {
        this.errorMessage = 'Không thể tải thông tin vai trò';
        this.isLoading = false;
      }
    });
  }

  initPermissions(roleName: string): void {
    const active = new Set(ROLE_PERMISSIONS[roleName] || []);
    this.permissions = ALL_PERMISSIONS.map(p => ({ ...p, checked: active.has(p.key) }));
    this.selectedPermissions = new Set(active);
  }

  togglePermission(key: string): void {
    if (this.selectedPermissions.has(key)) {
      this.selectedPermissions.delete(key);
    } else {
      this.selectedPermissions.add(key);
    }
    this.permissions = this.permissions.map(p =>
      p.key === key ? { ...p, checked: !p.checked } : p
    );
  }

  getRoleLabel(roleName: string): string {
    return this.userService.getRoleLabel(roleName);
  }

  getRoleBadgeClass(roleName: string): string {
    return this.userService.getRoleBadgeClass(roleName);
  }

  isBuiltIn(): boolean {
    const builtIn = ['Admin', 'SalesManager', 'SalesRep', 'ProductionManager', 'QualityControl', 'DeliveryManager'];
    return this.role ? builtIn.includes(this.role.name) : false;
  }

  onSubmit(): void {
    if (this.roleForm.invalid) {
      this.roleForm.markAllAsTouched();
      return;
    }

    this.isSaving = true;
    this.errorMessage = '';

    if (this.isEditMode) {
      this.userService.updateRole(this.roleId, {
        description: this.roleForm.value.description || undefined
      }).subscribe({
        next: () => this.router.navigate(['/settings/roles']),
        error: (err) => {
          this.isSaving = false;
          this.errorMessage = err.error?.message || 'Cập nhật thất bại. Vui lòng thử lại.';
        }
      });
    } else {
      this.userService.createRole({
        name: this.roleForm.value.name.trim(),
        description: this.roleForm.value.description || undefined
      }).subscribe({
        next: () => this.router.navigate(['/settings/roles']),
        error: (err) => {
          this.isSaving = false;
          this.errorMessage = err.error?.message || 'Tạo vai trò thất bại. Vui lòng thử lại.';
        }
      });
    }
  }

  cancel(): void {
    this.router.navigate(['/settings/roles']);
  }

  get f() { return this.roleForm.controls; }
}
