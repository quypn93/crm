import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { ApiService } from './api.service';

export enum TaskStatus {
  Pending = 0,
  InProgress = 1,
  Completed = 2,
  Cancelled = 3
}

export enum TaskPriority {
  Low = 0,
  Medium = 1,
  High = 2
}

export interface Task {
  id: string;
  title: string;
  description?: string;
  status: TaskStatus;
  priority: TaskPriority;
  dueDate?: string;
  reminderDate?: string;
  completedAt?: string;
  customerId?: string;
  customerName?: string;
  dealId?: string;
  dealTitle?: string;
  assignedToUserId?: string;
  assignedToUserName?: string;
  createdByUserId: string;
  createdByUserName?: string;
  createdAt: string;
  updatedAt?: string;
}

export interface TaskSearchParams {
  search?: string;
  status?: TaskStatus;
  priority?: TaskPriority;
  customerId?: string;
  dealId?: string;
  assignedTo?: string;
  createdBy?: string;
  isOverdue?: boolean;
  page?: number;
  pageSize?: number;
}

export interface AssignableUser {
  id: string;
  fullName: string;
  email: string;
  roles: string[];
}

export interface PagedResult<T> {
  items: T[];
  totalCount: number;
  page: number;
  pageSize: number;
  totalPages: number;
}

export interface CreateTaskDto {
  title: string;
  description?: string;
  priority?: TaskPriority;
  dueDate?: string;
  reminderDate?: string;
  customerId?: string;
  dealId?: string;
  assignedToUserId?: string;
}

export interface UpdateTaskDto extends CreateTaskDto {
  id: string;
  status?: TaskStatus;
}

@Injectable({
  providedIn: 'root'
})
export class TaskService {
  constructor(private api: ApiService) {}

  getTasks(params: TaskSearchParams): Observable<PagedResult<Task>> {
    return this.api.get<PagedResult<Task>>('tasks', this.api.buildParams(params));
  }

  getTask(id: string): Observable<Task> {
    return this.api.get<Task>(`tasks/${id}`);
  }

  getMyTasks(): Observable<Task[]> {
    return this.api.get<Task[]>('tasks/my-tasks');
  }

  getAssignableUsers(): Observable<AssignableUser[]> {
    return this.api.get<AssignableUser[]>('tasks/assignable-users');
  }

  getOverdueTasks(): Observable<Task[]> {
    return this.api.get<Task[]>('tasks/overdue');
  }

  createTask(task: CreateTaskDto): Observable<Task> {
    return this.api.post<Task>('tasks', task);
  }

  updateTask(id: string, task: UpdateTaskDto): Observable<Task> {
    return this.api.put<Task>(`tasks/${id}`, task);
  }

  updateTaskStatus(id: string, status: TaskStatus): Observable<Task> {
    return this.api.put<Task>(`tasks/${id}/status`, { taskId: id, status });
  }

  deleteTask(id: string): Observable<void> {
    return this.api.delete<void>(`tasks/${id}`);
  }

  getStatusOptions(): { value: TaskStatus; label: string }[] {
    return [
      { value: TaskStatus.Pending, label: 'Chờ xử lý' },
      { value: TaskStatus.InProgress, label: 'Đang thực hiện' },
      { value: TaskStatus.Completed, label: 'Hoàn thành' },
      { value: TaskStatus.Cancelled, label: 'Đã hủy' }
    ];
  }

  getPriorityOptions(): { value: TaskPriority; label: string }[] {
    return [
      { value: TaskPriority.Low, label: 'Thấp' },
      { value: TaskPriority.Medium, label: 'Trung bình' },
      { value: TaskPriority.High, label: 'Cao' }
    ];
  }

  getStatusLabel(status: TaskStatus): string {
    const option = this.getStatusOptions().find(o => o.value === status);
    return option?.label || '';
  }

  getPriorityLabel(priority: TaskPriority): string {
    const option = this.getPriorityOptions().find(o => o.value === priority);
    return option?.label || '';
  }
}
