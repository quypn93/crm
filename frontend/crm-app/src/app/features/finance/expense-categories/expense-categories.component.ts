import { Component, OnInit } from '@angular/core';
import { FinanceService } from '../../../core/services/finance.service';
import { ToastService } from '../../../core/services/toast.service';
import { ExpenseCategory } from '../../../core/models/finance.model';

@Component({
  selector: 'app-expense-categories',
  template: `
    <div class="page-container">
      <div class="page-header">
        <h1>Đầu mục chi phí cố định</h1>
        <button class="btn btn-primary" (click)="openForm()">+ Thêm đầu mục</button>
      </div>

      <p class="hint">
        Kế toán tự thêm đầu mục khi thiếu. Đầu mục mặc định không xóa được — bỏ tick
        <strong>Hoạt động</strong> để ẩn khỏi form nhập chi phí.
      </p>

      <table class="table">
        <thead>
          <tr>
            <th style="width:70px">Thứ tự</th>
            <th>Tên đầu mục</th>
            <th>Mô tả</th>
            <th style="width:110px">Đã dùng</th>
            <th style="width:100px">Hoạt động</th>
            <th style="width:140px"></th>
          </tr>
        </thead>
        <tbody>
          <tr *ngIf="loading"><td colspan="6" class="empty">Đang tải...</td></tr>
          <tr *ngIf="!loading && categories.length === 0">
            <td colspan="6" class="empty">Chưa có đầu mục nào.</td>
          </tr>
          <tr *ngFor="let c of categories" [class.inactive]="!c.isActive">
            <td>{{ c.sortOrder }}</td>
            <td class="strong">
              {{ c.name }}
              <span class="badge system" *ngIf="c.isSystem">Mặc định</span>
            </td>
            <td class="muted">{{ c.description || '' }}</td>
            <td>{{ c.usageCount }} khoản</td>
            <td>
              <span class="badge" [class.on]="c.isActive" [class.off]="!c.isActive">
                {{ c.isActive ? 'Đang dùng' : 'Đã ẩn' }}
              </span>
            </td>
            <td class="actions">
              <button class="btn btn-sm" (click)="openForm(c)">Sửa</button>
              <button class="btn btn-sm btn-danger" (click)="remove(c)"
                      [disabled]="c.isSystem || c.usageCount > 0">Xóa</button>
            </td>
          </tr>
        </tbody>
      </table>

      <div class="modal-overlay" *ngIf="showForm" (click)="showForm = false">
        <div class="modal" (click)="$event.stopPropagation()">
          <div class="modal-header">
            <h3>{{ form.id ? 'Sửa đầu mục' : 'Thêm đầu mục' }}</h3>
            <button class="btn-close" (click)="showForm = false">×</button>
          </div>
          <div class="modal-body">
            <div class="form-group">
              <label>Tên đầu mục *</label>
              <input type="text" [(ngModel)]="form.name" placeholder="VD: Chi phí bảo trì máy">
            </div>
            <div class="form-group">
              <label>Mô tả</label>
              <input type="text" [(ngModel)]="form.description">
            </div>
            <div class="form-group">
              <label>Thứ tự hiển thị</label>
              <input type="number" [(ngModel)]="form.sortOrder">
            </div>
            <label class="cb">
              <input type="checkbox" [(ngModel)]="form.isActive">
              <span>Hoạt động (hiện trong form nhập chi phí)</span>
            </label>
          </div>
          <div class="modal-footer">
            <button class="btn btn-secondary" (click)="showForm = false">Hủy</button>
            <button class="btn btn-primary" (click)="save()" [disabled]="busy || !form.name">
              {{ busy ? 'Đang lưu...' : 'Lưu' }}
            </button>
          </div>
        </div>
      </div>
    </div>
  `,
  styles: [`
    .page-container { padding:24px; }
    .page-header { display:flex; justify-content:space-between; align-items:center; margin-bottom:8px; }
    .page-header h1 { margin:0; font-size:22px; font-weight:600; }
    .hint { color:#64748b; font-size:13px; margin:0 0 16px; max-width:720px; }
    .table { width:100%; border-collapse:collapse; font-size:14px; background:#fff; border-radius:8px; overflow:hidden; box-shadow:0 1px 3px rgba(0,0,0,.06); }
    .table th { background:#f8fafc; font-weight:600; color:#475569; }
    .table th, .table td { padding:11px 12px; border-bottom:1px solid #e2e8f0; text-align:left; }
    .table tr:last-child td { border-bottom:none; }
    .table tr.inactive td { color:#94a3b8; }
    .empty { text-align:center; color:#94a3b8; padding:24px; }
    .strong { font-weight:600; }
    .muted { color:#94a3b8; }
    .badge { padding:2px 8px; border-radius:10px; font-size:11px; background:#e2e8f0; }
    .badge.system { background:#e0e7ff; color:#3730a3; margin-left:6px; }
    .badge.on { background:#dcfce7; color:#166534; }
    .badge.off { background:#fee2e2; color:#991b1b; }
    .actions { white-space:nowrap; }
    .actions .btn-sm { margin-right:4px; }
    .btn { padding:8px 16px; border:none; border-radius:6px; cursor:pointer; font-size:14px; }
    .btn:disabled { opacity:.4; cursor:not-allowed; }
    .btn-primary { background:#6366f1; color:#fff; }
    .btn-secondary { background:#e2e8f0; color:#1e293b; }
    .btn-danger { background:#ef4444; color:#fff; }
    .btn-sm { padding:4px 10px; font-size:12px; background:#e2e8f0; }
    .btn-close { background:none; border:none; font-size:24px; cursor:pointer; color:#64748b; }
    .modal-overlay { position:fixed; inset:0; background:rgba(0,0,0,.5); display:flex; align-items:center; justify-content:center; z-index:1000; }
    .modal { background:#fff; border-radius:8px; max-width:460px; width:90%; }
    .modal-header { display:flex; justify-content:space-between; align-items:center; padding:16px 20px; border-bottom:1px solid #e2e8f0; }
    .modal-header h3 { margin:0; font-size:17px; }
    .modal-body { padding:20px; }
    .modal-footer { display:flex; justify-content:flex-end; gap:8px; padding:16px 20px; border-top:1px solid #e2e8f0; }
    .form-group { margin-bottom:14px; }
    .form-group label { display:block; margin-bottom:6px; font-size:13px; color:#475569; font-weight:500; }
    .form-group input { width:100%; padding:8px 12px; border:1px solid #cbd5e1; border-radius:6px; font-size:14px; box-sizing:border-box; }
    .cb { display:flex; gap:8px; align-items:center; font-size:14px; }
  `]
})
export class ExpenseCategoriesComponent implements OnInit {
  categories: ExpenseCategory[] = [];
  loading = false;
  busy = false;
  showForm = false;
  form: Partial<ExpenseCategory> = {};

  constructor(private finance: FinanceService, private toast: ToastService) {}

  ngOnInit(): void { this.load(); }

  load(): void {
    this.loading = true;
    this.finance.getExpenseCategories().subscribe({
      next: list => { this.categories = list; this.loading = false; },
      error: () => { this.loading = false; this.toast.error('Không tải được danh sách đầu mục.'); }
    });
  }

  openForm(category?: ExpenseCategory): void {
    this.form = category
      ? { ...category }
      : { name: '', sortOrder: this.nextSortOrder(), isActive: true };
    this.showForm = true;
  }

  save(): void {
    const dto: any = {
      id: this.form.id,
      name: (this.form.name || '').trim(),
      description: this.form.description,
      sortOrder: Number(this.form.sortOrder) || 0,
      isActive: this.form.isActive !== false
    };

    this.busy = true;
    const req = dto.id
      ? this.finance.updateExpenseCategory(dto.id, dto)
      : this.finance.createExpenseCategory(dto);

    req.subscribe({
      next: () => {
        this.busy = false;
        this.showForm = false;
        this.toast.success('Lưu đầu mục thành công.');
        this.load();
      },
      error: err => {
        this.busy = false;
        this.toast.error(err?.error?.message || 'Lưu thất bại.');
      }
    });
  }

  remove(category: ExpenseCategory): void {
    if (!confirm(`Xóa đầu mục "${category.name}"?`)) return;
    this.finance.deleteExpenseCategory(category.id).subscribe({
      next: () => { this.toast.success('Đã xóa.'); this.load(); },
      error: err => this.toast.error(err?.error?.message || 'Xóa thất bại.')
    });
  }

  private nextSortOrder(): number {
    const nonSystem = this.categories.filter(c => !c.isSystem).map(c => c.sortOrder);
    return nonSystem.length ? Math.max(...nonSystem) + 1 : 10;
  }
}
