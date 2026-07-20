import { Component, OnInit } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { SettingsService } from '../../../core/services/settings.service';
import { LookupItem } from '../../../core/models/lookup.model';
import { ColorFabric, DesignService } from '../../../core/services/design.service';

@Component({
  selector: 'app-lookups-admin',
  template: `
    <div class="page-container">
      <div class="page-header">
        <h1>{{ title }}</h1>
        <button class="btn btn-primary" (click)="openNew()">+ Thêm</button>
      </div>

      <table class="table">
        <thead><tr><th>Tên</th><th>Mô tả</th><th *ngIf="isMaterials">Màu sắc</th><th>Trạng thái</th><th></th></tr></thead>
        <tbody>
          <tr *ngFor="let m of items">
            <td>{{ m.name }}</td>
            <td>{{ m.description }}</td>
            <td *ngIf="isMaterials">
              <span class="color-chip" *ngFor="let c of colorsOf(m.id)">{{ c.name }}</span>
              <span class="muted" *ngIf="!colorsOf(m.id).length">—</span>
            </td>
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

            <!-- Quản lý màu sắc của chất liệu (chỉ resource materials) -->
            <div class="colors-section" *ngIf="isMaterials">
              <label>Màu sắc của chất liệu</label>
              <p class="muted" *ngIf="!editing">Lưu chất liệu xong sẽ thêm được màu ngay tại đây.</p>
              <ng-container *ngIf="editing">
                <div class="color-row" *ngFor="let c of colors">
                  <ng-container *ngIf="editingColorId !== c.id; else editColorTpl">
                    <span class="color-name">{{ c.name }}</span>
                    <button class="btn btn-sm" (click)="startEditColor(c)">Sửa</button>
                    <button class="btn btn-sm btn-danger" (click)="removeColor(c)">Xóa</button>
                  </ng-container>
                  <ng-template #editColorTpl>
                    <input type="text" [(ngModel)]="editingColorName" (keyup.enter)="saveEditColor(c)">
                    <button class="btn btn-sm btn-primary" (click)="saveEditColor(c)">Lưu</button>
                    <button class="btn btn-sm" (click)="editingColorId = null">Hủy</button>
                  </ng-template>
                </div>
                <p class="muted" *ngIf="!colors.length">Chưa có màu nào.</p>
                <div class="color-row add-row">
                  <input type="text" placeholder="Tên màu mới..." [(ngModel)]="newColorName" (keyup.enter)="addColor()">
                  <button class="btn btn-sm btn-primary" (click)="addColor()" [disabled]="!newColorName.trim()">+ Thêm màu</button>
                </div>
              </ng-container>
              <p class="error" *ngIf="colorError">{{ colorError }}</p>
            </div>
          </div>
          <div class="modal-footer">
            <button class="btn btn-secondary" (click)="cancel()">Đóng</button>
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
    .btn-primary:disabled { opacity:.5; cursor:default; }
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
    .colors-section { margin-top:20px; padding-top:16px; border-top:1px solid #e2e8f0; }
    .colors-section > label { display:block; margin-bottom:8px; font-size:13px; color:#475569; font-weight:600; }
    .color-row { display:flex; align-items:center; gap:6px; margin-bottom:6px; }
    .color-row .color-name { flex:1; font-size:14px; }
    .color-row input { flex:1; padding:6px 10px; border:1px solid #cbd5e1; border-radius:6px; font-size:14px; }
    .color-row .btn-sm { margin-right:0; white-space:nowrap; }
    .add-row { margin-top:10px; }
    .color-chip { display:inline-block; padding:2px 8px; margin:2px 4px 2px 0; background:#eef2ff; color:#4338ca; border-radius:999px; font-size:12px; }
    .muted { color:#94a3b8; font-size:13px; }
    .error { color:#ef4444; font-size:13px; margin-top:8px; }
  `]
})
export class LookupsAdminComponent implements OnInit {
  resource: 'materials' | 'product-forms' | 'product-specifications' | 'order-types' = 'materials';
  title = 'Chất liệu';
  items: LookupItem[] = [];
  showForm = false;
  editing: LookupItem | null = null;
  formData: any = { name: '', description: '', isActive: true };

  // Màu sắc theo chất liệu (chỉ dùng khi resource === 'materials')
  allColors: ColorFabric[] = [];
  colors: ColorFabric[] = [];
  newColorName = '';
  editingColorId: string | null = null;
  editingColorName = '';
  colorError = '';

  constructor(
    private route: ActivatedRoute,
    private settings: SettingsService,
    private designService: DesignService
  ) {}

  get isMaterials(): boolean { return this.resource === 'materials'; }

  ngOnInit(): void {
    this.route.data.subscribe(d => {
      this.resource = d['resource'];
      this.title = d['title'];
      this.load();
    });
  }

  load(): void {
    this.settings.getLookups(this.resource).subscribe(i => this.items = i);
    if (this.isMaterials) this.loadAllColors();
  }

  loadAllColors(): void {
    this.designService.getAllColorFabrics().subscribe(cs => {
      this.allColors = cs;
      if (this.editing) this.colors = this.colorsOf(this.editing.id);
    });
  }

  colorsOf(materialId: string): ColorFabric[] {
    return this.allColors.filter(c => c.materialId === materialId);
  }

  openNew(): void {
    this.editing = null;
    this.formData = { name: '', description: '', isActive: true };
    this.colors = [];
    this.resetColorInputs();
    this.showForm = true;
  }

  edit(m: LookupItem): void {
    this.editing = m;
    this.formData = { name: m.name, description: m.description || '', isActive: m.isActive };
    this.colors = this.colorsOf(m.id);
    this.resetColorInputs();
    this.showForm = true;
  }

  cancel(): void { this.showForm = false; }

  save(): void {
    if (this.editing) {
      this.settings.updateLookup(this.resource, this.editing.id, { ...this.formData, id: this.editing.id })
        .subscribe(() => { this.showForm = false; this.load(); });
      return;
    }
    this.settings.createLookup(this.resource, this.formData).subscribe(created => {
      this.load();
      if (this.isMaterials) {
        // Giữ modal mở ở chế độ sửa để thêm màu ngay cho chất liệu vừa tạo.
        this.editing = created;
        this.colors = [];
      } else {
        this.showForm = false;
      }
    });
  }

  remove(m: LookupItem): void {
    if (!confirm(`Xóa "${m.name}"?`)) return;
    this.settings.deleteLookup(this.resource, m.id).subscribe(() => this.load());
  }

  // ===== Màu sắc =====
  addColor(): void {
    const name = this.newColorName.trim();
    if (!name || !this.editing) return;
    this.colorError = '';
    this.designService.createColorFabric({ name, materialId: this.editing.id }).subscribe({
      next: () => { this.newColorName = ''; this.loadAllColors(); },
      error: err => this.colorError = err?.error?.message || 'Thêm màu thất bại.'
    });
  }

  startEditColor(c: ColorFabric): void {
    this.editingColorId = c.id;
    this.editingColorName = c.name;
    this.colorError = '';
  }

  saveEditColor(c: ColorFabric): void {
    const name = this.editingColorName.trim();
    if (!name) return;
    this.designService.updateColorFabric(c.id, { id: c.id, name, description: c.description, materialId: c.materialId }).subscribe({
      next: () => { this.editingColorId = null; this.loadAllColors(); },
      error: err => this.colorError = err?.error?.message || 'Cập nhật màu thất bại.'
    });
  }

  removeColor(c: ColorFabric): void {
    if (!confirm(`Xóa màu "${c.name}"?`)) return;
    this.colorError = '';
    this.designService.deleteColorFabric(c.id).subscribe({
      next: () => this.loadAllColors(),
      error: err => this.colorError = err?.error?.message || 'Xóa màu thất bại.'
    });
  }

  private resetColorInputs(): void {
    this.newColorName = '';
    this.editingColorId = null;
    this.editingColorName = '';
    this.colorError = '';
  }
}
