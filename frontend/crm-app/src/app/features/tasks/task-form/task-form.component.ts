import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { TaskService, TaskPriority, TaskStatus, AssignableUser } from '../../../core/services/task.service';
import { CustomerService, Customer } from '../../../core/services/customer.service';
import { DealService, Deal } from '../../../core/services/deal.service';
import { AuthService } from '../../../core/services/auth.service';

@Component({
  selector: 'app-task-form',
  templateUrl: './task-form.component.html',
  styleUrls: ['./task-form.component.scss']
})
export class TaskFormComponent implements OnInit {
  taskForm: FormGroup;
  isLoading = false;
  isEditMode = false;
  taskId: string | null = null;
  errorMessage = '';

  statusOptions = this.taskService.getStatusOptions();
  priorityOptions = this.taskService.getPriorityOptions();
  customers: Customer[] = [];
  deals: Deal[] = [];
  assignableUsers: AssignableUser[] = [];
  canEditFields = true;
  canEditStatus = true;

  constructor(
    private fb: FormBuilder,
    private taskService: TaskService,
    private customerService: CustomerService,
    private dealService: DealService,
    private authService: AuthService,
    private router: Router,
    private route: ActivatedRoute
  ) {
    this.taskForm = this.fb.group({
      title: ['', [Validators.required]],
      description: [''],
      status: [TaskStatus.Pending],
      priority: [TaskPriority.Medium, [Validators.required]],
      dueDate: [''],
      customerId: [''],
      dealId: [''],
      assignedToUserId: ['']
    });
  }

  ngOnInit(): void {
    this.loadCustomers();
    this.loadDeals();
    this.loadAssignableUsers();

    const id = this.route.snapshot.paramMap.get('id');
    if (id && id !== 'new') {
      this.isEditMode = true;
      this.taskId = id;
      this.loadTask();
    }
  }

  loadCustomers(): void {
    this.customerService.getCustomers({ pageSize: 1000 }).subscribe({
      next: (response) => {
        this.customers = response?.items || [];
      }
    });
  }

  loadDeals(): void {
    this.dealService.getDeals({ pageSize: 1000 }).subscribe({
      next: (response) => {
        this.deals = response?.items || [];
      }
    });
  }

  loadAssignableUsers(): void {
    this.taskService.getAssignableUsers().subscribe({
      next: (users) => {
        this.assignableUsers = users || [];
      }
    });
  }

  loadTask(): void {
    if (!this.taskId) return;

    this.isLoading = true;
    this.taskService.getTask(this.taskId).subscribe({
      next: (task) => {
        this.taskForm.patchValue({
          title: task.title,
          description: task.description,
          status: task.status,
          priority: task.priority,
          dueDate: task.dueDate ? this.formatDate(new Date(task.dueDate)) : '',
          customerId: task.customerId || '',
          dealId: task.dealId || '',
          assignedToUserId: task.assignedToUserId || ''
        });
        this.applyPermissions(task);
        this.isLoading = false;
      },
      error: () => {
        this.errorMessage = 'Không thể tải thông tin công việc';
        this.isLoading = false;
      }
    });
  }

  // Người tạo + Admin: full edit. Assignee: chỉ status. Khác: redirect.
  private applyPermissions(task: { createdByUserId: string; assignedToUserId?: string }): void {
    const userId = this.authService.getCurrentUser()?.id;
    const isCreator = !!userId && task.createdByUserId === userId;
    const isAssignee = !!userId && task.assignedToUserId === userId;
    const isAdmin = this.authService.isAdmin();

    this.canEditFields = isCreator || isAdmin;
    this.canEditStatus = this.canEditFields || isAssignee;

    if (!this.canEditStatus) {
      this.router.navigate(['/tasks']);
      return;
    }

    if (!this.canEditFields) {
      Object.keys(this.taskForm.controls).forEach(key => {
        if (key !== 'status') this.taskForm.get(key)?.disable();
      });
    }
  }

  formatDate(date: Date): string {
    return date.toISOString().split('T')[0];
  }

  onSubmit(): void {
    if (this.taskForm.invalid) {
      this.taskForm.markAllAsTouched();
      return;
    }

    this.isLoading = true;
    this.errorMessage = '';

    // Assignee chỉ được update status
    if (this.isEditMode && this.taskId && !this.canEditFields && this.canEditStatus) {
      const status = this.taskForm.get('status')?.value;
      this.taskService.updateTaskStatus(this.taskId, status).subscribe({
        next: () => this.router.navigate(['/tasks']),
        error: (error) => {
          this.isLoading = false;
          this.errorMessage = error.error?.message || 'Cập nhật trạng thái thất bại.';
        }
      });
      return;
    }

    const formData = {
      ...this.taskForm.value,
      customerId: this.taskForm.value.customerId || null,
      dealId: this.taskForm.value.dealId || null,
      assignedToUserId: this.taskForm.value.assignedToUserId || null
    };

    if (this.isEditMode && this.taskId) {
      this.taskService.updateTask(this.taskId, { ...formData, id: this.taskId }).subscribe({
        next: () => {
          this.router.navigate(['/tasks']);
        },
        error: (error) => {
          this.isLoading = false;
          this.errorMessage = error.error?.message || 'Cập nhật thất bại. Vui lòng thử lại.';
        }
      });
    } else {
      this.taskService.createTask(formData).subscribe({
        next: () => {
          this.router.navigate(['/tasks']);
        },
        error: (error) => {
          this.isLoading = false;
          this.errorMessage = error.error?.message || 'Thêm công việc thất bại. Vui lòng thử lại.';
        }
      });
    }
  }

  cancel(): void {
    this.router.navigate(['/tasks']);
  }

  get f() {
    return this.taskForm.controls;
  }
}
