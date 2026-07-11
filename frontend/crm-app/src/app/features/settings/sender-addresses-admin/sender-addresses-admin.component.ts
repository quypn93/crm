import { Component, OnInit } from '@angular/core';
import { SettingsService } from '../../../core/services/settings.service';
import { SenderAddress, VtpCategory } from '../../../core/models/lookup.model';

@Component({
  selector: 'app-sender-addresses-admin',
  template: `
  <div class="page">
    <div class="page-header">
      <div>
        <h1>Địa chỉ gửi hàng</h1>
        <p class="subtitle">Kho gửi dùng khi tạo vận đơn Viettel Post. Đặt 1 địa chỉ mặc định.</p>
      </div>
      <button class="btn btn-primary" (click)="openNew()">+ Thêm địa chỉ</button>
    </div>

    <div class="loading" *ngIf="isLoading">Đang tải...</div>

    <table class="data-table" *ngIf="!isLoading">
      <thead>
        <tr><th>Tên</th><th>SĐT</th><th>Địa chỉ</th><th>Tỉnh/Huyện/Xã</th><th>Mặc định</th><th>Trạng thái</th><th></th></tr>
      </thead>
      <tbody>
        <tr *ngFor="let a of items">
          <td>{{ a.name }}</td>
          <td>{{ a.phone }}</td>
          <td>{{ a.address }}</td>
          <td>{{ [a.wardName, a.districtName, a.provinceName] | json }}</td>
          <td>{{ a.isDefault ? '✓' : '' }}</td>
          <td>{{ a.isActive ? 'Hoạt động' : 'Tắt' }}</td>
          <td class="actions">
            <button class="btn-icon" (click)="edit(a)">✏️</button>
            <button class="btn-icon btn-danger" (click)="remove(a)">🗑️</button>
          </td>
        </tr>
        <tr *ngIf="items.length === 0"><td colspan="7" class="no-data">Chưa có địa chỉ gửi hàng</td></tr>
      </tbody>
    </table>

    <div class="modal-overlay" *ngIf="showForm" (click)="cancel()">
      <div class="modal" (click)="$event.stopPropagation()">
        <h2>{{ editing ? 'Sửa địa chỉ gửi' : 'Thêm địa chỉ gửi' }}</h2>
        <div class="form-group">
          <label>Tên người/kho gửi *</label>
          <input type="text" [(ngModel)]="formData.name">
        </div>
        <div class="form-group">
          <label>Số điện thoại *</label>
          <input type="text" [(ngModel)]="formData.phone">
        </div>
        <div class="form-group">
          <label>Địa chỉ (số nhà, đường...) *</label>
          <input type="text" [(ngModel)]="formData.address">
        </div>
        <div class="form-group">
          <label>Tỉnh/Thành *</label>
          <select [(ngModel)]="formData.provinceId" (ngModelChange)="onProvinceChange()">
            <option [ngValue]="0">-- Chọn tỉnh --</option>
            <option *ngFor="let p of provinces" [ngValue]="p.PROVINCE_ID">{{ p.PROVINCE_NAME }}</option>
          </select>
        </div>
        <div class="form-group">
          <label>Quận/Huyện *</label>
          <select [(ngModel)]="formData.districtId" (ngModelChange)="onDistrictChange()" [disabled]="!formData.provinceId">
            <option [ngValue]="0">-- Chọn quận/huyện --</option>
            <option *ngFor="let d of districts" [ngValue]="d.DISTRICT_ID">{{ d.DISTRICT_NAME }}</option>
          </select>
        </div>
        <div class="form-group">
          <label>Phường/Xã *</label>
          <select [(ngModel)]="formData.wardId" [disabled]="!formData.districtId">
            <option [ngValue]="0">-- Chọn phường/xã --</option>
            <option *ngFor="let w of wards" [ngValue]="w.WARDS_ID">{{ w.WARDS_NAME }}</option>
          </select>
        </div>
        <div class="form-row">
          <label class="chk"><input type="checkbox" [(ngModel)]="formData.isDefault"> Đặt mặc định</label>
          <label class="chk"><input type="checkbox" [(ngModel)]="formData.isActive"> Hoạt động</label>
        </div>
        <div class="err" *ngIf="error">{{ error }}</div>
        <div class="modal-actions">
          <button class="btn btn-secondary" (click)="cancel()">Hủy</button>
          <button class="btn btn-primary" (click)="save()" [disabled]="saving">{{ editing ? 'Cập nhật' : 'Thêm' }}</button>
        </div>
      </div>
    </div>
  </div>
  `,
  styles: [`
    .page { padding: 20px; }
    .page-header { display:flex; justify-content:space-between; align-items:flex-start; margin-bottom:16px; }
    .subtitle { color:#6b7280; font-size:13px; margin:4px 0 0; }
    .data-table { width:100%; border-collapse:collapse; background:#fff; }
    .data-table th, .data-table td { padding:10px 12px; border-bottom:1px solid #eee; text-align:left; font-size:14px; }
    .no-data { text-align:center; color:#9ca3af; }
    .actions { display:flex; gap:6px; }
    .btn-icon { background:none; border:none; cursor:pointer; font-size:16px; }
    .btn { padding:8px 16px; border:none; border-radius:6px; cursor:pointer; }
    .btn-primary { background:#6366f1; color:#fff; }
    .btn-secondary { background:#e5e7eb; }
    .btn-danger { color:#dc2626; }
    .modal-overlay { position:fixed; inset:0; background:rgba(0,0,0,.4); display:flex; align-items:center; justify-content:center; z-index:1000; }
    .modal { background:#fff; padding:24px; border-radius:10px; width:460px; max-width:92vw; max-height:90vh; overflow:auto; }
    .form-group { margin-bottom:12px; display:flex; flex-direction:column; gap:4px; }
    .form-group input, .form-group select { padding:8px; border:1px solid #d1d5db; border-radius:6px; }
    .form-row { display:flex; gap:20px; margin:8px 0 12px; }
    .chk { display:flex; align-items:center; gap:6px; }
    .err { color:#dc2626; font-size:13px; margin-bottom:8px; }
    .modal-actions { display:flex; justify-content:flex-end; gap:8px; margin-top:8px; }
  `]
})
export class SenderAddressesAdminComponent implements OnInit {
  items: SenderAddress[] = [];
  provinces: VtpCategory[] = [];
  districts: VtpCategory[] = [];
  wards: VtpCategory[] = [];
  isLoading = false;
  showForm = false;
  saving = false;
  editing: SenderAddress | null = null;
  error = '';
  formData = this.empty();

  constructor(private settings: SettingsService) {}

  ngOnInit(): void {
    this.load();
    this.settings.getVtpProvinces().subscribe({ next: p => this.provinces = p || [], error: () => this.provinces = [] });
  }

  private empty() {
    return { name: '', phone: '', address: '', provinceId: 0, districtId: 0, wardId: 0,
      provinceName: '', districtName: '', wardName: '', isDefault: false, isActive: true };
  }

  load(): void {
    this.isLoading = true;
    this.settings.getSenderAddresses().subscribe({
      next: a => { this.items = a || []; this.isLoading = false; },
      error: () => { this.items = []; this.isLoading = false; }
    });
  }

  openNew(): void {
    this.editing = null;
    this.formData = this.empty();
    this.districts = []; this.wards = [];
    this.error = '';
    this.showForm = true;
  }

  edit(a: SenderAddress): void {
    this.editing = a;
    this.formData = { ...this.empty(), ...a };
    this.error = '';
    this.showForm = true;
    // Nạp lại danh sách huyện/xã để hiển thị đúng lựa chọn đang lưu.
    if (a.provinceId) this.settings.getVtpDistricts(a.provinceId).subscribe(d => this.districts = d || []);
    if (a.districtId) this.settings.getVtpWards(a.districtId).subscribe(w => this.wards = w || []);
  }

  onProvinceChange(): void {
    this.formData.districtId = 0; this.formData.wardId = 0;
    this.districts = []; this.wards = [];
    if (this.formData.provinceId) {
      this.settings.getVtpDistricts(this.formData.provinceId).subscribe(d => this.districts = d || []);
    }
  }

  onDistrictChange(): void {
    this.formData.wardId = 0;
    this.wards = [];
    if (this.formData.districtId) {
      this.settings.getVtpWards(this.formData.districtId).subscribe(w => this.wards = w || []);
    }
  }

  save(): void {
    if (!this.formData.name.trim() || !this.formData.phone.trim() || !this.formData.address.trim()) {
      this.error = 'Nhập đủ tên, số điện thoại và địa chỉ.'; return;
    }
    if (!this.formData.provinceId || !this.formData.districtId || !this.formData.wardId) {
      this.error = 'Chọn đủ Tỉnh / Quận-Huyện / Phường-Xã.'; return;
    }
    // Gắn tên hành chính từ danh mục đang chọn.
    this.formData.provinceName = this.provinces.find(p => p.PROVINCE_ID === this.formData.provinceId)?.PROVINCE_NAME || '';
    this.formData.districtName = this.districts.find(d => d.DISTRICT_ID === this.formData.districtId)?.DISTRICT_NAME || '';
    this.formData.wardName = this.wards.find(w => w.WARDS_ID === this.formData.wardId)?.WARDS_NAME || '';

    this.saving = true;
    this.error = '';
    const obs = this.editing
      ? this.settings.updateSenderAddress(this.editing.id, { ...this.formData, id: this.editing.id })
      : this.settings.createSenderAddress(this.formData);
    obs.subscribe({
      next: () => { this.saving = false; this.showForm = false; this.load(); },
      error: (err) => { this.saving = false; this.error = err?.error?.message || 'Lưu thất bại.'; }
    });
  }

  remove(a: SenderAddress): void {
    if (!confirm(`Xóa địa chỉ gửi "${a.name}"?`)) return;
    this.settings.deleteSenderAddress(a.id).subscribe({ next: () => this.load() });
  }

  cancel(): void { this.showForm = false; }
}
