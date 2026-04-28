import { Component, OnInit } from '@angular/core';
import { DesignService, Design, DesignStatus, DesignStatusLabels } from '../../../core/services/design.service';
import { ToastService } from '../../../core/services/toast.service';
import { environment } from '../../../../environments/environment';

@Component({
  selector: 'app-design-my-tasks',
  templateUrl: './design-my-tasks.component.html',
  styleUrls: ['./design-my-tasks.component.scss']
})
export class DesignMyTasksComponent implements OnInit {
  readonly DesignStatus = DesignStatus;
  readonly statusLabels = DesignStatusLabels;

  tasks: Design[] = [];
  isLoading = false;
  filterStatus: DesignStatus | null = null;

  busyId: string | null = null;
  errorMessage = '';

  constructor(private designService: DesignService, private toast: ToastService) {}

  ngOnInit(): void { this.load(); }

  load(): void {
    this.isLoading = true;
    const status = this.filterStatus ?? undefined;
    this.designService.getMyTasks(status).subscribe({
      next: (list) => { this.tasks = list; this.isLoading = false; },
      error: () => { this.isLoading = false; }
    });
  }

  onFilterChange(value: string): void {
    this.filterStatus = value === '' ? null : Number(value) as DesignStatus;
    this.load();
  }

  resolveUrl(path?: string): string {
    if (!path) return '';
    if (path.startsWith('http')) return path;
    const origin = (environment.apiUrl || '').replace(/\/api\/?$/, '');
    return origin + (path.startsWith('/') ? path : '/' + path);
  }

  /** Các trạng thái được phép chuyển đến từ trạng thái hiện tại (trùng với backend). */
  availableTransitions(current: DesignStatus): DesignStatus[] {
    switch (current) {
      case DesignStatus.Assigned:   return [DesignStatus.InProgress, DesignStatus.Cancelled];
      case DesignStatus.InProgress: return [DesignStatus.Completed, DesignStatus.Cancelled, DesignStatus.Assigned];
      default: return [];
    }
  }

  updateStatus(task: Design, newStatus: DesignStatus): void {
    const label = this.statusLabels[newStatus];
    if (!confirm(`Chuyển "${task.designName}" sang trạng thái "${label}"?`)) return;
    this.busyId = task.id;
    this.errorMessage = '';
    this.designService.updateDesignStatus(task.id, newStatus).subscribe({
      next: () => {
        this.busyId = null;
        this.toast.success(`Đã cập nhật trạng thái: ${label}`);
        this.load();
      },
      error: (err) => {
        this.busyId = null;
        this.errorMessage = err?.error?.message || 'Không cập nhật được.';
      }
    });
  }
}
