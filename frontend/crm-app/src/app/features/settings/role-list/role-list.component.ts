import { Component, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { UserManagementService, RoleItem } from '../../../core/services/user-management.service';

const BUILT_IN_ROLES = ['Admin', 'SalesManager', 'SalesRep', 'ProductionManager', 'QualityControl', 'DeliveryManager'];

@Component({
  selector: 'app-role-list',
  templateUrl: './role-list.component.html',
  styleUrls: ['./role-list.component.scss']
})
export class RoleListComponent implements OnInit {
  roles: RoleItem[] = [];
  isLoading = false;

  readonly roleDescriptions: Record<string, string> = {
    'Admin': 'Toàn quyền quản trị hệ thống, quản lý người dùng và cấu hình.',
    'SalesManager': 'Quản lý đội ngũ kinh doanh, xem báo cáo, duyệt đơn hàng.',
    'SalesRep': 'Quản lý khách hàng, tạo giao dịch và đơn hàng.',
    'ProductionManager': 'Quản lý quy trình sản xuất và tiến độ đơn hàng.',
    'QualityControl': 'Kiểm tra chất lượng sản phẩm trước khi giao hàng.',
    'DeliveryManager': 'Quản lý việc giao hàng và theo dõi trạng thái vận chuyển.'
  };

  constructor(private userService: UserManagementService, private router: Router) {}

  ngOnInit(): void {
    this.loadRoles();
  }

  loadRoles(): void {
    this.isLoading = true;
    this.userService.getRoles().subscribe({
      next: (roles) => { this.roles = roles; this.isLoading = false; },
      error: () => { this.isLoading = false; }
    });
  }

  addRole(): void {
    this.router.navigate(['/settings/roles/new']);
  }

  editRole(id: string): void {
    this.router.navigate(['/settings/roles', id, 'edit']);
  }

  deleteRole(role: RoleItem): void {
    if (confirm(`Bạn có chắc muốn xóa vai trò "${this.getRoleLabel(role.name)}"? Hành động này không thể hoàn tác.`)) {
      this.userService.deleteRole(role.id).subscribe({
        next: () => this.loadRoles(),
        error: (err) => alert(err.error?.message || 'Xóa vai trò thất bại.')
      });
    }
  }

  isBuiltIn(roleName: string): boolean {
    return BUILT_IN_ROLES.includes(roleName);
  }

  getRoleLabel(roleName: string): string {
    return this.userService.getRoleLabel(roleName);
  }

  getRoleBadgeClass(roleName: string): string {
    return this.userService.getRoleBadgeClass(roleName);
  }

  getDescription(role: RoleItem): string {
    return role.description || this.roleDescriptions[role.name] || 'Không có mô tả.';
  }
}
