import { Component, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { DesignService, ColorFabric, CreateColorFabricDto, UpdateColorFabricDto } from '../../../core/services/design.service';
import { SettingsService } from '../../../core/services/settings.service';
import { LookupItem } from '../../../core/models/lookup.model';

@Component({
  selector: 'app-color-fabrics',
  templateUrl: './color-fabrics.component.html',
  styleUrls: ['./color-fabrics.component.scss']
})
export class ColorFabricsComponent implements OnInit {
  colorFabrics: ColorFabric[] = [];
  isLoading = false;
  showForm = false;
  isEditing = false;
  currentFabric: ColorFabric | null = null;
  materials: LookupItem[] = [];

  formData = {
    name: '',
    description: '',
    materialId: ''
  };

  constructor(
    private designService: DesignService,
    private settingsService: SettingsService,
    private router: Router
  ) {}

  ngOnInit(): void {
    this.settingsService.getLookups('materials').subscribe(m => this.materials = m || []);
    this.loadColorFabrics();
  }

  loadColorFabrics(): void {
    this.isLoading = true;
    this.designService.getAllColorFabrics().subscribe({
      next: (fabrics) => {
        this.colorFabrics = fabrics;
        this.isLoading = false;
      },
      error: () => {
        this.isLoading = false;
      }
    });
  }

  openAddForm(): void {
    this.showForm = true;
    this.isEditing = false;
    this.currentFabric = null;
    this.formData = { name: '', description: '', materialId: '' };
  }

  openEditForm(fabric: ColorFabric): void {
    this.showForm = true;
    this.isEditing = true;
    this.currentFabric = fabric;
    this.formData = {
      name: fabric.name,
      description: fabric.description || '',
      materialId: fabric.materialId || ''
    };
  }

  closeForm(): void {
    this.showForm = false;
    this.currentFabric = null;
    this.formData = { name: '', description: '', materialId: '' };
  }

  saveColorFabric(): void {
    if (!this.formData.name.trim()) return;

    if (this.isEditing && this.currentFabric) {
      const dto: UpdateColorFabricDto = {
        id: this.currentFabric.id,
        name: this.formData.name,
        description: this.formData.description || undefined,
        materialId: this.formData.materialId || undefined
      };
      this.designService.updateColorFabric(this.currentFabric.id, dto).subscribe({
        next: () => {
          this.closeForm();
          this.loadColorFabrics();
        }
      });
    } else {
      const dto: CreateColorFabricDto = {
        name: this.formData.name,
        description: this.formData.description || undefined,
        materialId: this.formData.materialId || undefined
      };
      this.designService.createColorFabric(dto).subscribe({
        next: () => {
          this.closeForm();
          this.loadColorFabrics();
        }
      });
    }
  }

  deleteColorFabric(fabric: ColorFabric): void {
    if (confirm(`Bạn có chắc muốn xóa màu vải "${fabric.name}"?`)) {
      this.designService.deleteColorFabric(fabric.id).subscribe({
        next: () => {
          this.loadColorFabrics();
        }
      });
    }
  }

  goBack(): void {
    this.router.navigate(['/designs']);
  }
}
