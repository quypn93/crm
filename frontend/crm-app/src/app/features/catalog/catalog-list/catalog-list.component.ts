import { Component, OnInit } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { DesignService, ShirtComponent, ComponentType } from '../../../core/services/design.service';

@Component({
  selector: 'app-catalog-list',
  templateUrl: './catalog-list.component.html',
  styleUrls: ['./catalog-list.component.scss']
})
export class CatalogListComponent implements OnInit {
  type!: ComponentType;
  title = '';
  placeholder = '';
  items: ShirtComponent[] = [];
  isLoading = false;
  showForm = false;
  editingItem: ShirtComponent | null = null;
  inputName = '';
  errorMessage = '';

  constructor(private route: ActivatedRoute, private designService: DesignService) {}

  ngOnInit(): void {
    this.route.data.subscribe(data => {
      this.type = data['componentType'];
      this.title = data['title'];
      this.placeholder = data['placeholder'] || 'Nhập tên...';
      this.load();
    });
  }

  load(): void {
    this.isLoading = true;
    this.designService.getActiveShirtComponentsByType(this.type).subscribe({
      next: (items) => { this.items = items; this.isLoading = false; },
      error: () => { this.isLoading = false; }
    });
  }

  openAdd(): void {
    this.editingItem = null;
    this.inputName = '';
    this.errorMessage = '';
    this.showForm = true;
  }

  openEdit(item: ShirtComponent): void {
    this.editingItem = item;
    this.inputName = item.name;
    this.errorMessage = '';
    this.showForm = true;
  }

  closeForm(): void {
    this.showForm = false;
    this.editingItem = null;
    this.inputName = '';
  }

  save(): void {
    if (!this.inputName.trim()) { this.errorMessage = 'Vui lòng nhập tên.'; return; }
    const dto = { name: this.inputName.trim(), type: this.type };
    const obs$ = this.editingItem
      ? this.designService.updateShirtComponent(this.editingItem.id, { ...dto, id: this.editingItem.id })
      : this.designService.createShirtComponent(dto);
    obs$.subscribe({ next: () => { this.closeForm(); this.load(); }, error: () => { this.errorMessage = 'Lưu thất bại.'; } });
  }

  delete(item: ShirtComponent): void {
    if (!confirm(`Xóa "${item.name}"?`)) return;
    this.designService.deleteShirtComponent(item.id).subscribe({ next: () => this.load() });
  }
}
