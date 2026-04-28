import { Component, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { CdkDragDrop, transferArrayItem } from '@angular/cdk/drag-drop';
import { TaskService, Task, TaskSearchParams, TaskStatus, TaskPriority } from '../../../core/services/task.service';
import { AuthService } from '../../../core/services/auth.service';

type TaskTab = 'assigned-to-me' | 'created-by-me';
type ViewMode = 'list' | 'board';

interface BoardColumn {
  status: TaskStatus;
  label: string;
  tasks: Task[];
}

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
  activeTab: TaskTab = 'assigned-to-me';
  viewMode: ViewMode = 'list';

  boardColumns: BoardColumn[] = [
    { status: TaskStatus.Pending,    label: 'Chờ xử lý',     tasks: [] },
    { status: TaskStatus.InProgress, label: 'Đang thực hiện', tasks: [] },
    { status: TaskStatus.Completed,  label: 'Hoàn thành',    tasks: [] },
    { status: TaskStatus.Cancelled,  label: 'Đã hủy',        tasks: [] }
  ];
  boardListIds = this.boardColumns.map(c => `col-${c.status}`);

  // Expose enums to template
  TaskStatus = TaskStatus;
  TaskPriority = TaskPriority;

  statusOptions = this.taskService.getStatusOptions();
  priorityOptions = this.taskService.getPriorityOptions();

  constructor(
    private taskService: TaskService,
    private authService: AuthService,
    private router: Router
  ) {}

  ngOnInit(): void {
    this.loadTasks();
  }

  setTab(tab: TaskTab): void {
    if (this.activeTab === tab) return;
    this.activeTab = tab;
    this.currentPage = 1;
    this.loadTasks();
  }

  setViewMode(mode: ViewMode): void {
    if (this.viewMode === mode) return;
    this.viewMode = mode;
    this.currentPage = 1;
    this.loadTasks();
  }

  loadTasks(): void {
    this.isLoading = true;
    const userId = this.authService.getCurrentUser()?.id;
    const params: TaskSearchParams = {
      search: this.searchTerm || undefined,
      // Board mode hiển thị mọi cột nên bỏ filter status; status filter chỉ áp ở list mode.
      status: this.viewMode === 'list' ? (this.selectedStatus ?? undefined) : undefined,
      priority: this.selectedPriority ?? undefined,
      page: this.viewMode === 'board' ? 1 : this.currentPage,
      pageSize: this.viewMode === 'board' ? 500 : this.pageSize,
      assignedTo: this.activeTab === 'assigned-to-me' ? userId : undefined,
      createdBy: this.activeTab === 'created-by-me' ? userId : undefined
    };

    this.taskService.getTasks(params).subscribe({
      next: (response) => {
        this.tasks = response?.items || [];
        this.totalItems = response?.totalCount || 0;
        this.totalPages = response?.totalPages || 0;
        this.distributeBoard();
        this.isLoading = false;
      },
      error: () => {
        this.tasks = [];
        this.distributeBoard();
        this.isLoading = false;
      }
    });
  }

  private distributeBoard(): void {
    this.boardColumns.forEach(col => col.tasks = []);
    for (const t of this.tasks) {
      const col = this.boardColumns.find(c => c.status === t.status);
      if (col) col.tasks.push(t);
    }
  }

  // Drag-drop handler — đổi status khi card thả sang cột khác.
  onCardDrop(event: CdkDragDrop<Task[]>, target: BoardColumn): void {
    if (event.previousContainer === event.container) return;

    const task = event.previousContainer.data[event.previousIndex];
    if (!this.canMoveCard(task)) {
      // Không có quyền — không di chuyển. UI đã optimistic, ta phải rollback.
      return;
    }

    transferArrayItem(event.previousContainer.data, event.container.data, event.previousIndex, event.currentIndex);
    const previousStatus = task.status;
    task.status = target.status;

    this.taskService.updateTaskStatus(task.id, target.status).subscribe({
      error: () => {
        // Rollback nếu BE từ chối.
        task.status = previousStatus;
        this.distributeBoard();
      }
    });
  }

  canMoveCard(task: Task): boolean {
    const userId = this.authService.getCurrentUser()?.id;
    if (!userId) return false;
    return this.authService.isAdmin() || task.createdByUserId === userId || task.assignedToUserId === userId;
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
