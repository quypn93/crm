import { Component, OnInit } from '@angular/core';
import { FinanceService } from '../../../core/services/finance.service';
import { ToastService } from '../../../core/services/toast.service';
import { UserManagementService, UserListItem } from '../../../core/services/user-management.service';
import { PayrollEntry, PayrollPeriod } from '../../../core/models/finance.model';

@Component({
  selector: 'app-payroll',
  template: `
    <div class="page-container">
      <div class="page-header">
        <h1>Chi phí nhân sự</h1>
        <div class="header-actions">
          <button class="btn btn-secondary" (click)="copyFromPrevious()" [disabled]="busy">Sao chép từ tháng trước</button>
          <button class="btn btn-primary" (click)="openForm()">+ Thêm nhân sự</button>
        </div>
      </div>

      <p class="hint">Chi phí nhân sự nhập theo <strong>tháng</strong>. Mỗi dòng là 1 nhân sự trong kỳ lương.</p>

      <div class="filter-bar">
        <label class="filter-date">
          <span>Kỳ lương</span>
          <select [(ngModel)]="month" (change)="load()">
            <option *ngFor="let m of months" [value]="m">Tháng {{ m }}</option>
          </select>
          <select [(ngModel)]="year" (change)="load()">
            <option *ngFor="let y of years" [value]="y">{{ y }}</option>
          </select>
        </label>
      </div>

      <div class="kpi-row">
        <div class="kpi"><span class="kpi-label">Số nhân sự</span><span class="kpi-value">{{ period.items.length }}</span></div>
        <div class="kpi"><span class="kpi-label">Lương</span><span class="kpi-value">{{ period.totalSalary | number }} đ</span></div>
        <div class="kpi"><span class="kpi-label">Phụ cấp</span><span class="kpi-value">{{ period.totalAllowance | number }} đ</span></div>
        <div class="kpi"><span class="kpi-label">Bảo hiểm</span><span class="kpi-value">{{ period.totalInsurance | number }} đ</span></div>
        <div class="kpi"><span class="kpi-label">Khác</span><span class="kpi-value">{{ period.totalOther | number }} đ</span></div>
        <div class="kpi total"><span class="kpi-label">Tổng chi phí nhân sự</span><span class="kpi-value">{{ period.grandTotal | number }} đ</span></div>
      </div>

      <div class="table-wrap">
        <table class="table">
          <thead>
            <tr>
              <th>Nhân sự</th>
              <th>Chức danh</th>
              <th class="num">Lương</th>
              <th class="num">Phụ cấp</th>
              <th class="num">Bảo hiểm</th>
              <th class="num">Chi phí khác</th>
              <th class="num">Tổng</th>
              <th>Ghi chú</th>
              <th></th>
            </tr>
          </thead>
          <tbody>
            <tr *ngIf="loading"><td colspan="9" class="empty">Đang tải...</td></tr>
            <tr *ngIf="!loading && period.items.length === 0">
              <td colspan="9" class="empty">Chưa có dữ liệu lương tháng {{ month }}/{{ year }}.</td>
            </tr>
            <tr *ngFor="let e of period.items">
              <td class="strong">{{ e.employeeName }}</td>
              <td>{{ e.position || '—' }}</td>
              <td class="num">{{ e.salary | number }}</td>
              <td class="num">{{ e.allowance | number }}</td>
              <td class="num">{{ e.insurance | number }}</td>
              <td class="num">{{ e.otherCost | number }}</td>
              <td class="num strong">{{ e.totalAmount | number }}</td>
              <td class="notes">{{ e.notes || '' }}</td>
              <td class="actions">
                <button class="btn btn-sm" (click)="openForm(e)">Sửa</button>
                <button class="btn btn-sm btn-danger" (click)="remove(e)">Xóa</button>
              </td>
            </tr>
          </tbody>
          <tfoot *ngIf="period.items.length > 0">
            <tr>
              <td colspan="2">Tổng cộng</td>
              <td class="num">{{ period.totalSalary | number }}</td>
              <td class="num">{{ period.totalAllowance | number }}</td>
              <td class="num">{{ period.totalInsurance | number }}</td>
              <td class="num">{{ period.totalOther | number }}</td>
              <td class="num">{{ period.grandTotal | number }}</td>
              <td colspan="2"></td>
            </tr>
          </tfoot>
        </table>
      </div>

      <div class="modal-overlay" *ngIf="showForm" (click)="showForm = false">
        <div class="modal" (click)="$event.stopPropagation()">
          <div class="modal-header">
            <h3>{{ form.id ? 'Sửa dòng lương' : 'Thêm nhân sự' }} — tháng {{ month }}/{{ year }}</h3>
            <button class="btn-close" (click)="showForm = false">×</button>
          </div>
          <div class="modal-body">
            <div class="form-group">
              <label>Chọn từ danh sách tài khoản</label>
              <select [ngModel]="form.userId || ''" (ngModelChange)="onPickUser($event)">
                <option value="">— Nhập tay tên nhân sự —</option>
                <option *ngFor="let u of users" [value]="u.id">{{ u.fullName }}</option>
              </select>
            </div>
            <div class="form-group">
              <label>Tên nhân sự *</label>
              <input type="text" [(ngModel)]="form.employeeName" placeholder="Nguyễn Văn A">
            </div>
            <div class="form-group">
              <label>Chức danh / bộ phận</label>
              <input type="text" [(ngModel)]="form.position" placeholder="Nhân viên may">
            </div>
            <div class="form-row">
              <div class="form-group"><label>Lương</label><input type="number" min="0" [(ngModel)]="form.salary"></div>
              <div class="form-group"><label>Phụ cấp</label><input type="number" min="0" [(ngModel)]="form.allowance"></div>
            </div>
            <div class="form-row">
              <div class="form-group"><label>Bảo hiểm</label><input type="number" min="0" [(ngModel)]="form.insurance"></div>
              <div class="form-group"><label>Chi phí khác</label><input type="number" min="0" [(ngModel)]="form.otherCost"></div>
            </div>
            <div class="form-group">
              <label>Ghi chú</label>
              <input type="text" [(ngModel)]="form.notes">
            </div>
            <div class="form-total">Tổng: <strong>{{ formTotal | number }} đ</strong></div>
          </div>
          <div class="modal-footer">
            <button class="btn btn-secondary" (click)="showForm = false">Hủy</button>
            <button class="btn btn-primary" (click)="save()" [disabled]="busy || !form.employeeName">
              {{ busy ? 'Đang lưu...' : 'Lưu' }}
            </button>
          </div>
        </div>
      </div>
    </div>
  `,
  styles: [`
    .page-container { padding:24px; }
    .page-header { display:flex; justify-content:space-between; align-items:center; margin-bottom:8px; flex-wrap:wrap; gap:12px; }
    .page-header h1 { margin:0; font-size:22px; font-weight:600; }
    .header-actions { display:flex; gap:8px; }
    .hint { color:#64748b; font-size:13px; margin:0 0 14px; }
    .filter-bar { display:flex; gap:12px; margin-bottom:14px; }
    .filter-date { display:flex; align-items:center; gap:8px; }
    .filter-date span { font-size:13px; color:#64748b; }
    .filter-date select { padding:8px 10px; border:1px solid #cbd5e1; border-radius:6px; font-size:14px; }
    .kpi-row { display:flex; gap:12px; flex-wrap:wrap; margin-bottom:14px; }
    .kpi { background:#fff; border-radius:8px; padding:12px 16px; box-shadow:0 1px 3px rgba(0,0,0,.06); min-width:140px; display:flex; flex-direction:column; gap:4px; }
    .kpi.total { background:#eef2ff; }
    .kpi-label { font-size:12px; color:#64748b; }
    .kpi-value { font-size:17px; font-weight:600; color:#1e293b; }
    .table-wrap { overflow-x:auto; }
    .table { width:100%; border-collapse:collapse; font-size:13px; background:#fff; border-radius:8px; overflow:hidden; box-shadow:0 1px 3px rgba(0,0,0,.06); }
    .table th { background:#f8fafc; font-weight:600; color:#475569; white-space:nowrap; }
    .table th, .table td { padding:10px; border-bottom:1px solid #e2e8f0; text-align:left; }
    .table th.num, .table td.num { text-align:right; }
    .table tfoot td { background:#f8fafc; font-weight:600; }
    .empty { text-align:center; color:#94a3b8; padding:24px; }
    .strong { font-weight:600; }
    .notes { color:#64748b; max-width:220px; }
    .actions { white-space:nowrap; }
    .actions .btn-sm { margin-right:4px; }
    .btn { padding:8px 16px; border:none; border-radius:6px; cursor:pointer; font-size:14px; }
    .btn:disabled { opacity:.5; cursor:not-allowed; }
    .btn-primary { background:#6366f1; color:#fff; }
    .btn-secondary { background:#e2e8f0; color:#1e293b; }
    .btn-danger { background:#ef4444; color:#fff; }
    .btn-sm { padding:4px 10px; font-size:12px; background:#e2e8f0; }
    .btn-close { background:none; border:none; font-size:24px; cursor:pointer; color:#64748b; }
    .modal-overlay { position:fixed; inset:0; background:rgba(0,0,0,.5); display:flex; align-items:center; justify-content:center; z-index:1000; }
    .modal { background:#fff; border-radius:8px; max-width:560px; width:90%; max-height:90vh; overflow-y:auto; }
    .modal-header { display:flex; justify-content:space-between; align-items:center; padding:16px 20px; border-bottom:1px solid #e2e8f0; }
    .modal-header h3 { margin:0; font-size:17px; }
    .modal-body { padding:20px; }
    .modal-footer { display:flex; justify-content:flex-end; gap:8px; padding:16px 20px; border-top:1px solid #e2e8f0; }
    .form-row { display:flex; gap:12px; }
    .form-row .form-group { flex:1; }
    .form-group { margin-bottom:14px; }
    .form-group label { display:block; margin-bottom:6px; font-size:13px; color:#475569; font-weight:500; }
    .form-group input, .form-group select { width:100%; padding:8px 12px; border:1px solid #cbd5e1; border-radius:6px; font-size:14px; box-sizing:border-box; }
    .form-total { padding:10px 12px; background:#f8fafc; border-radius:6px; font-size:14px; }
  `]
})
export class PayrollComponent implements OnInit {
  period: PayrollPeriod = {
    year: 0, month: 0, items: [],
    totalSalary: 0, totalAllowance: 0, totalInsurance: 0, totalOther: 0, grandTotal: 0
  };

  year = new Date().getFullYear();
  month = new Date().getMonth() + 1;
  months = Array.from({ length: 12 }, (_, i) => i + 1);
  years: number[] = [];

  users: UserListItem[] = [];
  loading = false;
  busy = false;
  showForm = false;
  form: Partial<PayrollEntry> = {};

  constructor(
    private finance: FinanceService,
    private toast: ToastService,
    private userService: UserManagementService
  ) {}

  ngOnInit(): void {
    const current = new Date().getFullYear();
    this.years = [current - 2, current - 1, current, current + 1];
    this.loadUsers();
    this.load();
  }

  load(): void {
    this.loading = true;
    this.finance.getPayroll(Number(this.year), Number(this.month)).subscribe({
      next: p => { this.period = p; this.loading = false; },
      error: () => { this.loading = false; this.toast.error('Không tải được bảng lương.'); }
    });
  }

  private loadUsers(): void {
    this.userService.getUsers({ page: 1, pageSize: 300, isActive: true }).subscribe({
      next: res => this.users = (res?.items || []).sort((a, b) =>
        (a.fullName || '').localeCompare(b.fullName || '', 'vi')),
      error: () => { /* không critical — kế toán vẫn gõ tay được */ }
    });
  }

  get formTotal(): number {
    return this.n(this.form.salary) + this.n(this.form.allowance)
      + this.n(this.form.insurance) + this.n(this.form.otherCost);
  }

  openForm(entry?: PayrollEntry): void {
    this.form = entry
      ? { ...entry }
      : { salary: 0, allowance: 0, insurance: 0, otherCost: 0, employeeName: '' };
    this.showForm = true;
  }

  onPickUser(userId: string): void {
    this.form.userId = userId || undefined;
    if (userId) {
      const u = this.users.find(x => x.id === userId);
      if (u) this.form.employeeName = u.fullName;
    }
  }

  save(): void {
    const dto: Partial<PayrollEntry> = {
      ...this.form,
      year: Number(this.year),
      month: Number(this.month),
      salary: this.n(this.form.salary),
      allowance: this.n(this.form.allowance),
      insurance: this.n(this.form.insurance),
      otherCost: this.n(this.form.otherCost)
    };

    this.busy = true;
    const req = dto.id
      ? this.finance.updatePayrollEntry(dto.id, dto)
      : this.finance.createPayrollEntry(dto);

    req.subscribe({
      next: () => {
        this.busy = false;
        this.showForm = false;
        this.toast.success('Lưu thành công.');
        this.load();
      },
      error: err => {
        this.busy = false;
        this.toast.error(err?.error?.message || 'Lưu thất bại.');
      }
    });
  }

  remove(entry: PayrollEntry): void {
    if (!confirm(`Xóa dòng lương của ${entry.employeeName}?`)) return;
    this.finance.deletePayrollEntry(entry.id).subscribe({
      next: () => { this.toast.success('Đã xóa.'); this.load(); },
      error: err => this.toast.error(err?.error?.message || 'Xóa thất bại.')
    });
  }

  copyFromPrevious(): void {
    if (!confirm(`Sao chép bảng lương tháng trước sang tháng ${this.month}/${this.year}?`)) return;
    this.busy = true;
    this.finance.copyPayrollFromPrevious(Number(this.year), Number(this.month)).subscribe({
      next: added => {
        this.busy = false;
        this.toast.success(`Đã sao chép ${added} dòng lương.`);
        this.load();
      },
      error: err => {
        this.busy = false;
        this.toast.error(err?.error?.message || 'Sao chép thất bại.');
      }
    });
  }

  private n(v: any): number { return Number(v) || 0; }
}
