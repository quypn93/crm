import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, map } from 'rxjs';
import { environment } from '../../../environments/environment';
import { ApiResponse } from './api.service';
import {
  LookupItem, CreateLookupItem,
  Collection, CreateCollection,
  ProductionDaysOption, CreateProductionDaysOption,
  DepositTransaction, CreateDepositTransaction,
  SenderAddress, CreateSenderAddress, VtpCategory
} from '../models/lookup.model';

@Injectable({ providedIn: 'root' })
export class SettingsService {
  private readonly base = environment.apiUrl;

  constructor(private http: HttpClient) {}

  private unwrap<T>(obs: Observable<ApiResponse<T>>): Observable<T> {
    return obs.pipe(map(r => r.data));
  }

  // Collections
  getCollections(): Observable<Collection[]> {
    return this.unwrap(this.http.get<ApiResponse<Collection[]>>(`${this.base}/collections`));
  }
  createCollection(dto: CreateCollection): Observable<Collection> {
    return this.unwrap(this.http.post<ApiResponse<Collection>>(`${this.base}/collections`, dto));
  }
  updateCollection(id: string, dto: CreateCollection & { id: string }): Observable<Collection> {
    return this.unwrap(this.http.put<ApiResponse<Collection>>(`${this.base}/collections/${id}`, dto));
  }
  deleteCollection(id: string): Observable<void> {
    return this.unwrap(this.http.delete<ApiResponse<void>>(`${this.base}/collections/${id}`));
  }

  // Sender addresses (địa chỉ gửi hàng — Viettel Post)
  getSenderAddresses(): Observable<SenderAddress[]> {
    return this.unwrap(this.http.get<ApiResponse<SenderAddress[]>>(`${this.base}/sender-addresses`));
  }
  createSenderAddress(dto: CreateSenderAddress): Observable<SenderAddress> {
    return this.unwrap(this.http.post<ApiResponse<SenderAddress>>(`${this.base}/sender-addresses`, dto));
  }
  updateSenderAddress(id: string, dto: CreateSenderAddress & { id: string }): Observable<SenderAddress> {
    return this.unwrap(this.http.put<ApiResponse<SenderAddress>>(`${this.base}/sender-addresses/${id}`, dto));
  }
  deleteSenderAddress(id: string): Observable<void> {
    return this.unwrap(this.http.delete<ApiResponse<void>>(`${this.base}/sender-addresses/${id}`));
  }

  // Danh mục hành chính Viettel Post cho dropdown chọn kho gửi
  getVtpProvinces(): Observable<VtpCategory[]> {
    return this.unwrap(this.http.get<ApiResponse<VtpCategory[]>>(`${this.base}/viettelpost/provinces`));
  }
  getVtpDistricts(provinceId: number): Observable<VtpCategory[]> {
    return this.unwrap(this.http.get<ApiResponse<VtpCategory[]>>(`${this.base}/viettelpost/districts?provinceId=${provinceId}`));
  }
  getVtpWards(districtId: number): Observable<VtpCategory[]> {
    return this.unwrap(this.http.get<ApiResponse<VtpCategory[]>>(`${this.base}/viettelpost/wards?districtId=${districtId}`));
  }

  // Generic lookup helpers
  getLookups(resource: 'materials' | 'product-forms' | 'product-specifications' | 'order-types'): Observable<LookupItem[]> {
    return this.unwrap(this.http.get<ApiResponse<LookupItem[]>>(`${this.base}/${resource}`));
  }
  createLookup(resource: string, dto: CreateLookupItem): Observable<LookupItem> {
    return this.unwrap(this.http.post<ApiResponse<LookupItem>>(`${this.base}/${resource}`, dto));
  }
  updateLookup(resource: string, id: string, dto: CreateLookupItem & { id: string }): Observable<LookupItem> {
    return this.unwrap(this.http.put<ApiResponse<LookupItem>>(`${this.base}/${resource}/${id}`, dto));
  }
  deleteLookup(resource: string, id: string): Observable<void> {
    return this.unwrap(this.http.delete<ApiResponse<void>>(`${this.base}/${resource}/${id}`));
  }

  // Production days
  getProductionDaysOptions(): Observable<ProductionDaysOption[]> {
    return this.unwrap(this.http.get<ApiResponse<ProductionDaysOption[]>>(`${this.base}/production-days-options`));
  }
  createProductionDaysOption(dto: CreateProductionDaysOption): Observable<ProductionDaysOption> {
    return this.unwrap(this.http.post<ApiResponse<ProductionDaysOption>>(`${this.base}/production-days-options`, dto));
  }
  updateProductionDaysOption(id: string, dto: CreateProductionDaysOption & { id: string }): Observable<ProductionDaysOption> {
    return this.unwrap(this.http.put<ApiResponse<ProductionDaysOption>>(`${this.base}/production-days-options/${id}`, dto));
  }
  deleteProductionDaysOption(id: string): Observable<void> {
    return this.unwrap(this.http.delete<ApiResponse<void>>(`${this.base}/production-days-options/${id}`));
  }

  // Deposits
  getDeposits(): Observable<DepositTransaction[]> {
    return this.unwrap(this.http.get<ApiResponse<DepositTransaction[]>>(`${this.base}/deposits`));
  }
  createDeposit(dto: CreateDepositTransaction): Observable<DepositTransaction> {
    return this.unwrap(this.http.post<ApiResponse<DepositTransaction>>(`${this.base}/deposits`, dto));
  }
  deleteDeposit(id: string): Observable<void> {
    return this.unwrap(this.http.delete<ApiResponse<void>>(`${this.base}/deposits/${id}`));
  }
}
