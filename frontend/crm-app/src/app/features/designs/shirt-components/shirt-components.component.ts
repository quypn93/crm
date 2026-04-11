import { Component, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import {
  DesignService,
  ShirtComponent,
  CreateShirtComponentDto,
  UpdateShirtComponentDto,
  ComponentType,
  ComponentTypeNames,
  ColorFabric
} from '../../../core/services/design.service';

@Component({
  selector: 'app-shirt-components',
  templateUrl: './shirt-components.component.html',
  styleUrls: ['./shirt-components.component.scss']
})
export class ShirtComponentsComponent implements OnInit {
  components: ShirtComponent[] = [];
  colorFabrics: ColorFabric[] = [];
  componentTypes = ComponentTypeNames;
  isLoading = false;
  showForm = false;
  isEditing = false;
  currentComponent: ShirtComponent | null = null;

  filterType: ComponentType | null = null;
  includeDeleted = false;

  formData = {
    name: '',
    type: ComponentType.Collar,
    imageUrl: '',
    womenImageUrl: '',
    colorFabricId: ''
  };

  constructor(
    private designService: DesignService,
    private router: Router
  ) {}

  ngOnInit(): void {
    this.loadComponents();
    this.loadColorFabrics();
  }

  loadComponents(): void {
    this.isLoading = true;
    this.designService.getShirtComponents({
      type: this.filterType ?? undefined,
      includeDeleted: this.includeDeleted,
      pageSize: 100
    }).subscribe({
      next: (response) => {
        this.components = response.items;
        this.isLoading = false;
      },
      error: () => {
        this.isLoading = false;
      }
    });
  }

  loadColorFabrics(): void {
    this.designService.getAllColorFabrics().subscribe({
      next: (fabrics) => {
        this.colorFabrics = fabrics;
      }
    });
  }

  onFilterChange(): void {
    this.loadComponents();
  }

  openAddForm(): void {
    this.showForm = true;
    this.isEditing = false;
    this.currentComponent = null;
    this.formData = {
      name: '',
      type: ComponentType.Collar,
      imageUrl: '',
      womenImageUrl: '',
      colorFabricId: ''
    };
  }

  openEditForm(component: ShirtComponent): void {
    this.showForm = true;
    this.isEditing = true;
    this.currentComponent = component;
    this.formData = {
      name: component.name,
      type: component.type,
      imageUrl: component.imageUrl || '',
      womenImageUrl: component.womenImageUrl || '',
      colorFabricId: component.colorFabricId || ''
    };
  }

  closeForm(): void {
    this.showForm = false;
    this.currentComponent = null;
  }

  saveComponent(): void {
    if (!this.formData.name.trim()) return;

    if (this.isEditing && this.currentComponent) {
      const dto: UpdateShirtComponentDto = {
        id: this.currentComponent.id,
        name: this.formData.name,
        type: this.formData.type,
        imageUrl: this.formData.imageUrl || undefined,
        womenImageUrl: this.formData.womenImageUrl || undefined,
        colorFabricId: this.formData.colorFabricId || undefined
      };
      this.designService.updateShirtComponent(this.currentComponent.id, dto).subscribe({
        next: () => {
          this.closeForm();
          this.loadComponents();
        }
      });
    } else {
      const dto: CreateShirtComponentDto = {
        name: this.formData.name,
        type: this.formData.type,
        imageUrl: this.formData.imageUrl || undefined,
        womenImageUrl: this.formData.womenImageUrl || undefined,
        colorFabricId: this.formData.colorFabricId || undefined
      };
      this.designService.createShirtComponent(dto).subscribe({
        next: () => {
          this.closeForm();
          this.loadComponents();
        }
      });
    }
  }

  deleteComponent(component: ShirtComponent): void {
    if (confirm(`Bạn có chắc muốn xóa thành phần "${component.name}"?`)) {
      this.designService.deleteShirtComponent(component.id).subscribe({
        next: () => {
          this.loadComponents();
        }
      });
    }
  }

  restoreComponent(component: ShirtComponent): void {
    this.designService.restoreShirtComponent(component.id).subscribe({
      next: () => {
        this.loadComponents();
      }
    });
  }

  getComponentTypeName(type: ComponentType): string {
    return ComponentTypeNames[type] || type.toString();
  }

  getTypeKeys(): number[] {
    return Object.keys(ComponentTypeNames).map(k => Number(k));
  }

  goBack(): void {
    this.router.navigate(['/designs']);
  }
}
