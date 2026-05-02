import { Component, OnInit } from '@angular/core';
import { NotificationService } from '../../../core/services/notification.service';
import { ToastService } from '../../../core/services/toast.service';
import { RolePreference, NotificationType, NOTIFICATION_TYPE_LABELS } from '../../../core/models/notification.model';

interface RoleRow {
  roleId: string;
  roleName: string;
  cells: Record<NotificationType, { inApp: boolean; email: boolean; isDefault: boolean; dirty: boolean }>;
}

const VISIBLE_ROLES = [
  'Admin',
  'SalesManager',
  'SalesRep',
  'DesignManager',
  'Designer',
  'ContentManager',
  'ContentStaff'
];

const ROLE_LABELS: Record<string, string> = {
  Admin: 'Quản trị viên',
  SalesManager: 'Trưởng phòng kinh doanh',
  SalesRep: 'Nhân viên kinh doanh',
  DesignManager: 'Trưởng phòng thiết kế',
  Designer: 'Nhân viên thiết kế',
  ContentManager: 'Trưởng phòng content',
  ContentStaff: 'Nhân viên content'
};

@Component({
  selector: 'app-role-preferences-page',
  templateUrl: './role-preferences-page.component.html',
  styleUrls: ['./role-preferences-page.component.scss']
})
export class RolePreferencesPageComponent implements OnInit {
  rows: RoleRow[] = [];
  types: NotificationType[] = [
    NotificationType.TaskAssigned,
    NotificationType.TaskDueSoon,
    NotificationType.TaskOverdue,
    NotificationType.TaskCompleted,
    NotificationType.TaskReassigned
  ];
  loading = false;
  saving = false;

  constructor(
    private notificationService: NotificationService,
    private toast: ToastService
  ) {}

  ngOnInit(): void {
    this.load();
  }

  load(): void {
    this.loading = true;
    this.notificationService.getAllPreferences().subscribe({
      next: prefs => {
        this.rows = this.buildRows(prefs);
        this.loading = false;
      },
      error: () => (this.loading = false)
    });
  }

  private buildRows(prefs: RolePreference[]): RoleRow[] {
    const byRole = new Map<string, RolePreference[]>();
    for (const p of prefs) {
      const list = byRole.get(p.roleName) ?? [];
      list.push(p);
      byRole.set(p.roleName, list);
    }

    const rows: RoleRow[] = [];
    for (const roleName of VISIBLE_ROLES) {
      const list = byRole.get(roleName);
      if (!list || list.length === 0) continue;

      const row: RoleRow = {
        roleId: list[0].roleId,
        roleName,
        cells: {} as RoleRow['cells']
      };
      for (const type of this.types) {
        const p = list.find(x => x.type === type);
        if (p) {
          row.cells[type] = { inApp: p.inApp, email: p.email, isDefault: p.isDefault, dirty: false };
        } else {
          row.cells[type] = { inApp: false, email: false, isDefault: true, dirty: false };
        }
      }
      rows.push(row);
    }
    return rows;
  }

  roleLabel(name: string): string {
    return ROLE_LABELS[name] ?? name;
  }

  typeLabel(type: NotificationType): string {
    return NOTIFICATION_TYPE_LABELS[type];
  }

  onCellToggle(row: RoleRow, type: NotificationType, channel: 'inApp' | 'email'): void {
    const cell = row.cells[type];
    cell[channel] = !cell[channel];
    cell.dirty = true;
    cell.isDefault = false;
  }

  hasDirty(): boolean {
    return this.rows.some(r => Object.values(r.cells).some(c => c.dirty));
  }

  save(): void {
    const items = [];
    for (const row of this.rows) {
      for (const type of this.types) {
        const cell = row.cells[type];
        if (cell.dirty || !cell.isDefault) {
          items.push({
            roleId: row.roleId,
            type,
            inApp: cell.inApp,
            email: cell.email
          });
        }
      }
    }

    this.saving = true;
    this.notificationService.updatePreferences(items).subscribe({
      next: () => {
        this.saving = false;
        this.toast.success('Đã lưu cấu hình thông báo.');
        this.load();
      },
      error: () => {
        this.saving = false;
        this.toast.error('Lưu cấu hình thất bại.');
      }
    });
  }

  reset(): void {
    if (!confirm('Khôi phục về cấu hình mặc định? Tất cả tuỳ chỉnh hiện tại sẽ bị xoá.')) {
      return;
    }
    this.saving = true;
    this.notificationService.resetPreferences().subscribe({
      next: () => {
        this.saving = false;
        this.toast.success('Đã khôi phục mặc định.');
        this.load();
      },
      error: () => {
        this.saving = false;
        this.toast.error('Khôi phục thất bại.');
      }
    });
  }
}
