import { Component, OnInit } from '@angular/core';
import { FinanceService } from '../../../core/services/finance.service';
import { ToastService } from '../../../core/services/toast.service';
import {
  ExpenseCategory, FixedExpense, FixedExpenseListResult
} from '../../../core/models/finance.model';

@Component({
  selector: 'app-fixed-expenses',
  template: `
    <div class="page-container">
      <div class="page-header">
        <h1>Chi phí cố định</h1>
        <div class="header-actions">
          <a routerLink="/finance/expense-categories" class="btn btn-secondary">Quản lý đầu mục</a>
          <button class="btn btn-primary" (click)="openForm()">+ Thêm chi phí</button>
        </div>
      </div>

      <p class="hint">
        Chi phí cố định nhập theo <strong>ngày</strong>. Thiếu đầu mục nào thì tự thêm ở trang
        <a routerLink="/finance/expense-categories">Quản lý đầu mục</a>.
      </p>

      <div class="filter-bar">
        <label class="filter-date"><span>Từ ngày</span><input type="date" [(ngModel)]="dateFrom" (change)="load()"></label>
        <label class="filter-date"><span>Đến ngày</span><input type="date" [(ngModel)]="dateTo" (change)="load()"></label>
        <label class="filter-date">
          <span>Đầu mục</span>
          <select [(ngModel)]="categoryId" (change)="load()">
            <option value="">Tất cả</option>
            <option *ngFor="let c of categories" [value]="c.id">{{ c.name }}</option>
          </select>
        </label>
        <button class="btn btn-secondary" (click)="clearFilters()">Xóa lọc</button>
      </div>

      <div class="content-row">
        <div class="table-wrap">
          <table class="table">
            <thead>
              <tr>
                <th>Ngày</th>
                <th>Đầu mục</th>
                <th class="num">Số tiền</th>
                <th>Ghi chú</th>
                <th>Người nhập</th>
                <th></th>
              </tr>
            </thead>
            <tbody>
              <tr *ngIf="loading"><td colspan="6" class="empty">Đang tải...</td></tr>
              <tr *ngIf="!loading && result.items.length === 0">
                <td colspan="6" class="empty">Chưa có khoản chi phí nào trong khoảng này.</td>
              </tr>
              <tr *ngFor="let e of result.items">
                <td class="nowrap">{{ formatDate(e.expenseDate) }}</td>
                <td>{{ e.categoryName }}</td>
                <td class="num strong">{{ e.amount | number }}</td>
                <td class="notes">{{ e.notes || '' }}</td>
                <td class="muted">{{ e.createdByUserName || '—' }}</td>
                <td class="actions">
                  <button class="btn btn-sm" (click)="openForm(e)">Sửa</button>
                  <button class="btn btn-sm btn-danger" (click)="remove(e)">Xóa</button>
                </td>
              </tr>
            </tbody>
            <tfoot *ngIf="result.items.length > 0">
              <tr>
                <td colspan="2">Tổng cộng</td>
                <td class="num">{{ result.grandTotal | number }}</td>
                <td colspan="3"></td>
              </tr>
            </tfoot>
          </table>

          <div class="pager" *ngIf="result.totalPages > 1">
            <button class="btn btn-sm" [disabled]="page <= 1" (click)="goPage(page - 1)">‹ Trước</button>
            <span>Trang {{ page }} / {{ result.totalPages }} — {{ result.totalCount | number }} khoản</span>
            <button class="btn btn-sm" [disabled]="page >= result.totalPages" (click)="goPage(page + 1)">Sau ›</button>
          </div>
        </div>

        <div class="side-panel">
          <h3>Tổng theo đầu mục</h3>
          <div class="cat-row" *ngFor="let t of result.totalsByCategory">
            <span class="cat-name">{{ t.categoryName }}</span>
            <span class="cat-amount">{{ t.amount | number }}</span>
          </div>
          <div class="cat-row total" *ngIf="result.totalsByCategory.length">
            <span class="cat-name">Tổng</span>
            <span class="cat-amount">{{ result.grandTotal | number }}</span>
          </div>
          <p class="muted" *ngIf="!result.totalsByCategory.length">Chưa có dữ liệu.</p>
        </div>
      </div>

      <div class="modal-overlay" *ngIf="showForm" (click)="showForm = false">
        <div class="modal" (click)="$event.stopPropagation()">
          <div class="modal-header">
            <h3>{{ form.id ? 'Sửa khoản chi phí' : 'Thêm chi phí' }}</h3>
            <button class="btn-close" (click)="showForm = false">×</button>
          </div>
          <div class="modal-body">
            <div class="form-group">
              <label>Ngày *</label>
              <input type="date" [(ngModel)]="form.expenseDate">
            </div>
            <div class="form-group">
              <label>Đầu mục *</label>
              <select [(ngModel)]="form.expenseCategoryId">
                <option value="">— Chọn đầu mục —</option>
                <option *ngFor="let c of activeCategories" [value]="c.id">{{ c.name }}</option>
              </select>
            </div>
            <div class="form-group">
              <label>Số tiền *</label>
              <input type="number" min="0" [(ngModel)]="form.amount">
            </div>
            <div class="form-group">
              <label>Ghi chú</label>
              <input type="text" [(ngModel)]="form.notes">
            </div>
          </div>
          <div class="modal-footer">
            <button class="btn btn-secondary" (click)="showForm = false">Hủy</button>
            <button class="btn btn-primary" (click)="save()"
                    [disabled]="busy || !form.expenseCategoryId || !form.expenseDate">
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
    .header-actions { display:flex; gap:8px; align-items:center; }
    .hint { color:#64748b; font-size:13px; margin:0 0 14px; }
    .hint a { color:#4f46e5; }
    .filter-bar { display:flex; align-items:center; gap:12px; flex-wrap:wrap; margin-bottom:14px; }
    .filter-date { display:flex; align-items:center; gap:6px; }
    .filter-date span { font-size:13px; color:#64748b; white-space:nowrap; }
    .filter-date input, .filter-date select { padding:8px 10px; border:1px solid #cbd5e1; border-radius:6px; font-size:14px; }
    .content-row { display:flex; gap:16px; align-items:flex-start; flex-wrap:wrap; }
    .table-wrap { flex:1; min-width:520px; overflow-x:auto; }
    .side-panel { width:280px; background:#fff; border-radius:8px; padding:16px; box-shadow:0 1px 3px rgba(0,0,0,.06); }
    .side-panel h3 { margin:0 0 12px; font-size:15px; color:#334155; }
    .cat-row { display:flex; justify-content:space-between; padding:7px 0; border-bottom:1px solid #f1f5f9; font-size:13px; }
    .cat-row.total { font-weight:600; border-bottom:none; border-top:2px solid #e2e8f0; margin-top:4px; padding-top:10px; }
    .cat-name { color:#475569; }
    .cat-amount { font-weight:600; color:#1e293b; }
    .table { width:100%; border-collapse:collapse; font-size:13px; background:#fff; border-radius:8px; overflow:hidden; box-shadow:0 1px 3px rgba(0,0,0,.06); }
    .table th { background:#f8fafc; font-weight:600; color:#475569; white-space:nowrap; }
    .table th, .table td { padding:10px; border-bottom:1px solid #e2e8f0; text-align:left; }
    .table th.num, .table td.num { text-align:right; }
    .table tfoot td { background:#f8fafc; font-weight:600; }
    .empty { text-align:center; color:#94a3b8; padding:24px; }
    .nowrap { white-space:nowrap; }
    .strong { font-weight:600; }
    .muted { color:#94a3b8; font-size:13px; }
    .notes { color:#64748b; max-width:260px; }
    .actions { white-space:nowrap; }
    .actions .btn-sm { margin-right:4px; }
    .pager { display:flex; align-items:center; gap:12px; justify-content:center; margin-top:14px; font-size:13px; color:#475569; }
    .btn { padding:8px 16px; border:none; border-radius:6px; cursor:pointer; font-size:14px; text-decoration:none; display:inline-block; }
    .btn:disabled { opacity:.5; cursor:not-allowed; }
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
    .form-group input, .form-group select { width:100%; padding:8px 12px; border:1px solid #cbd5e1; border-radius:6px; font-size:14px; box-sizing:border-box; }
  `]
})
export class FixedExpensesComponent implements OnInit {
  result: FixedExpenseListResult = {
    items: [], totalCount: 0, page: 1, pageSize: 100, totalPages: 1, grandTotal: 0, totalsByCategory: []
  };
  categories: ExpenseCategory[] = [];

  dateFrom = '';
  dateTo = '';
  categoryId = '';
  page = 1;

  loading = false;
  busy = false;
  showForm = false;
  form: Partial<FixedExpense> = {};

  constructor(private finance: FinanceService, private toast: ToastService) {}

  ngOnInit(): void {
    const now = new Date();
    this.dateFrom = this.toDateInput(new Date(now.getFullYear(), now.getMonth(), 1));
    this.dateTo = this.toDateInput(new Date(now.getFullYear(), now.getMonth() + 1, 0));
    this.loadCategories();
    this.load();
  }

  get activeCategories(): ExpenseCategory[] {
    return this.categories.filter(c => c.isActive);
  }

  loadCategories(): void {
    this.finance.getExpenseCategories().subscribe({
      next: list => this.categories = list,
      error: () => this.toast.error('Không tải được danh sách đầu mục.')
    });
  }

  load(): void {
    this.loading = true;
    this.finance.getFixedExpenses({
      dateFrom: this.dateFrom,
      dateTo: this.dateTo,
      categoryId: this.categoryId,
      page: this.page,
      pageSize: 100
    }).subscribe({
      next: res => { this.result = res; this.loading = false; },
      error: () => { this.loading = false; this.toast.error('Không tải được danh sách chi phí.'); }
    });
  }

  clearFilters(): void {
    this.dateFrom = '';
    this.dateTo = '';
    this.categoryId = '';
    this.page = 1;
    this.load();
  }

  goPage(p: number): void { this.page = p; this.load(); }

  openForm(entry?: FixedExpense): void {
    this.form = entry
      ? { ...entry }
      : { expenseDate: this.toDateInput(new Date()), amount: 0, expenseCategoryId: '' };
    this.showForm = true;
  }

  save(): void {
    const dto: any = {
      id: this.form.id,
      expenseDate: this.form.expenseDate,
      expenseCategoryId: this.form.expenseCategoryId,
      amount: Number(this.form.amount) || 0,
      notes: this.form.notes
    };

    this.busy = true;
    const req = dto.id
      ? this.finance.updateFixedExpense(dto.id, dto)
      : this.finance.createFixedExpense(dto);

    req.subscribe({
      next: () => {
        this.busy = false;
        this.showForm = false;
        this.toast.success('Lưu chi phí thành công.');
        this.load();
      },
      error: err => {
        this.busy = false;
        this.toast.error(err?.error?.message || 'Lưu thất bại.');
      }
    });
  }

  remove(entry: FixedExpense): void {
    if (!confirm(`Xóa khoản "${entry.categoryName}" ngày ${this.formatDate(entry.expenseDate)}?`)) return;
    this.finance.deleteFixedExpense(entry.id).subscribe({
      next: () => { this.toast.success('Đã xóa.'); this.load(); },
      error: err => this.toast.error(err?.error?.message || 'Xóa thất bại.')
    });
  }

  /** API trả yyyy-MM-dd (DateOnly) — hiển thị dd/MM/yyyy, không đụng tới múi giờ. */
  formatDate(value: string): string {
    if (!value) return '';
    const [y, m, d] = value.split('-');
    return d ? `${d}/${m}/${y}` : value;
  }

  private toDateInput(d: Date): string {
    const m = String(d.getMonth() + 1).padStart(2, '0');
    const day = String(d.getDate()).padStart(2, '0');
    return `${d.getFullYear()}-${m}-${day}`;
  }
}
