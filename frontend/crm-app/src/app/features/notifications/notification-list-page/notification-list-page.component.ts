import { Component, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { NotificationService } from '../../../core/services/notification.service';
import { Notification, NotificationType, NOTIFICATION_TYPE_LABELS } from '../../../core/models/notification.model';

@Component({
  selector: 'app-notification-list-page',
  templateUrl: './notification-list-page.component.html',
  styleUrls: ['./notification-list-page.component.scss']
})
export class NotificationListPageComponent implements OnInit {
  notifications: Notification[] = [];
  loading = false;
  unreadOnly = false;
  page = 1;
  pageSize = 20;
  totalCount = 0;
  totalPages = 0;

  constructor(
    private notificationService: NotificationService,
    private router: Router
  ) {}

  ngOnInit(): void {
    this.load();
  }

  load(): void {
    this.loading = true;
    this.notificationService.list({
      unreadOnly: this.unreadOnly,
      page: this.page,
      pageSize: this.pageSize
    }).subscribe({
      next: response => {
        this.notifications = response.items;
        this.totalCount = response.totalCount;
        this.totalPages = response.totalPages;
        this.loading = false;
      },
      error: () => (this.loading = false)
    });
  }

  toggleFilter(): void {
    this.unreadOnly = !this.unreadOnly;
    this.page = 1;
    this.load();
  }

  goToPage(p: number): void {
    if (p < 1 || p > this.totalPages || p === this.page) return;
    this.page = p;
    this.load();
  }

  onItemClick(n: Notification): void {
    if (!n.isRead) {
      this.notificationService.markRead(n.id).subscribe({
        next: () => {
          n.isRead = true;
          n.readAt = new Date().toISOString();
        },
        error: () => {}
      });
    }
    if (n.link) {
      this.router.navigateByUrl(n.link);
    }
  }

  onMarkAllRead(): void {
    this.notificationService.markAllRead().subscribe({
      next: () => {
        this.notifications.forEach(n => {
          if (!n.isRead) {
            n.isRead = true;
            n.readAt = new Date().toISOString();
          }
        });
      },
      error: () => {}
    });
  }

  onDelete(n: Notification, event: Event): void {
    event.stopPropagation();
    this.notificationService.delete(n.id).subscribe({
      next: () => {
        this.notifications = this.notifications.filter(x => x.id !== n.id);
        this.totalCount = Math.max(0, this.totalCount - 1);
      },
      error: () => {}
    });
  }

  typeLabel(type: NotificationType): string {
    return NOTIFICATION_TYPE_LABELS[type] ?? '';
  }

  formatDateTime(iso: string): string {
    return new Date(iso).toLocaleString('vi-VN');
  }

  pages(): number[] {
    const result: number[] = [];
    const start = Math.max(1, this.page - 2);
    const end = Math.min(this.totalPages, this.page + 2);
    for (let i = start; i <= end; i++) result.push(i);
    return result;
  }
}
