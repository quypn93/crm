import { Component, OnInit } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { SettingsService } from '../../../core/services/settings.service';
import { LookupItem } from '../../../core/models/lookup.model';

@Component({
  selector: 'app-lookups-admin',
  template: `
    <div class="page-container">
      <div class="page-header">
        <h1>{{ title }}</h1>
        <button class="btn btn-primary" (click)="openNew()">+ Thêm</button>
      </div>

      <table class="table">
        <thead><tr><th>Tên</th><th>Mô tả</th><th>Trạng thái</th><th></th></tr></thead>
        <tbody>
          <tr *ngFor="let m of items">
            <td>{{ m.name }}</td>
            <td>{{ m.description }}</td>
            <td>{{ m.isActive ? 'Đang dùng' : 'Tắt' }}</td>
            <td>
              <button class="btn btn-sm" (click)="edit(m)">Sửa</button>
              <button class="btn btn-sm btn-danger" (click)="remove(m)">Xóa</button>
            </td>
          </tr>
        </tbody>
      </table>

      <div class="modal-overlay" *ngIf="showForm" (click)="cancel()">
        <div class="modal" (click)="$event.stopPropagation()">
          <div class="modal-header">
            <h3>{{ editing ? 'Sửa' : 'Thêm' }}</h3>
            <button class="btn-close" (click)="cancel()">×</button>
          </div>
          <div class="modal-body">
            <div class="form-group">
              <label>Tên *</label>
              <input type="text" [(ngModel)]="formData.name">
            </div>
            <div class="form-group">
              <label>Mô tả</label>
              <textarea [(ngModel)]="formData.description" rows="2"></textarea>
            </div>
            <label class="cb">
              <input type="checkbox" [(ngModel)]="formData.isActive">
              Đang dùng
            </label>
          </div>
          <div class="modal-footer">
            <button class="btn btn-secondary" (click)="cancel()">Hủy</button>
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
    .table { width:100%; border-collapse:collapse; background:#fff; border-radius:8px; overflow:hidden; box-shadow:0 1px 3px rgba(0,0,0,.06); }
    .table th { background:#f8fafc; font-weight:600; color:#475569; }
    .table th, .table td { padding:12px; border-bottom:1px solid #e2e8f0; text-align:left; }
    .table tr:last-child td { border-bottom:none; }
    .btn { padding:8px 16px; border:none; border-radius:6px; cursor:pointer; font-size:14px; }
    .btn-primary { background:#6366f1; color:#fff; }
    .btn-secondary { background:#e2e8f0; color:#1e293b; }
    .btn-danger { background:#ef4444; color:#fff; }
    .btn-sm { padding:4px 10px; font-size:12px; margin-right:4px; }
    .btn-close { background:none; border:none; font-size:24px; cursor:pointer; color:#64748b; }
    .cb { display:flex; gap:6px; align-items:center; font-size:14px; }
    .modal-overlay { position:fixed; inset:0; background:rgba(0,0,0,.5); display:flex; align-items:center; justify-content:center; z-index:1000; }
    .modal { background:#fff; border-radius:8px; max-width:500px; width:90%; max-height:90vh; overflow-y:auto; }
    .modal-header { display:flex; justify-content:space-between; align-items:center; padding:16px 20px; border-bottom:1px solid #e2e8f0; }
    .modal-header h3 { margin:0; font-size:18px; }
    .modal-body { padding:20px; }
    .modal-footer { display:flex; justify-content:flex-end; gap:8px; padding:16px 20px; border-top:1px solid #e2e8f0; }
    .form-group { margin-bottom:16px; }
    .form-group label { display:block; margin-bottom:6px; font-size:13px; color:#475569; font-weight:500; }
    .form-group input, .form-group textarea, .form-group select { width:100%; padding:8px 12px; border:1px solid #cbd5e1; border-radius:6px; font-size:14px; }
  `]
})
export class LookupsAdminComponent implements OnInit {
  resource: 'materials' | 'product-forms' | 'product-specifications' = 'materials';
  title = 'Chất liệu';
  items: LookupItem[] = [];
  showForm = false;
  editing: LookupItem | null = null;
  formData: any = { name: '', description: '', isActive: true };

  constructor(private route: ActivatedRoute, private settings: SettingsService) {}

  ngOnInit(): void {
    this.route.data.subscribe(d => {
      this.resource = d['resource'];
      this.title = d['title'];
      this.load();
    });
  }

  load(): void {
    this.settings.getLookups(this.resource).subscribe(i => this.items = i);
  }
  openNew(): void { this.editing = null; this.formData = { name: '', description: '', isActive: true }; this.showForm = true; }
  edit(m: LookupItem): void { this.editing = m; this.formData = { name: m.name, description: m.description || '', isActive: m.isActive }; this.showForm = true; }
  cancel(): void { this.showForm = false; }
  save(): void {
    const obs = this.editing
      ? this.settings.updateLookup(this.resource, this.editing.id, { ...this.formData, id: this.editing.id })
      : this.settings.createLookup(this.resource, this.formData);
    obs.subscribe(() => { this.showForm = false; this.load(); });
  }
  remove(m: LookupItem): void {
    if (!confirm(`Xóa "${m.name}"?`)) return;
    this.settings.deleteLookup(this.resource, m.id).subscribe(() => this.load());
  }
}
