import { Component, OnInit } from '@angular/core';
import { SettingsService } from '../../../core/services/settings.service';
import { DepositTransaction } from '../../../core/models/lookup.model';

@Component({
  selector: 'app-deposits-admin',
  template: `
    <div class="page-container">
      <div class="page-header">
        <div class="page-header-text">
          <h1>💰 Lịch sử cộng tiền</h1>
          <p class="subtitle">Giao dịch từ Casso webhook sẽ tự động xuất hiện ở đây. Sale có thể nhìn vào danh sách này để biết mã giao dịch nào là của mình và điền vào đơn hàng.</p>
        </div>
        <button class="btn btn-primary" (click)="showForm = true">+ Thêm thủ công</button>
      </div>

      <div class="filter-card">
        <div class="filter-bar">
          <div class="filter-search-wrapper">
            <svg class="filter-search-icon" xmlns="http://www.w3.org/2000/svg" width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor" stroke-width="2">
              <circle cx="11" cy="11" r="8"></circle>
              <line x1="21" y1="21" x2="16.65" y2="16.65"></line>
            </svg>
            <input type="text" class="filter-search" [(ngModel)]="searchText" (ngModelChange)="onFilterChange()"
                   placeholder="Tìm mã GD, nội dung, ngân hàng, số tiền...">
          </div>
          <label class="filter-date">
            <span>Từ ngày</span>
            <input type="date" [(ngModel)]="dateFrom" (ngModelChange)="onFilterChange()">
          </label>
          <label class="filter-date">
            <span>Đến ngày</span>
            <input type="date" [(ngModel)]="dateTo" (ngModelChange)="onFilterChange()">
          </label>
          <button class="btn btn-outline" *ngIf="searchText || dateFrom || dateTo" (click)="clearFilters()">Xóa lọc</button>
        </div>

        <div class="filter-summary" *ngIf="searchText || dateFrom || dateTo">
          Tìm thấy <strong>{{ filteredDeposits.length }}</strong> giao dịch — tổng <strong class="amount-highlight">{{ filteredTotal | number }} đ</strong>
        </div>
      </div>

      <div class="table-card">
        <table class="table">
          <thead>
            <tr>
              <th>Ngày</th>
              <th>Mã GD</th>
              <th class="text-right">Số tiền</th>
              <th>Ngân hàng</th>
              <th>Nội dung</th>
              <th>Nguồn</th>
              <th></th>
            </tr>
          </thead>
          <tbody>
            <tr *ngIf="filteredDeposits.length === 0" class="empty-row">
              <td colspan="7">Không có giao dịch nào khớp bộ lọc.</td>
            </tr>
            <tr *ngFor="let d of pagedDeposits" [class.split-parent]="d.isSplit">
              <td class="cell-date">{{ d.transactionDate | date:'dd/MM/yyyy' }}<span class="cell-time">{{ d.transactionDate | date:'HH:mm' }}</span></td>
              <td>
                <code>{{ d.code }}</code>
                <span class="badge split" *ngIf="d.isSplit">Đã tách</span>
                <span class="badge child" *ngIf="d.parentId">Tách</span>
                <span class="badge matched" *ngIf="d.matchedOrderId">Đã gắn đơn</span>
              </td>
              <td class="text-right amount-cell">{{ d.amount | number }} đ</td>
              <td>{{ d.bankName }}</td>
              <td class="cell-desc">{{ d.description }}</td>
              <td><span class="badge" [class.auto]="d.source==='casso'"><span class="badge-dot"></span>{{ d.source }}</span></td>
              <td class="actions">
                <button class="btn btn-sm" *ngIf="!d.isSplit && !d.matchedOrderId" (click)="openSplit(d)">Tách</button>
                <button class="btn btn-sm btn-danger" *ngIf="!d.isSplit" (click)="remove(d)">Xóa</button>
              </td>
            </tr>
          </tbody>
        </table>

        <div class="pagination" *ngIf="filteredDeposits.length > 0">
          <button class="page-btn" [disabled]="currentPage === 1" (click)="goToPage(currentPage - 1)">‹ Trước</button>
          <button *ngFor="let page of pageNumbers" class="page-btn" [class.active]="page === currentPage" (click)="goToPage(page)">{{ page }}</button>
          <button class="page-btn" [disabled]="currentPage === totalPages" (click)="goToPage(currentPage + 1)">Sau ›</button>
          <span class="page-info">
            Hiển thị {{ (currentPage - 1) * pageSize + 1 }}–{{ Math.min(currentPage * pageSize, filteredDeposits.length) }} / {{ filteredDeposits.length }}
          </span>
          <label class="page-size-select">
            <span>Mỗi trang</span>
            <select [ngModel]="pageSize" (ngModelChange)="onPageSizeChange($event)">
              <option [ngValue]="20">20</option>
              <option [ngValue]="50">50</option>
              <option [ngValue]="100">100</option>
              <option [ngValue]="200">200</option>
            </select>
          </label>
        </div>
      </div>

      <!-- Modal tách giao dịch gộp thành nhiều khoản -->
      <div class="modal-overlay" *ngIf="splitting" (click)="closeSplit()">
        <div class="modal" (click)="$event.stopPropagation()">
          <div class="modal-header">
            <h3>Tách giao dịch</h3>
            <button class="btn-close" (click)="closeSplit()">×</button>
          </div>
          <div class="modal-body">
            <p class="split-info">
              Mã gốc: <code>{{ splitting.code }}</code> —
              <strong class="amount-highlight">{{ splitting.amount | number }} đ</strong>
            </p>
            <p class="muted">Mỗi khoản con sẽ có mã <code>{{ splitting.code }}-1</code>, <code>{{ splitting.code }}-2</code>... để điền vào từng đơn hàng.</p>

            <div class="split-row" *ngFor="let p of splitParts; let i = index; trackBy: trackByIndex">
              <span class="split-code">{{ splitting.code }}-{{ i + 1 }}</span>
              <input type="number" min="0" [(ngModel)]="splitParts[i]" placeholder="Số tiền">
              <button class="btn btn-sm btn-danger" (click)="removeSplitPart(i)" [disabled]="splitParts.length <= 2">−</button>
            </div>
            <button class="btn btn-sm btn-secondary" (click)="splitParts.push(0)">+ Thêm khoản</button>

            <div class="split-summary" [class.ok]="splitRemainder === 0" [class.bad]="splitRemainder !== 0">
              Đã nhập: <strong>{{ splitTotal | number }} đ</strong>
              <ng-container *ngIf="splitRemainder > 0"> — còn thiếu {{ splitRemainder | number }} đ</ng-container>
              <ng-container *ngIf="splitRemainder < 0"> — vượt quá {{ -splitRemainder | number }} đ</ng-container>
              <ng-container *ngIf="splitRemainder === 0"> — khớp số tiền gốc ✓</ng-container>
            </div>
            <p class="error" *ngIf="splitError">{{ splitError }}</p>
          </div>
          <div class="modal-footer">
            <button class="btn btn-secondary" (click)="closeSplit()">Hủy</button>
            <button class="btn btn-primary" (click)="confirmSplit()" [disabled]="splitRemainder !== 0 || splitBusy">
              {{ splitBusy ? 'Đang tách...' : 'Tách' }}
            </button>
          </div>
        </div>
      </div>

      <div class="modal-overlay" *ngIf="showForm" (click)="showForm = false">
        <div class="modal" (click)="$event.stopPropagation()">
          <div class="modal-header">
            <h3>Thêm giao dịch thủ công</h3>
            <button class="btn-close" (click)="showForm = false">×</button>
          </div>
          <div class="modal-body">
            <div class="form-group">
              <label>Mã giao dịch *</label>
              <input type="text" [(ngModel)]="formData.code">
            </div>
            <div class="form-group">
              <label>Số tiền *</label>
              <input type="number" [(ngModel)]="formData.amount" min="0">
            </div>
            <div class="form-group">
              <label>Ngân hàng</label>
              <input type="text" [(ngModel)]="formData.bankName">
            </div>
            <div class="form-group">
              <label>Số tài khoản</label>
              <input type="text" [(ngModel)]="formData.accountNumber">
            </div>
            <div class="form-group">
              <label>Nội dung</label>
              <textarea [(ngModel)]="formData.description" rows="2"></textarea>
            </div>
            <div class="form-group">
              <label>Ngày giao dịch</label>
              <input type="datetime-local" [(ngModel)]="formData.transactionDate">
            </div>
          </div>
          <div class="modal-footer">
            <button class="btn btn-secondary" (click)="showForm = false">Hủy</button>
            <button class="btn btn-primary" (click)="save()">Lưu</button>
          </div>
        </div>
      </div>
    </div>
  `,
  styles: [`
    :host { display:block; padding:24px; background:var(--bg-app, #f5f7fb); }
    .page-container { max-width:1280px; margin:0 auto; }

    .page-header { display:flex; align-items:flex-start; justify-content:space-between; gap:16px; margin-bottom:20px; flex-wrap:wrap; }
    .page-header h1 { margin:0 0 4px; font-size:24px; font-weight:700; letter-spacing:-.02em; color:var(--text-primary, #1e293b); }
    .subtitle { margin:0; font-size:13px; color:var(--text-secondary, #64748b); max-width:640px; line-height:1.5; }

    .filter-card {
      background:var(--bg-primary, #fff); border:1px solid var(--border-color, #e2e8f0); border-radius:var(--radius-lg, 12px);
      box-shadow:var(--shadow-sm, 0 1px 2px rgba(15,23,42,.06)); padding:16px 18px; margin-bottom:18px;
    }
    .filter-bar { display:flex; align-items:center; gap:12px; flex-wrap:wrap; }
    .filter-search-wrapper { position:relative; flex:1; min-width:260px; }
    .filter-search-icon { position:absolute; left:12px; top:50%; transform:translateY(-50%); color:var(--text-muted, #94a3b8); pointer-events:none; }
    .filter-search {
      width:100%; padding:9px 12px 9px 36px; border:1px solid var(--border-color, #e2e8f0); border-radius:var(--radius-sm, 6px);
      font-size:14px; box-sizing:border-box; transition:border-color .15s, box-shadow .15s;
    }
    .filter-search:focus { outline:none; border-color:var(--primary-color, #6366f1); box-shadow:0 0 0 3px var(--primary-100, #e0e7ff); }
    .filter-date { display:flex; align-items:center; gap:6px; }
    .filter-date span { font-size:13px; color:var(--text-secondary, #64748b); white-space:nowrap; }
    .filter-date input {
      padding:8px 10px; border:1px solid var(--border-color, #e2e8f0); border-radius:var(--radius-sm, 6px); font-size:14px; box-sizing:border-box;
      transition:border-color .15s, box-shadow .15s;
    }
    .filter-date input:focus { outline:none; border-color:var(--primary-color, #6366f1); box-shadow:0 0 0 3px var(--primary-100, #e0e7ff); }
    .filter-summary { margin-top:12px; padding-top:12px; border-top:1px dashed var(--border-color, #e2e8f0); font-size:13px; color:var(--text-secondary, #64748b); }
    .amount-highlight { color:var(--success-color, #16a34a); font-weight:700; }

    .table-card {
      background:var(--bg-primary, #fff); border:1px solid var(--border-color, #e2e8f0); border-radius:var(--radius-lg, 12px);
      box-shadow:var(--shadow-sm, 0 1px 2px rgba(15,23,42,.06)); overflow:hidden;
    }
    .table { width:100%; border-collapse:collapse; font-size:13.5px; }
    .table th {
      background:var(--gray-50, #f8fafc); font-weight:600; font-size:11px; text-transform:uppercase; letter-spacing:.04em;
      color:var(--text-secondary, #64748b);
    }
    .table th, .table td { padding:13px 16px; border-bottom:1px solid var(--border-color, #e2e8f0); text-align:left; }
    .table tbody tr { transition:background .12s; }
    .table tbody tr:hover { background:var(--gray-50, #f8fafc); }
    .table tr:last-child td { border-bottom:none; }
    .text-right { text-align:right; }
    .cell-date { white-space:nowrap; }
    .cell-time { display:block; font-size:11.5px; color:var(--text-muted, #94a3b8); }
    .cell-desc { max-width:260px; overflow:hidden; text-overflow:ellipsis; white-space:nowrap; }
    .amount-cell { color:var(--success-color, #16a34a); font-weight:700; font-variant-numeric:tabular-nums; white-space:nowrap; }
    .empty-row td { text-align:center; padding:48px 16px; color:var(--text-muted, #94a3b8); }

    .pagination { display:flex; align-items:center; justify-content:center; gap:6px; flex-wrap:wrap; padding:16px; border-top:1px solid var(--border-color, #e2e8f0); }
    .page-btn {
      min-width:34px; padding:6px 10px; border:1px solid var(--border-color, #e2e8f0); background:var(--bg-primary, #fff);
      border-radius:var(--radius-sm, 6px); font-size:13px; cursor:pointer; color:var(--text-secondary, #334155); transition:all .12s;
    }
    .page-btn:hover:not(:disabled) { background:var(--gray-100, #f1f5f9); }
    .page-btn.active { background:var(--primary-color, #6366f1); border-color:var(--primary-color, #6366f1); color:#fff; font-weight:600; }
    .page-btn:disabled { opacity:.45; cursor:default; }
    .page-info { margin-left:8px; font-size:13px; color:var(--text-secondary, #64748b); white-space:nowrap; }
    .page-size-select { display:flex; align-items:center; gap:6px; margin-left:12px; font-size:13px; color:var(--text-secondary, #64748b); }
    .page-size-select select { padding:5px 8px; border:1px solid var(--border-color, #e2e8f0); border-radius:var(--radius-sm, 6px); font-size:13px; }

    .badge { display:inline-flex; align-items:center; gap:5px; padding:3px 9px; border-radius:var(--radius-pill, 999px); font-size:11px; font-weight:600; background:var(--gray-100, #f1f5f9); color:var(--text-secondary, #64748b); }
    .badge-dot { width:5px; height:5px; border-radius:50%; background:currentColor; opacity:.8; }
    .badge.auto { background:var(--info-soft, #dbeafe); color:var(--info-strong, #1e40af); }
    .badge.split { background:var(--warning-soft, #fef3c7); color:var(--warning-strong, #92400e); margin-left:6px; }
    .badge.child { background:var(--success-soft, #dcfce7); color:var(--success-strong, #166534); margin-left:6px; }
    .badge.matched { background:var(--primary-100, #ede9fe); color:var(--accent-600, #5b21b6); margin-left:6px; }
    tr.split-parent td { color:var(--text-muted, #94a3b8); }
    tr.split-parent code { opacity:.6; }

    .actions { white-space:nowrap; text-align:right; }
    .actions .btn-sm { margin-left:6px; }
    .split-info { font-size:14px; margin:0 0 4px; }
    .muted { color:var(--text-muted, #94a3b8); font-size:13px; margin:0 0 14px; }
    .split-row { display:flex; align-items:center; gap:8px; margin-bottom:8px; }
    .split-code { font-family:monospace; font-size:12px; background:var(--gray-100, #f1f5f9); padding:4px 8px; border-radius:4px; white-space:nowrap; }
    .split-row input { flex:1; padding:8px 12px; border:1px solid var(--border-color, #e2e8f0); border-radius:var(--radius-sm, 6px); font-size:14px; }
    .split-summary { margin-top:14px; font-size:13px; padding:10px 12px; border-radius:var(--radius-sm, 6px); font-weight:500; }
    .split-summary.ok { background:var(--success-soft, #f0fdf4); color:var(--success-strong, #166534); }
    .split-summary.bad { background:var(--danger-soft, #fef2f2); color:var(--danger-strong, #b91c1c); }
    .error { color:var(--danger-color, #ef4444); font-size:13px; margin-top:8px; }

    .btn { padding:9px 16px; border:none; border-radius:var(--radius-sm, 6px); cursor:pointer; font-size:14px; font-weight:500; transition:all .12s; }
    .btn-primary { background:var(--primary-color, #6366f1); color:#fff; box-shadow:0 1px 2px rgba(79,70,229,.25); }
    .btn-primary:hover { background:var(--primary-hover, #4f46e5); }
    .btn-secondary { background:var(--gray-100, #e2e8f0); color:var(--text-primary, #1e293b); }
    .btn-secondary:hover { background:var(--gray-200, #cbd5e1); }
    .btn-outline { background:#fff; color:var(--text-secondary, #475569); border:1px solid var(--border-color, #e2e8f0); }
    .btn-outline:hover { background:var(--gray-50, #f8fafc); }
    .btn-danger { background:var(--danger-soft, #fef2f2); color:var(--danger-strong, #dc2626); }
    .btn-danger:hover { background:var(--danger-soft-2, #fee2e2); }
    .btn-sm { padding:5px 11px; font-size:12px; background:var(--gray-100, #f1f5f9); color:var(--text-secondary, #475569); }
    .btn-sm:hover { background:var(--gray-200, #e2e8f0); }
    .btn-sm.btn-danger:hover { background:var(--danger-soft-2, #fee2e2); }
    .btn-close { background:none; border:none; font-size:22px; line-height:1; cursor:pointer; color:var(--text-muted, #94a3b8); padding:2px 6px; border-radius:6px; }
    .btn-close:hover { background:var(--gray-100, #f1f5f9); color:var(--text-primary, #1e293b); }
    code { background:var(--gray-100, #f1f5f9); padding:2px 6px; border-radius:4px; font-size:12.5px; }

    .modal-overlay { position:fixed; inset:0; background:rgba(15,23,42,.5); display:flex; align-items:center; justify-content:center; z-index:1000; backdrop-filter:blur(1px); }
    .modal { background:#fff; border-radius:var(--radius-lg, 12px); max-width:500px; width:90%; max-height:90vh; overflow-y:auto; box-shadow:var(--shadow-lg, 0 20px 40px rgba(15,23,42,.2)); }
    .modal-header { display:flex; justify-content:space-between; align-items:center; padding:18px 22px; border-bottom:1px solid var(--border-color, #e2e8f0); }
    .modal-header h3 { margin:0; font-size:17px; font-weight:700; }
    .modal-body { padding:22px; }
    .modal-footer { display:flex; justify-content:flex-end; gap:8px; padding:16px 22px; border-top:1px solid var(--border-color, #e2e8f0); background:var(--gray-50, #f8fafc); border-radius:0 0 var(--radius-lg, 12px) var(--radius-lg, 12px); }
    .form-group { margin-bottom:16px; }
    .form-group label { display:block; margin-bottom:6px; font-size:13px; color:var(--text-secondary, #475569); font-weight:500; }
    .form-group input, .form-group textarea, .form-group select {
      width:100%; padding:9px 12px; border:1px solid var(--border-color, #e2e8f0); border-radius:var(--radius-sm, 6px); font-size:14px; box-sizing:border-box; transition:border-color .15s, box-shadow .15s;
    }
    .form-group input:focus, .form-group textarea:focus, .form-group select:focus { outline:none; border-color:var(--primary-color, #6366f1); box-shadow:0 0 0 3px var(--primary-100, #e0e7ff); }
    .cb { display:flex; gap:6px; align-items:center; font-size:14px; }
  `]
})
export class DepositsAdminComponent implements OnInit {
  readonly Math = Math;

  deposits: DepositTransaction[] = [];
  showForm = false;
  searchText = '';
  dateFrom = '';
  dateTo = '';

  // Phân trang client-side trên danh sách đã lọc.
  currentPage = 1;
  pageSize = 50;

  get pagedDeposits(): DepositTransaction[] {
    // Clamp để không rơi vào trang rỗng nếu dữ liệu vừa thay đổi (xóa/tách) làm mất trang cuối.
    const page = Math.min(this.currentPage, this.totalPages);
    const start = (page - 1) * this.pageSize;
    return this.filteredDeposits.slice(start, start + this.pageSize);
  }

  get totalPages(): number {
    return Math.max(1, Math.ceil(this.filteredDeposits.length / this.pageSize));
  }

  get pageNumbers(): number[] {
    const total = this.totalPages;
    const start = Math.max(1, this.currentPage - 2);
    const end = Math.min(total, this.currentPage + 2);
    const pages: number[] = [];
    for (let i = start; i <= end; i++) pages.push(i);
    return pages;
  }

  goToPage(page: number): void {
    if (page < 1 || page > this.totalPages) return;
    this.currentPage = page;
  }

  onPageSizeChange(size: number): void {
    this.pageSize = size;
    this.currentPage = 1;
  }

  onFilterChange(): void {
    this.currentPage = 1;
  }

  // Lọc client-side: tìm theo mã GD / nội dung / ngân hàng / số tiền + khoảng ngày giao dịch
  get filteredDeposits(): DepositTransaction[] {
    const term = this.searchText.trim().toLowerCase();
    // Số tiền nhập kiểu "2.862.660" hay "2,862,660" đều khớp — so sánh theo chuỗi chỉ-chữ-số
    const digits = term.replace(/\D/g, '');

    return this.deposits.filter(d => {
      if (term) {
        const textMatch = (d.code || '').toLowerCase().includes(term)
          || (d.description || '').toLowerCase().includes(term)
          || (d.bankName || '').toLowerCase().includes(term);
        const amountMatch = digits.length > 0 && String(d.amount).includes(digits);
        if (!textMatch && !amountMatch) return false;
      }
      if (this.dateFrom || this.dateTo) {
        const localDate = this.toLocalDateString(d.transactionDate);
        if (this.dateFrom && localDate < this.dateFrom) return false;
        if (this.dateTo && localDate > this.dateTo) return false;
      }
      return true;
    });
  }

  get filteredTotal(): number {
    return this.filteredDeposits.reduce((sum, d) => sum + (d.amount || 0), 0);
  }

  clearFilters(): void {
    this.searchText = '';
    this.dateFrom = '';
    this.dateTo = '';
    this.currentPage = 1;
  }

  // yyyy-MM-dd theo giờ địa phương — khớp với ngày hiển thị trên bảng
  private toLocalDateString(value: string | Date): string {
    const t = new Date(value);
    const m = String(t.getMonth() + 1).padStart(2, '0');
    const day = String(t.getDate()).padStart(2, '0');
    return `${t.getFullYear()}-${m}-${day}`;
  }
  formData: any = { code: '', amount: 0, bankName: '', accountNumber: '', description: '', transactionDate: new Date().toISOString().slice(0, 16) };

  constructor(private settings: SettingsService) {}

  ngOnInit(): void { this.load(); }

  load(): void {
    this.settings.getDeposits().subscribe(d => this.deposits = d);
  }
  save(): void {
    if (!this.formData.code || !this.formData.amount) return;
    this.settings.createDeposit(this.formData).subscribe(() => {
      this.showForm = false;
      this.formData = { code: '', amount: 0, bankName: '', accountNumber: '', description: '', transactionDate: new Date().toISOString().slice(0, 16) };
      this.load();
    });
  }
  remove(d: DepositTransaction): void {
    if (!confirm('Xóa giao dịch này?')) return;
    this.settings.deleteDeposit(d.id).subscribe({
      next: () => this.load(),
      error: err => alert(err?.error?.message || 'Xóa thất bại.')
    });
  }

  // ===== Tách giao dịch gộp =====
  splitting: DepositTransaction | null = null;
  splitParts: number[] = [];
  splitError = '';
  splitBusy = false;

  get splitTotal(): number {
    return this.splitParts.reduce((sum, v) => sum + (Number(v) || 0), 0);
  }

  get splitRemainder(): number {
    return (this.splitting?.amount || 0) - this.splitTotal;
  }

  trackByIndex(index: number): number { return index; }

  openSplit(d: DepositTransaction): void {
    this.splitting = d;
    this.splitParts = [0, 0];
    this.splitError = '';
    this.splitBusy = false;
  }

  closeSplit(): void { this.splitting = null; }

  removeSplitPart(i: number): void {
    if (this.splitParts.length <= 2) return;
    this.splitParts.splice(i, 1);
  }

  confirmSplit(): void {
    if (!this.splitting || this.splitRemainder !== 0 || this.splitBusy) return;
    const amounts = this.splitParts.map(v => Number(v) || 0);
    if (amounts.some(a => a <= 0)) {
      this.splitError = 'Số tiền mỗi khoản phải lớn hơn 0.';
      return;
    }
    this.splitBusy = true;
    this.splitError = '';
    this.settings.splitDeposit(this.splitting.id, amounts).subscribe({
      next: () => { this.splitBusy = false; this.splitting = null; this.load(); },
      error: err => {
        this.splitBusy = false;
        this.splitError = err?.error?.message || 'Tách giao dịch thất bại.';
      }
    });
  }
}
