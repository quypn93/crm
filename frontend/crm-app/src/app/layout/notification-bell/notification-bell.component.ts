import { Component, ElementRef, HostListener, OnDestroy, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { Subject, takeUntil } from 'rxjs';
import { NotificationService } from '../../core/services/notification.service';
import { Notification, NotificationType } from '../../core/models/notification.model';

@Component({
  selector: 'app-notification-bell',
  templateUrl: './notification-bell.component.html',
  styleUrls: ['./notification-bell.component.scss']
})
export class NotificationBellComponent implements OnInit, OnDestroy {
  isOpen = false;
  unreadCount = 0;
  notifications: Notification[] = [];
  loading = false;

  private destroy$ = new Subject<void>();

  constructor(
    private notificationService: NotificationService,
    private router: Router,
    private elementRef: ElementRef
  ) {}

  ngOnInit(): void {
    this.notificationService.unreadCount$
      .pipe(takeUntil(this.destroy$))
      .subscribe(count => (this.unreadCount = count));

    this.notificationService.latest$
      .pipe(takeUntil(this.destroy$))
      .subscribe(items => (this.notifications = items));
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  toggleDropdown(event: Event): void {
    event.stopPropagation();
    this.isOpen = !this.isOpen;
    if (this.isOpen) {
      this.loading = true;
      this.notificationService.loadLatest(10).subscribe({
        next: () => (this.loading = false),
        error: () => (this.loading = false)
      });
      // Refresh count khi mở dropdown để đồng bộ với state thật
      this.notificationService.getUnreadCount().subscribe({ error: () => {} });
    }
  }

  @HostListener('document:click', ['$event'])
  onDocumentClick(event: MouseEvent): void {
    if (this.isOpen && !this.elementRef.nativeElement.contains(event.target)) {
      this.isOpen = false;
    }
  }

  onNotificationClick(notification: Notification, event: Event): void {
    event.stopPropagation();
    if (!notification.isRead) {
      this.notificationService.markRead(notification.id).subscribe({ error: () => {} });
    }
    if (notification.link) {
      this.router.navigateByUrl(notification.link);
    }
    this.isOpen = false;
  }

  onMarkAllRead(event: Event): void {
    event.stopPropagation();
    this.notificationService.markAllRead().subscribe({ error: () => {} });
  }

  onViewAll(event: Event): void {
    event.stopPropagation();
    this.router.navigate(['/notifications']);
    this.isOpen = false;
  }

  iconForType(type: NotificationType): string {
    switch (type) {
      case NotificationType.TaskAssigned:
      case NotificationType.TaskReassigned:
        return 'inbox';
      case NotificationType.TaskDueSoon:
        return 'clock';
      case NotificationType.TaskOverdue:
        return 'alert';
      case NotificationType.TaskCompleted:
        return 'check';
      default:
        return 'bell';
    }
  }

  formatTimeAgo(iso: string): string {
    const diffMs = Date.now() - new Date(iso).getTime();
    const minutes = Math.floor(diffMs / 60000);
    if (minutes < 1) return 'vừa xong';
    if (minutes < 60) return `${minutes} phút trước`;
    const hours = Math.floor(minutes / 60);
    if (hours < 24) return `${hours} giờ trước`;
    const days = Math.floor(hours / 24);
    if (days < 7) return `${days} ngày trước`;
    return new Date(iso).toLocaleDateString('vi-VN');
  }
}
