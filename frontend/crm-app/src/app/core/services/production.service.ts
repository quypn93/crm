import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, map } from 'rxjs';
import { environment } from '../../../environments/environment';
import { ApiService, ApiResponse } from './api.service';

export interface ProductionStage {
  id: string;
  stageOrder: number;
  stageName: string;
  description?: string;
  responsibleRole?: string;
  isActive: boolean;
}

export interface OrderProductionStep {
  id: string;
  orderId: string;
  productionStageId: string;
  stageOrder: number;
  stageName: string;
  responsibleRole?: string;
  isCompleted: boolean;
  completedByUserId?: string;
  completedByUserName?: string;
  completedAt?: string;
  notes?: string;
}

export interface OrderProductionProgress {
  orderId: string;
  orderNumber: string;
  customerName?: string;
  orderStatus: number;
  totalSteps: number;
  completedSteps: number;
  progressPercent: number;
  currentStageName?: string;
  isFullyCompleted: boolean;
  steps: OrderProductionStep[];
}

export interface CreateProductionStageDto {
  stageOrder: number;
  stageName: string;
  description?: string;
  responsibleRole?: string;
}

export interface UpdateProductionStageDto extends CreateProductionStageDto {
  isActive: boolean;
}

export interface ReorderProductionStagesDto {
  stages: { id: string; newOrder: number }[];
}

export interface CompleteProductionStepDto {
  notes?: string;
}

@Injectable({ providedIn: 'root' })
export class ProductionService {
  constructor(private api: ApiService, private http: HttpClient) {}

  // ── Stage management ───────────────────────────────────────────────
  getStages(): Observable<ProductionStage[]> {
    return this.api.get<ProductionStage[]>('production-stages');
  }

  getActiveStages(): Observable<ProductionStage[]> {
    return this.api.get<ProductionStage[]>('production-stages/active');
  }

  createStage(dto: CreateProductionStageDto): Observable<ProductionStage> {
    return this.api.post<ProductionStage>('production-stages', dto);
  }

  updateStage(id: string, dto: UpdateProductionStageDto): Observable<ProductionStage> {
    return this.api.put<ProductionStage>(`production-stages/${id}`, dto);
  }

  deleteStage(id: string): Observable<void> {
    return this.api.delete<void>(`production-stages/${id}`);
  }

  reorderStages(dto: ReorderProductionStagesDto): Observable<void> {
    return this.api.put<void>('production-stages/reorder', dto);
  }

  // ── Production dashboard ──────────────────────────────────────────
  getDashboard(): Observable<OrderProductionProgress[]> {
    return this.api.get<OrderProductionProgress[]>('production/dashboard');
  }

  // ── Order production progress ──────────────────────────────────────
  getOrderProgress(orderId: string): Observable<OrderProductionProgress> {
    return this.api.get<OrderProductionProgress>(`orders/${orderId}/production`);
  }

  completeStep(orderId: string, stageId: string, dto: CompleteProductionStepDto): Observable<OrderProductionStep> {
    return this.api.post<OrderProductionStep>(`orders/${orderId}/production/steps/${stageId}/complete`, dto);
  }

  // ── QR scan (mobile) ───────────────────────────────────────────────
  // Dùng HttpClient trực tiếp với header X-Silent-Auth: token được đính kèm
  // nếu có, nhưng 401 sẽ propagate ra component thay vì kích hoạt logout
  // (tránh redirect login khi khách quét QR công khai).
  getProgressByToken(token: string): Observable<OrderProductionProgress> {
    return this.http
      .get<ApiResponse<OrderProductionProgress>>(`${environment.apiUrl}/production/scan/${token}`, {
        headers: { 'X-Silent-Auth': '1' }
      })
      .pipe(map(r => r.data));
  }

  completeStepByToken(token: string, stageId: string, dto: CompleteProductionStepDto): Observable<OrderProductionStep> {
    return this.http
      .post<ApiResponse<OrderProductionStep>>(`${environment.apiUrl}/production/scan/${token}/steps/${stageId}/complete`, dto, {
        headers: { 'X-Silent-Auth': '1' }
      })
      .pipe(map(r => r.data));
  }

  // ── Helper ─────────────────────────────────────────────────────────
  getRoleLabel(role?: string): string {
    const map: Record<string, string> = {
      'Admin': 'Quản trị viên',
      'ProductionManager': 'Quản lý sản xuất',
      'QualityControl': 'Kiểm tra chất lượng',
      'DeliveryManager': 'Quản lý giao hàng',
      'SalesManager': 'Trưởng phòng kinh doanh',
      'SalesRep': 'Nhân viên kinh doanh',
    };
    return role ? (map[role] || role) : 'Tất cả';
  }
}
