import { Component, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { DesignService, ColorFabric, CreateColorFabricDto, UpdateColorFabricDto } from '../../../core/services/design.service';

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

  formData = {
    name: '',
    description: ''
  };

  constructor(
    private designService: DesignService,
    private router: Router
  ) {}

  ngOnInit(): void {
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
    this.formData = { name: '', description: '' };
  }

  openEditForm(fabric: ColorFabric): void {
    this.showForm = true;
    this.isEditing = true;
    this.currentFabric = fabric;
    this.formData = {
      name: fabric.name,
      description: fabric.description || ''
    };
  }

  closeForm(): void {
    this.showForm = false;
    this.currentFabric = null;
    this.formData = { name: '', description: '' };
  }

  saveColorFabric(): void {
    if (!this.formData.name.trim()) return;

    if (this.isEditing && this.currentFabric) {
      const dto: UpdateColorFabricDto = {
        id: this.currentFabric.id,
        name: this.formData.name,
        description: this.formData.description || undefined
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
        description: this.formData.description || undefined
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
