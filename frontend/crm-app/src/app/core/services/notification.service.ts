import { Injectable } from '@angular/core';
import { BehaviorSubject, Observable, tap } from 'rxjs';
import { ApiService } from './api.service';
import {
  Notification,
  NotificationFilter,
  NotificationListResponse,
  RolePreference,
  RolePreferenceUpdate
} from '../models/notification.model';

@Injectable({ providedIn: 'root' })
export class NotificationService {
  private unreadCountSubject = new BehaviorSubject<number>(0);
  unreadCount$ = this.unreadCountSubject.asObservable();

  private latestSubject = new BehaviorSubject<Notification[]>([]);
  /**
   * Cache 20 notification mới nhất cho dropdown. Push từ realtime service prepend vào đây.
   * List page tự fetch riêng, không dùng cache này.
   */
  latest$ = this.latestSubject.asObservable();

  constructor(private api: ApiService) {}

  list(filter: NotificationFilter): Observable<NotificationListResponse> {
    return this.api.get<NotificationListResponse>('notifications', this.api.buildParams(filter));
  }

  getUnreadCount(): Observable<number> {
    return this.api.get<number>('notifications/unread-count').pipe(
      tap(count => this.unreadCountSubject.next(count))
    );
  }

  loadLatest(pageSize = 20): Observable<NotificationListResponse> {
    return this.api.get<NotificationListResponse>(
      'notifications',
      this.api.buildParams({ page: 1, pageSize })
    ).pipe(
      tap(response => this.latestSubject.next(response.items))
    );
  }

  markRead(id: string): Observable<void> {
    return this.api.post<void>(`notifications/${id}/read`, {}).pipe(
      tap(() => {
        const list = this.latestSubject.value.map(n =>
          n.id === id ? { ...n, isRead: true, readAt: new Date().toISOString() } : n
        );
        this.latestSubject.next(list);
        this.decrementUnread();
      })
    );
  }

  markAllRead(): Observable<number> {
    return this.api.post<number>('notifications/read-all', {}).pipe(
      tap(() => {
        const list = this.latestSubject.value.map(n => ({
          ...n,
          isRead: true,
          readAt: n.readAt ?? new Date().toISOString()
        }));
        this.latestSubject.next(list);
        this.unreadCountSubject.next(0);
      })
    );
  }

  delete(id: string): Observable<void> {
    return this.api.delete<void>(`notifications/${id}`).pipe(
      tap(() => {
        const removed = this.latestSubject.value.find(n => n.id === id);
        this.latestSubject.next(this.latestSubject.value.filter(n => n.id !== id));
        if (removed && !removed.isRead) {
          this.decrementUnread();
        }
      })
    );
  }

  // Realtime push hooks — gọi từ NotificationRealtimeService
  pushIncoming(notification: Notification): void {
    const list = [notification, ...this.latestSubject.value].slice(0, 20);
    this.latestSubject.next(list);
  }

  setUnreadCount(count: number): void {
    this.unreadCountSubject.next(Math.max(0, count));
  }

  reset(): void {
    this.unreadCountSubject.next(0);
    this.latestSubject.next([]);
  }

  // Admin preferences
  getAllPreferences(): Observable<RolePreference[]> {
    return this.api.get<RolePreference[]>('admin/notification-preferences');
  }

  updatePreferences(items: RolePreferenceUpdate[]): Observable<void> {
    return this.api.put<void>('admin/notification-preferences', { items });
  }

  resetPreferences(): Observable<void> {
    return this.api.post<void>('admin/notification-preferences/reset', {});
  }

  private decrementUnread(): void {
    this.unreadCountSubject.next(Math.max(0, this.unreadCountSubject.value - 1));
  }
}
