import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { ApiService } from './api.service';

// Enums
export enum ComponentType {
  Collar = 1,
  Sleeve = 2,
  Logo = 4,
  Button = 5,
  Fabric = 6,
  Color = 7,
  Body = 8,
  Stripe = 9,
  CollarStripe = 10
}

export const ComponentTypeNames: { [key: number]: string } = {
  [ComponentType.Collar]: 'Cổ áo',
  [ComponentType.Sleeve]: 'Tay áo',
  [ComponentType.Logo]: 'Logo',
  [ComponentType.Button]: 'Nút áo',
  [ComponentType.Fabric]: 'Chất liệu vải',
  [ComponentType.Color]: 'Màu sắc',
  [ComponentType.Body]: 'Thân áo',
  [ComponentType.Stripe]: 'Sọc',
  [ComponentType.CollarStripe]: 'Sọc cổ áo'
};

// Interfaces
export interface ColorFabric {
  id: string;
  name: string;
  description?: string;
  createdAt: string;
  updatedAt?: string;
}

export interface ShirtComponent {
  id: string;
  name: string;
  imageUrl?: string;
  womenImageUrl?: string;
  type: ComponentType;
  typeName: string;
  isDeleted: boolean;
  colorFabricId?: string;
  colorFabricName?: string;
  createdAt: string;
  updatedAt?: string;
}

export interface Design {
  id: string;
  designName: string;
  designData?: string;
  selectedComponents?: string;
  designer?: string;
  customerFullName?: string;
  total?: number;
  sizeMan?: string;
  sizeWomen?: string;
  sizeKid?: string;
  oversized?: string;
  finishedDate?: string;
  noteConfection?: string;
  noteOldCodeOrder?: string;
  noteAttachTagLabel?: string;
  noteOther?: string;
  saleStaff?: string;
  colorFabricId?: string;
  colorFabricName?: string;
  orderId?: string;
  orderNumber?: string;
  createdByUserId?: string;
  createdByUserName?: string;
  createdAt: string;
  updatedAt?: string;
}

export interface DesignDetail extends Design {
  customerName?: string;
  customerPhone?: string;
  customerEmail?: string;
}

// Filter DTOs
export interface ColorFabricFilter {
  search?: string;
  page?: number;
  pageSize?: number;
  sortBy?: string;
  sortOrder?: string;
}

export interface ShirtComponentFilter {
  search?: string;
  type?: ComponentType;
  colorFabricId?: string;
  includeDeleted?: boolean;
  page?: number;
  pageSize?: number;
  sortBy?: string;
  sortOrder?: string;
}

export interface DesignFilter {
  search?: string;
  orderId?: string;
  colorFabricId?: string;
  createdByUserId?: string;
  fromDate?: string;
  toDate?: string;
  page?: number;
  pageSize?: number;
  sortBy?: string;
  sortOrder?: string;
}

// Create/Update DTOs
export interface CreateColorFabricDto {
  name: string;
  description?: string;
}

export interface UpdateColorFabricDto extends CreateColorFabricDto {
  id: string;
}

export interface CreateShirtComponentDto {
  name: string;
  imageUrl?: string;
  womenImageUrl?: string;
  type: ComponentType;
  colorFabricId?: string;
}

export interface UpdateShirtComponentDto extends CreateShirtComponentDto {
  id: string;
}

export interface CreateDesignDto {
  designName: string;
  designData?: string;
  selectedComponents?: string;
  designer?: string;
  customerFullName?: string;
  total?: number;
  sizeMan?: string;
  sizeWomen?: string;
  sizeKid?: string;
  oversized?: string;
  finishedDate?: string;
  noteConfection?: string;
  noteOldCodeOrder?: string;
  noteAttachTagLabel?: string;
  noteOther?: string;
  saleStaff?: string;
  colorFabricId?: string;
  orderId?: string;
}

export interface UpdateDesignDto extends CreateDesignDto {
  id: string;
}

export interface DuplicateDesignDto {
  newDesignName?: string;
  newOrderId?: string;
}

export interface PagedResult<T> {
  items: T[];
  totalCount: number;
  page: number;
  pageSize: number;
  totalPages: number;
}

@Injectable({
  providedIn: 'root'
})
export class DesignService {
  constructor(private api: ApiService) {}

  // ColorFabric methods
  getColorFabrics(params?: ColorFabricFilter): Observable<PagedResult<ColorFabric>> {
    return this.api.get<PagedResult<ColorFabric>>('colorfabrics', this.api.buildParams(params || {}));
  }

  getAllColorFabrics(): Observable<ColorFabric[]> {
    return this.api.get<ColorFabric[]>('colorfabrics/all');
  }

  getColorFabric(id: string): Observable<ColorFabric> {
    return this.api.get<ColorFabric>(`colorfabrics/${id}`);
  }

  createColorFabric(dto: CreateColorFabricDto): Observable<ColorFabric> {
    return this.api.post<ColorFabric>('colorfabrics', dto);
  }

  updateColorFabric(id: string, dto: UpdateColorFabricDto): Observable<ColorFabric> {
    return this.api.put<ColorFabric>(`colorfabrics/${id}`, dto);
  }

  deleteColorFabric(id: string): Observable<void> {
    return this.api.delete<void>(`colorfabrics/${id}`);
  }

  // ShirtComponent methods
  getShirtComponents(params?: ShirtComponentFilter): Observable<PagedResult<ShirtComponent>> {
    return this.api.get<PagedResult<ShirtComponent>>('shirtcomponents', this.api.buildParams(params || {}));
  }

  getShirtComponent(id: string): Observable<ShirtComponent> {
    return this.api.get<ShirtComponent>(`shirtcomponents/${id}`);
  }

  getShirtComponentsByType(type: ComponentType): Observable<ShirtComponent[]> {
    return this.api.get<ShirtComponent[]>(`shirtcomponents/by-type/${type}`);
  }

  getActiveShirtComponentsByType(type: ComponentType): Observable<ShirtComponent[]> {
    return this.api.get<ShirtComponent[]>(`shirtcomponents/active/by-type/${type}`);
  }

  getShirtComponentsByColorFabric(colorFabricId: string): Observable<ShirtComponent[]> {
    return this.api.get<ShirtComponent[]>(`shirtcomponents/by-colorfabric/${colorFabricId}`);
  }

  getComponentTypes(): Observable<{ [key: number]: string }> {
    return this.api.get<{ [key: number]: string }>('shirtcomponents/types');
  }

  createShirtComponent(dto: CreateShirtComponentDto): Observable<ShirtComponent> {
    return this.api.post<ShirtComponent>('shirtcomponents', dto);
  }

  updateShirtComponent(id: string, dto: UpdateShirtComponentDto): Observable<ShirtComponent> {
    return this.api.put<ShirtComponent>(`shirtcomponents/${id}`, dto);
  }

  deleteShirtComponent(id: string): Observable<void> {
    return this.api.delete<void>(`shirtcomponents/${id}`);
  }

  restoreShirtComponent(id: string): Observable<void> {
    return this.api.post<void>(`shirtcomponents/${id}/restore`, {});
  }

  // Design methods
  getDesigns(params?: DesignFilter): Observable<PagedResult<Design>> {
    return this.api.get<PagedResult<Design>>('designs', this.api.buildParams(params || {}));
  }

  getDesign(id: string): Observable<DesignDetail> {
    return this.api.get<DesignDetail>(`designs/${id}`);
  }

  getDesignsByOrder(orderId: string): Observable<Design[]> {
    return this.api.get<Design[]>(`designs/by-order/${orderId}`);
  }

  getMyDesigns(): Observable<Design[]> {
    return this.api.get<Design[]>('designs/my-designs');
  }

  createDesign(dto: CreateDesignDto): Observable<Design> {
    return this.api.post<Design>('designs', dto);
  }

  updateDesign(id: string, dto: UpdateDesignDto): Observable<Design> {
    return this.api.put<Design>(`designs/${id}`, dto);
  }

  deleteDesign(id: string): Observable<void> {
    return this.api.delete<void>(`designs/${id}`);
  }

  duplicateDesign(id: string, dto: DuplicateDesignDto): Observable<Design> {
    return this.api.post<Design>(`designs/${id}/duplicate`, dto);
  }

  // Helper methods
  getComponentTypeList(): { value: ComponentType; label: string }[] {
    return Object.entries(ComponentTypeNames).map(([key, label]) => ({
      value: Number(key) as ComponentType,
      label
    }));
  }
}
