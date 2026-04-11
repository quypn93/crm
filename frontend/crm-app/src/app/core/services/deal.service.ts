import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { ApiService } from './api.service';

export interface DealStage {
  id: string;
  name: string;
  order: number;
  color: string;
  probability: number;
  isDefault: boolean;
  isWonStage: boolean;
  isLostStage: boolean;
}

export interface Deal {
  id: string;
  title: string;
  value: number;
  currency: string;
  customerId: string;
  customerName: string;
  stageId: string;
  stageName: string;
  stageColor: string;
  assignedToUserId?: string;
  assignedToUserName?: string;
  expectedCloseDate?: string;
  actualCloseDate?: string;
  probability: number;
  notes?: string;
  lostReason?: string;
  createdAt: string;
  updatedAt?: string;
}

export interface DealSearchParams {
  searchTerm?: string;
  stageId?: string;
  customerId?: string;
  assignedToUserId?: string;
  page?: number;
  pageSize?: number;
}

export interface PagedResult<T> {
  items: T[];
  totalCount: number;
  page: number;
  pageSize: number;
  totalPages: number;
}

export interface CreateDealDto {
  title: string;
  value: number;
  customerId: string;
  stageId?: string;
  expectedCloseDate?: string;
  probability?: number;
  notes?: string;
  assignedToUserId?: string;
}

export interface UpdateDealDto extends CreateDealDto {
  id: string;
}

export interface StageColumn {
  stage: DealStage;
  deals: Deal[];
  totalValue: number;
}

// Pipeline data is returned as StageColumn[] directly from API

@Injectable({
  providedIn: 'root'
})
export class DealService {
  constructor(private api: ApiService) {}

  getDeals(params: DealSearchParams): Observable<PagedResult<Deal>> {
    return this.api.get<PagedResult<Deal>>('deals', this.api.buildParams(params));
  }

  getDeal(id: string): Observable<Deal> {
    return this.api.get<Deal>(`deals/${id}`);
  }

  createDeal(deal: CreateDealDto): Observable<Deal> {
    return this.api.post<Deal>('deals', deal);
  }

  updateDeal(id: string, deal: UpdateDealDto): Observable<Deal> {
    return this.api.put<Deal>(`deals/${id}`, deal);
  }

  deleteDeal(id: string): Observable<void> {
    return this.api.delete<void>(`deals/${id}`);
  }

  getPipeline(): Observable<StageColumn[]> {
    return this.api.get<StageColumn[]>('deals/pipeline');
  }

  updateDealStage(dealId: string, stageId: string): Observable<Deal> {
    return this.api.put<Deal>(`deals/${dealId}/stage`, { stageId });
  }

  markAsWon(id: string): Observable<Deal> {
    return this.api.post<Deal>(`deals/${id}/won`, {});
  }

  markAsLost(id: string, lostReason?: string): Observable<Deal> {
    return this.api.post<Deal>(`deals/${id}/lost`, { lostReason });
  }

  getStages(): Observable<DealStage[]> {
    return this.api.get<DealStage[]>('deals/stages');
  }
}
