import { Component, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { TaskService, Task, TaskSearchParams, TaskStatus, TaskPriority } from '../../../core/services/task.service';

@Component({
  selector: 'app-task-list',
  templateUrl: './task-list.component.html',
  styleUrls: ['./task-list.component.scss']
})
export class TaskListComponent implements OnInit {
  tasks: Task[] = [];
  isLoading = false;
  searchTerm = '';
  selectedStatus: TaskStatus | null = null;
  selectedPriority: TaskPriority | null = null;
  currentPage = 1;
  pageSize = 10;
  totalItems = 0;
  totalPages = 0;

  // Expose enums to template
  TaskStatus = TaskStatus;
  TaskPriority = TaskPriority;

  statusOptions = this.taskService.getStatusOptions();
  priorityOptions = this.taskService.getPriorityOptions();

  constructor(
    private taskService: TaskService,
    private router: Router
  ) {}

  ngOnInit(): void {
    this.loadTasks();
  }

  loadTasks(): void {
    this.isLoading = true;
    const params: TaskSearchParams = {
      searchTerm: this.searchTerm,
      status: this.selectedStatus ?? undefined,
      priority: this.selectedPriority ?? undefined,
      page: this.currentPage,
      pageSize: this.pageSize
    };

    this.taskService.getTasks(params).subscribe({
      next: (response) => {
        this.tasks = response?.items || [];
        this.totalItems = response?.totalCount || 0;
        this.totalPages = response?.totalPages || 0;
        this.isLoading = false;
      },
      error: () => {
        this.tasks = [];
        this.isLoading = false;
      }
    });
  }

  onSearch(): void {
    this.currentPage = 1;
    this.loadTasks();
  }

  onFilterChange(): void {
    this.currentPage = 1;
    this.loadTasks();
  }

  onStatusChange(event: Event): void {
    const value = (event.target as HTMLSelectElement).value;
    this.selectedStatus = value ? Number(value) as TaskStatus : null;
    this.onFilterChange();
  }

  onPriorityChange(event: Event): void {
    const value = (event.target as HTMLSelectElement).value;
    this.selectedPriority = value ? Number(value) as TaskPriority : null;
    this.onFilterChange();
  }

  onPageChange(page: number): void {
    this.currentPage = page;
    this.loadTasks();
  }

  addTask(): void {
    this.router.navigate(['/tasks/new']);
  }

  editTask(task: Task): void {
    this.router.navigate(['/tasks', task.id, 'edit']);
  }

  toggleStatus(task: Task): void {
    let newStatus: TaskStatus;
    if (task.status === TaskStatus.Pending) {
      newStatus = TaskStatus.InProgress;
    } else if (task.status === TaskStatus.InProgress) {
      newStatus = TaskStatus.Completed;
    } else {
      return;
    }

    this.taskService.updateTaskStatus(task.id, newStatus).subscribe({
      next: () => {
        this.loadTasks();
      }
    });
  }

  deleteTask(task: Task): void {
    if (confirm(`Bạn có chắc muốn xóa công việc "${task.title}"?`)) {
      this.taskService.deleteTask(task.id).subscribe({
        next: () => {
          this.loadTasks();
        }
      });
    }
  }

  getStatusClass(status: TaskStatus): string {
    const classes: { [key: number]: string } = {
      [TaskStatus.Pending]: 'status-pending',
      [TaskStatus.InProgress]: 'status-progress',
      [TaskStatus.Completed]: 'status-completed',
      [TaskStatus.Cancelled]: 'status-cancelled'
    };
    return classes[status] || '';
  }

  getStatusLabel(status: TaskStatus): string {
    return this.taskService.getStatusLabel(status);
  }

  getPriorityClass(priority: TaskPriority): string {
    const classes: { [key: number]: string } = {
      [TaskPriority.Low]: 'priority-low',
      [TaskPriority.Medium]: 'priority-medium',
      [TaskPriority.High]: 'priority-high'
    };
    return classes[priority] || '';
  }

  getPriorityLabel(priority: TaskPriority): string {
    return this.taskService.getPriorityLabel(priority);
  }

  isOverdue(task: Task): boolean {
    if (!task.dueDate || task.status === TaskStatus.Completed || task.status === TaskStatus.Cancelled) {
      return false;
    }
    return new Date(task.dueDate) < new Date();
  }

  isTaskCompleted(task: Task): boolean {
    return task.status === TaskStatus.Completed;
  }

  isTaskDisabled(task: Task): boolean {
    return task.status === TaskStatus.Completed || task.status === TaskStatus.Cancelled;
  }

  getPages(): number[] {
    const pages: number[] = [];
    const start = Math.max(1, this.currentPage - 2);
    const end = Math.min(this.totalPages, this.currentPage + 2);
    for (let i = start; i <= end; i++) {
      pages.push(i);
    }
    return pages;
  }
}
