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
    }
  ];
}
