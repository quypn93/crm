import { Component, Input, OnInit } from '@angular/core';
import { Router } from '@angular/router';
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
    { label: 'Đơn hàng', icon: 'orders', route: '/orders', roles: RoleGroups.OrderRoles },
    { label: 'Công việc', icon: 'tasks', route: '/tasks', exact: false },
    { label: 'Giao thiết kế', icon: 'design', route: '/designs/assign', roles: RoleGroups.SalesRoles, exact: true },
    { label: 'Thiết kế của tôi', icon: 'design', route: '/designs/my-tasks', roles: [RoleNames.Admin, RoleNames.DesignManager, RoleNames.Designer], exact: true },
    // Thư viện — exact:false để active khi ở mọi sub-route /designs/* (bao gồm detail /designs/:id).
    // Không mở cho Designer thuần (họ chỉ thấy "Thiết kế của tôi"). Admin/DesignManager + Sale được xem.
    { label: 'Thư viện thiết kế', icon: 'design', route: '/designs', roles: [...RoleGroups.SalesRoles, RoleNames.Admin, RoleNames.DesignManager], exact: false },
    { label: 'Sản xuất', icon: 'factory', route: '/production', roles: RoleGroups.ProductionRoles },
    { label: 'Báo cáo', icon: 'chart', route: '/reports', roles: RoleGroups.ManagerRoles },
    { label: 'Form áo', icon: 'list', route: '/catalog/forms', roles: RoleGroups.ManagerRoles },
    { label: 'Quy cách', icon: 'list', route: '/catalog/style-specs', roles: RoleGroups.ManagerRoles },
    { label: 'Bộ sưu tập', icon: 'fabric', route: '/settings/collections', roles: [RoleNames.Admin], exact: true },
    { label: 'Chất liệu', icon: 'fabric', route: '/settings/materials', roles: [RoleNames.Admin], exact: true },
    { label: 'Form áo', icon: 'list', route: '/settings/product-forms', roles: [RoleNames.Admin], exact: true },
    { label: 'Quy cách', icon: 'list', route: '/settings/product-specifications', roles: [RoleNames.Admin], exact: true },
    { label: 'Thời gian SX', icon: 'check-circle', route: '/settings/production-days', roles: [RoleNames.Admin], exact: true },
    { label: 'Lịch sử cộng tiền', icon: 'chart', route: '/settings/deposits', roles: RoleGroups.SalesRoles, exact: true },
    { label: 'Người dùng', icon: 'user-manage', route: '/settings/users', roles: [RoleNames.Admin], exact: false },
    { label: 'Vai trò', icon: 'shield', route: '/settings/roles', roles: [RoleNames.Admin], exact: true },
    { label: 'Cấu hình thông báo', icon: 'bell', route: '/notifications/admin/preferences', roles: [RoleNames.Admin], exact: true },
    { label: 'Cài đặt', icon: 'settings', route: '/settings', roles: [RoleNames.Admin], exact: true }
  ];

  menuItems: MenuItem[] = [];

  constructor(private authService: AuthService, private router: Router) {}

  /**
   * Custom active-matching để tránh double-highlight khi có cả menu parent prefix
   * (VD /designs với exact:false) và sub-menu exact (VD /designs/assign).
   * Parent chỉ active nếu URL không khớp exact với bất kỳ sibling nào.
   */
  isMenuActive(item: MenuItem): boolean {
    const url = this.router.url.split('?')[0].split('#')[0];
    if (item.exact) {
      return url === item.route;
    }
    if (!url.startsWith(item.route)) return false;
    if (url === item.route) return true;
    const hasExactSibling = this.allMenuItems.some(
      o => o !== item && o.exact && o.route === url
    );
    return !hasExactSibling;
  }

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
