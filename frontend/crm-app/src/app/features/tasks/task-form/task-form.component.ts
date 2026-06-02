import { Component, ElementRef, OnInit, ViewChild } from '@angular/core';
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
  @ViewChild('descriptionEditor') descriptionEditor?: ElementRef<HTMLDivElement>;
  @ViewChild('descriptionImageInput') descriptionImageInput?: ElementRef<HTMLInputElement>;

  taskForm: FormGroup;
  isLoading = false;
  isUploadingDescriptionImage = false;
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
      workResult: [''],
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
          workResult: task.workResult || '',
          status: task.status,
          priority: task.priority,
          dueDate: task.dueDate ? this.formatDate(new Date(task.dueDate)) : '',
          customerId: task.customerId || '',
          dealId: task.dealId || '',
          assignedToUserId: task.assignedToUserId || ''
        });
        this.renderDescriptionEditor(task.description || '');
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
      this.taskForm.get('workResult')?.enable();
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
    this.syncDescriptionFromEditor();

    // Assignee chỉ được update status
    if (this.isEditMode && this.taskId && !this.canEditFields && this.canEditStatus) {
      const status = this.taskForm.get('status')?.value;
      const workResult = this.taskForm.get('workResult')?.value;
      this.taskService.updateTaskStatus(this.taskId, status, workResult).subscribe({
        next: () => this.router.navigate(['/tasks']),
        error: (error) => {
          this.isLoading = false;
          this.errorMessage = error.error?.message || 'Cập nhật trạng thái thất bại.';
        }
      });
      return;
    }

    const rawValue = this.taskForm.getRawValue();
    const formData = {
      ...rawValue,
      dueDate: rawValue.dueDate || null,
      customerId: rawValue.customerId || null,
      dealId: rawValue.dealId || null,
      assignedToUserId: rawValue.assignedToUserId || null
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

  formatDescription(command: string): void {
    if (!this.canEditFields) return;
    this.descriptionEditor?.nativeElement.focus();
    document.execCommand(command, false);
    this.syncDescriptionFromEditor();
  }

  promptForLink(): void {
    if (!this.canEditFields) return;
    const url = window.prompt('Nhap link');
    if (!url) return;
    this.descriptionEditor?.nativeElement.focus();
    document.execCommand('createLink', false, url);
    this.syncDescriptionFromEditor();
  }

  openDescriptionImagePicker(): void {
    if (!this.canEditFields || this.isUploadingDescriptionImage) return;
    this.descriptionImageInput?.nativeElement.click();
  }

  onDescriptionImageSelected(event: Event): void {
    const input = event.target as HTMLInputElement;
    const file = input.files?.[0];
    if (!file) return;

    this.isUploadingDescriptionImage = true;
    this.taskService.uploadImage(file).subscribe({
      next: (url) => {
        this.insertDescriptionHtml(`<img src="${url}" alt="Anh cong viec">`);
        this.isUploadingDescriptionImage = false;
        input.value = '';
      },
      error: (error) => {
        this.isUploadingDescriptionImage = false;
        input.value = '';
        this.errorMessage = error.error?.message || 'Upload anh that bai.';
      }
    });
  }

  onDescriptionInput(): void {
    this.syncDescriptionFromEditor();
  }

  private renderDescriptionEditor(html: string): void {
    setTimeout(() => {
      if (this.descriptionEditor) {
        this.descriptionEditor.nativeElement.innerHTML = this.sanitizeEditorHtml(html);
      }
    });
  }

  private insertDescriptionHtml(html: string): void {
    this.descriptionEditor?.nativeElement.focus();
    document.execCommand('insertHTML', false, html);
    this.syncDescriptionFromEditor();
  }

  private syncDescriptionFromEditor(): void {
    if (!this.descriptionEditor) return;
    const html = this.sanitizeEditorHtml(this.descriptionEditor.nativeElement.innerHTML);
    this.taskForm.get('description')?.setValue(html);
  }

  private sanitizeEditorHtml(html: string): string {
    const doc = new DOMParser().parseFromString(html || '', 'text/html');
    doc.querySelectorAll('script, iframe, object, embed, style').forEach(el => el.remove());
    doc.body.querySelectorAll('*').forEach(el => {
      Array.from(el.attributes).forEach(attr => {
        const name = attr.name.toLowerCase();
        const value = attr.value.trim().toLowerCase();
        if (name.startsWith('on') || value.startsWith('javascript:')) {
          el.removeAttribute(attr.name);
        }
      });
    });
    return doc.body.innerHTML;
  }
}
