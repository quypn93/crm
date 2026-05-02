import { Injectable } from '@angular/core';
import { HubConnection, HubConnectionBuilder, LogLevel, HubConnectionState } from '@microsoft/signalr';
import { environment } from '../../../environments/environment';
import { NotificationService } from './notification.service';
import { ToastService } from './toast.service';
import { StorageService } from './storage.service';
import { Notification, NotificationSeverity } from '../models/notification.model';

@Injectable({ providedIn: 'root' })
export class NotificationRealtimeService {
  private hub: HubConnection | null = null;

  constructor(
    private notificationService: NotificationService,
    private toast: ToastService,
    private storage: StorageService
  ) {}

  async connect(): Promise<void> {
    if (this.hub && this.hub.state !== HubConnectionState.Disconnected) {
      return;
    }

    const token = this.storage.getToken();
    if (!token) return;

    // SignalR cần URL tuyệt đối tới hub. apiUrl là http://host/api → strip /api để có root.
    const apiBase = environment.apiUrl.replace(/\/api\/?$/, '');
    const hubUrl = `${apiBase}/hubs/notifications`;

    this.hub = new HubConnectionBuilder()
      .withUrl(hubUrl, {
        // Token expire → ngắt kết nối; user cần login lại để renew.
        accessTokenFactory: () => this.storage.getToken() ?? ''
      })
      .withAutomaticReconnect([0, 2000, 5000, 10000, 30000])
      .configureLogging(LogLevel.Warning)
      .build();

    this.hub.on('notification', (n: Notification) => this.onNotification(n));
    this.hub.on('unreadCount', (count: number) => this.notificationService.setUnreadCount(count));

    try {
      await this.hub.start();
    } catch (err) {
      console.warn('SignalR connect thất bại:', err);
    }
  }

  async disconnect(): Promise<void> {
    if (!this.hub) return;
    try {
      await this.hub.stop();
    } catch {
      // ignore
    }
    this.hub = null;
  }

  private onNotification(n: Notification): void {
    this.notificationService.pushIncoming(n);

    // Toast cho severity ≥ warning. Info/Success chỉ tăng badge, tránh ồn.
    if (n.severity === NotificationSeverity.Warning) {
      this.toast.warning(n.title);
    } else if (n.severity === NotificationSeverity.Error) {
      this.toast.error(n.title);
    }
  }
}
