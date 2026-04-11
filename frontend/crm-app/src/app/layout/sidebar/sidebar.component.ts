import { Component, Input, OnInit } from '@angular/core';
import { AuthService, RoleNames, RoleGroups } from '../../core/services/auth.service';

interface MenuItem {
  label: string;
  icon: string;
  route: string;
  badge?: number;
  roles?: string[];
  exact?: boolean;
}

@Component({
  selector: 'app-sidebar',
  templateUrl: './sidebar.component.html',
  styleUrls: ['./sidebar.component.scss']
})
export class SidebarComponent implements OnInit {
  @Input() isCollapsed = false;

  private allMenuItems: MenuItem[] = [
    { label: 'Tổng quan', icon: 'dashboard', route: '/dashboard' },
    { label: 'Khách hàng', icon: 'users', route: '/customers', roles: RoleGroups.SalesRoles },
    { label: 'Giao dịch', icon: 'briefcase', route: '/deals', roles: RoleGroups.SalesRoles },
    { label: 'Đơn hàng', icon: 'orders', route: '/orders' },
    { label: 'Sản xuất', icon: 'factory', route: '/production', roles: RoleGroups.ProductionRoles },
    { label: 'Kiểm tra CL', icon: 'check-circle', route: '/dashboard/quality', roles: RoleGroups.QualityRoles },
    { label: 'Giao hàng', icon: 'truck', route: '/dashboard/delivery', roles: RoleGroups.DeliveryRoles },
    { label: 'Báo cáo', icon: 'chart', route: '/reports', roles: RoleGroups.ManagerRoles },
    { label: 'Người dùng', icon: 'user-manage', route: '/settings/users', roles: [RoleNames.Admin], exact: false },
    { label: 'Vai trò', icon: 'shield', route: '/settings/roles', roles: [RoleNames.Admin], exact: true },
    { label: 'Cài đặt', icon: 'settings', route: '/settings', roles: [RoleNames.Admin], exact: true }
  ];

  menuItems: MenuItem[] = [];

  constructor(private authService: AuthService) {}

  ngOnInit(): void {
    this.filterMenuItems();

    // Subscribe to user changes to update menu
    this.authService.currentUser$.subscribe(() => {
      this.filterMenuItems();
    });
  }

  private filterMenuItems(): void {
    this.menuItems = this.allMenuItems.filter(item => {
      // If no roles specified, visible to all
      if (!item.roles || item.roles.length === 0) {
        return true;
      }
      // Check if user has any of the required roles
      return this.authService.hasAnyRole(item.roles);
    });
  }
}
