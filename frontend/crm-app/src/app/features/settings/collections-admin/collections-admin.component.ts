import { Component, OnInit } from '@angular/core';
import { forkJoin } from 'rxjs';
import { SettingsService } from '../../../core/services/settings.service';
import { DesignService, ColorFabric } from '../../../core/services/design.service';
import { Collection, LookupItem } from '../../../core/models/lookup.model';

@Component({
  selector: 'app-collections-admin',
  template: `
    <div class="page-container">
      <div class="page-header">
        <h1>Quản lý Bộ sưu tập</h1>
        <button class="btn btn-primary" (click)="openNew()">+ Thêm bộ sưu tập</button>
      </div>

      <table class="table" *ngIf="collections.length">
        <thead><tr><th>Tên</th><th>Mô tả</th><th>Trạng thái</th><th></th></tr></thead>
        <tbody>
          <tr *ngFor="let c of collections">
            <td>{{ c.name }}</td>
            <td>{{ c.description }}</td>
            <td>{{ c.isActive ? 'Đang dùng' : 'Tắt' }}</td>
            <td>
              <button class="btn btn-sm" (click)="edit(c)">Sửa</button>
              <button class="btn btn-sm btn-danger" (click)="remove(c)">Xóa</button>
            </td>
          </tr>
        </tbody>
      </table>

      <div *ngIf="!collections.length" style="padding:24px;text-align:center;color:#64748b;">Chưa có bộ sưu tập nào.</div>

      <!-- Modal -->
      <div class="modal-overlay" *ngIf="showForm" (click)="cancel()">
        <div class="modal" (click)="$event.stopPropagation()" style="max-width:680px;">
          <div class="modal-header">
            <h3>{{ editing ? 'Sửa' : 'Thêm' }} bộ sưu tập</h3>
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
            <div class="form-group">
              <label>Chất liệu</label>
              <div class="checkbox-grid">
                <label *ngFor="let m of materials" class="cb">
                  <input type="checkbox" [checked]="formData.materialIds.includes(m.id)" (change)="toggle('materialIds', m.id)">
                  {{ m.name }}
                </label>
              </div>
            </div>
            <div class="form-group">
              <label>Màu</label>
              <div class="checkbox-grid">
                <label *ngFor="let c of colors" class="cb">
                  <input type="checkbox" [checked]="formData.colorFabricIds.includes(c.id)" (change)="toggle('colorFabricIds', c.id)">
                  {{ c.name }}
                </label>
              </div>
            </div>
            <div class="form-group">
              <label>Form</label>
              <div class="checkbox-grid">
                <label *ngFor="let f of forms" class="cb">
                  <input type="checkbox" [checked]="formData.formIds.includes(f.id)" (change)="toggle('formIds', f.id)">
                  {{ f.name }}
                </label>
              </div>
            </div>
            <div class="form-group">
              <label>Quy cách</label>
              <div class="checkbox-grid">
                <label *ngFor="let s of specs" class="cb">
                  <input type="checkbox" [checked]="formData.specificationIds.includes(s.id)" (change)="toggle('specificationIds', s.id)">
                  {{ s.name }}
                </label>
              </div>
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
    .checkbox-grid { display:grid; grid-template-columns:repeat(3,1fr); gap:8px; max-height:160px; overflow-y:auto; padding:8px; border:1px solid #e2e8f0; border-radius:6px; }
    .cb { display:flex; gap:6px; align-items:center; font-size:14px; }
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
    .modal-overlay { position:fixed; inset:0; background:rgba(0,0,0,.5); display:flex; align-items:center; justify-content:center; z-index:1000; }
    .modal { background:#fff; border-radius:8px; max-width:680px; width:90%; max-height:90vh; overflow-y:auto; }
    .modal-header { display:flex; justify-content:space-between; align-items:center; padding:16px 20px; border-bottom:1px solid #e2e8f0; }
    .modal-header h3 { margin:0; font-size:18px; }
    .modal-body { padding:20px; }
    .modal-footer { display:flex; justify-content:flex-end; gap:8px; padding:16px 20px; border-top:1px solid #e2e8f0; }
    .form-group { margin-bottom:16px; }
    .form-group label { display:block; margin-bottom:6px; font-size:13px; color:#475569; font-weight:500; }
    .form-group input, .form-group textarea, .form-group select { width:100%; padding:8px 12px; border:1px solid #cbd5e1; border-radius:6px; font-size:14px; }
  `]
})
export class CollectionsAdminComponent implements OnInit {
  collections: Collection[] = [];
  materials: LookupItem[] = [];
  forms: LookupItem[] = [];
  specs: LookupItem[] = [];
  colors: ColorFabric[] = [];

  showForm = false;
  editing: Collection | null = null;
  formData: any = this.emptyForm();

  constructor(private settings: SettingsService, private designs: DesignService) {}

  ngOnInit(): void { this.loadAll(); }

  loadAll(): void {
    forkJoin({
      collections: this.settings.getCollections(),
      materials: this.settings.getLookups('materials'),
      forms: this.settings.getLookups('product-forms'),
      specs: this.settings.getLookups('product-specifications'),
      colors: this.designs.getAllColorFabrics()
    }).subscribe(res => {
      this.collections = res.collections;
      this.materials = res.materials;
      this.forms = res.forms;
      this.specs = res.specs;
      this.colors = res.colors;
    });
  }

  emptyForm() {
    return {
      name: '', description: '', isActive: true,
      materialIds: [] as string[], colorFabricIds: [] as string[],
      formIds: [] as string[], specificationIds: [] as string[]
    };
  }

  openNew(): void { this.editing = null; this.formData = this.emptyForm(); this.showForm = true; }
  edit(c: Collection): void {
    this.editing = c;
    this.formData = {
      name: c.name, description: c.description || '', isActive: c.isActive,
      materialIds: [...c.materialIds], colorFabricIds: [...c.colorFabricIds],
      formIds: [...c.formIds], specificationIds: [...c.specificationIds]
    };
    this.showForm = true;
  }
  cancel(): void { this.showForm = false; }
  toggle(key: string, id: string): void {
    const arr: string[] = this.formData[key];
    const i = arr.indexOf(id);
    if (i >= 0) arr.splice(i, 1); else arr.push(id);
  }
  save(): void {
    if (!this.formData.name) return;
    const obs = this.editing
      ? this.settings.updateCollection(this.editing.id, { ...this.formData, id: this.editing.id })
      : this.settings.createCollection(this.formData);
    obs.subscribe(() => { this.showForm = false; this.loadAll(); });
  }
  remove(c: Collection): void {
    if (!confirm(`Xóa bộ sưu tập "${c.name}"?`)) return;
    this.settings.deleteCollection(c.id).subscribe(() => this.loadAll());
  }
}
