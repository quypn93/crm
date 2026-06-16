import { Component } from '@angular/core';

@Component({
  selector: 'app-settings-layout',
  templateUrl: './settings-layout.component.html',
  styleUrls: ['./settings-layout.component.scss']
})
export class SettingsLayoutComponent {
  navItems = [
    {
      label: 'Người dùng',
      route: '/settings/users',
      icon: 'users',
      description: 'Quản lý tài khoản'
    },
    {
      label: 'Vai trò',
      route: '/settings/roles',
      icon: 'shield',
      description: 'Phân quyền hệ thống'
    },
    {
      label: 'Dạng đơn',
      route: '/settings/order-types',
      icon: 'list',
      description: 'Cắt may, áo sẵn'
    },
    {
      label: 'Bộ sưu tập',
      route: '/settings/collections',
      icon: 'list',
      description: 'Quản lý mẫu sản phẩm'
    },
    {
      label: 'Chất liệu',
      route: '/settings/materials',
      icon: 'list',
      description: 'Quản lý chất liệu'
    },
    {
      label: 'Form áo',
      route: '/settings/product-forms',
      icon: 'list',
      description: 'Quản lý form sản phẩm'
    },
    {
      label: 'Quy cách',
      route: '/settings/product-specifications',
      icon: 'list',
      description: 'Quản lý quy cách'
    },
    {
      label: 'Thời gian SX',
      route: '/settings/production-days',
      icon: 'list',
      description: 'Quản lý ngày sản xuất'
    }
  ];
}
