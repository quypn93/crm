import { Component, OnInit } from '@angular/core';
import { SettingsService } from '../../../core/services/settings.service';
import { DepositTransaction } from '../../../core/models/lookup.model';

@Component({
  selector: 'app-deposits-admin',
  template: `
    <div class="page-container">
      <div class="page-header">
        <h1>Lịch sử cộng tiền</h1>
        <button class="btn btn-primary" (click)="showForm = true">+ Thêm thủ công</button>
      </div>

      <p style="color:#64748b;font-size:13px;">
        Giao dịch từ Casso webhook sẽ tự động xuất hiện ở đây. Sale có thể nhìn vào danh sách này để biết mã giao dịch nào là của mình và điền vào đơn hàng.
      </p>

      <div class="filter-bar">
        <input type="text" class="filter-search" [(ngModel)]="searchText"
               placeholder="Tìm mã GD, nội dung, ngân hàng, số tiền...">
        <label class="filter-date">
          <span>Từ ngày</span>
          <input type="date" [(ngModel)]="dateFrom">
        </label>
        <label class="filter-date">
          <span>Đến ngày</span>
          <input type="date" [(ngModel)]="dateTo">
        </label>
        <button class="btn btn-secondary btn-sm" *ngIf="searchText || dateFrom || dateTo" (click)="clearFilters()">Xóa lọc</button>
      </div>

      <div class="filter-summary" *ngIf="searchText || dateFrom || dateTo">
        Tìm thấy <strong>{{ filteredDeposits.length }}</strong> giao dịch —
        tổng <strong>{{ filteredTotal | number }} đ</strong>
      </div>

      <table class="table">
        <thead>
          <tr>
            <th>Ngày</th>
            <th>Mã GD</th>
            <th>Số tiền</th>
            <th>Ngân hàng</th>
            <th>Nội dung</th>
            <th>Nguồn</th>
            <th></th>
          </tr>
        </thead>
        <tbody>
          <tr *ngIf="filteredDeposits.length === 0">
            <td colspan="7" style="text-align:center;color:#94a3b8;">Không có giao dịch nào khớp bộ lọc.</td>
          </tr>
          <tr *ngFor="let d of filteredDeposits">
            <td>{{ d.transactionDate | date:'dd/MM/yyyy HH:mm' }}</td>
            <td><code>{{ d.code }}</code></td>
            <td style="text-align:right;color:#16a34a;font-weight:600;">{{ d.amount | number }} đ</td>
            <td>{{ d.bankName }}</td>
            <td>{{ d.description }}</td>
            <td><span class="badge" [class.auto]="d.source==='casso'">{{ d.source }}</span></td>
            <td>
              <button class="btn btn-sm btn-danger" (click)="remove(d)">Xóa</button>
            </td>
          </tr>
        </tbody>
      </table>

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
    :host { display:block; padding:24px; }
    .page-container { max-width:1200px; margin:0 auto; }
    .page-header { display:flex; align-items:center; justify-content:space-between; gap:16px; margin-bottom:16px; }
    .page-header h1 { margin:0; font-size:22px; font-weight:600; }
    .filter-bar { display:flex; align-items:flex-end; gap:12px; flex-wrap:wrap; margin-bottom:12px; }
    .filter-search { flex:1; min-width:240px; padding:8px 12px; border:1px solid #cbd5e1; border-radius:6px; font-size:14px; }
    .filter-date { display:flex; flex-direction:column; gap:4px; }
    .filter-date span { font-size:12px; color:#64748b; }
    .filter-date input { padding:7px 10px; border:1px solid #cbd5e1; border-radius:6px; font-size:13px; }
    .filter-summary { margin-bottom:10px; font-size:13px; color:#475569; }
    .filter-summary strong { color:#16a34a; }
    .table { width:100%; border-collapse:collapse; font-size:14px; background:#fff; border-radius:8px; overflow:hidden; box-shadow:0 1px 3px rgba(0,0,0,.06); }
    .table th { background:#f8fafc; font-weight:600; color:#475569; }
    .table th, .table td { padding:12px; border-bottom:1px solid #e2e8f0; text-align:left; }
    .table tr:last-child td { border-bottom:none; }
    .badge { padding:2px 8px; border-radius:10px; font-size:11px; background:#e2e8f0; }
    .badge.auto { background:#dbeafe; color:#1e40af; }
    .btn { padding:8px 16px; border:none; border-radius:6px; cursor:pointer; font-size:14px; }
    .btn-primary { background:#6366f1; color:#fff; }
    .btn-secondary { background:#e2e8f0; color:#1e293b; }
    .btn-danger { background:#ef4444; color:#fff; }
    .btn-sm { padding:4px 10px; font-size:12px; }
    .btn-close { background:none; border:none; font-size:24px; cursor:pointer; color:#64748b; }
    code { background:#f1f5f9; padding:2px 6px; border-radius:4px; }
    .modal-overlay { position:fixed; inset:0; background:rgba(0,0,0,.5); display:flex; align-items:center; justify-content:center; z-index:1000; }
    .modal { background:#fff; border-radius:8px; max-width:500px; width:90%; max-height:90vh; overflow-y:auto; }
    .modal-header { display:flex; justify-content:space-between; align-items:center; padding:16px 20px; border-bottom:1px solid #e2e8f0; }
    .modal-header h3 { margin:0; font-size:18px; }
    .modal-body { padding:20px; }
    .modal-footer { display:flex; justify-content:flex-end; gap:8px; padding:16px 20px; border-top:1px solid #e2e8f0; }
    .form-group { margin-bottom:16px; }
    .form-group label { display:block; margin-bottom:6px; font-size:13px; color:#475569; font-weight:500; }
    .form-group input, .form-group textarea, .form-group select { width:100%; padding:8px 12px; border:1px solid #cbd5e1; border-radius:6px; font-size:14px; }
    .cb { display:flex; gap:6px; align-items:center; font-size:14px; }
  `]
})
export class DepositsAdminComponent implements OnInit {
  deposits: DepositTransaction[] = [];
  showForm = false;
  searchText = '';
  dateFrom = '';
  dateTo = '';

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
    this.settings.deleteDeposit(d.id).subscribe(() => this.load());
  }
}
