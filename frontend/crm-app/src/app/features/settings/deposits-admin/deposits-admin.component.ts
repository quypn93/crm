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
        Giao dịch từ SePay webhook sẽ tự động xuất hiện ở đây. Sale có thể nhìn vào danh sách này để biết mã giao dịch nào là của mình và điền vào đơn hàng.
      </p>

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
          <tr *ngFor="let d of deposits">
            <td>{{ d.transactionDate | date:'dd/MM/yyyy HH:mm' }}</td>
            <td><code>{{ d.code }}</code></td>
            <td style="text-align:right;color:#16a34a;font-weight:600;">{{ d.amount | number }} đ</td>
            <td>{{ d.bankName }}</td>
            <td>{{ d.description }}</td>
            <td><span class="badge" [class.auto]="d.source==='sepay'">{{ d.source }}</span></td>
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
