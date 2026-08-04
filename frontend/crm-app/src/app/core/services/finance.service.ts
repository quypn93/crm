import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable, map } from 'rxjs';
import { environment } from '../../../environments/environment';
import { ApiResponse } from './api.service';
import {
  OrderCostListResult, OrderCostListItem, UpsertOrderCost, BulkOrderCostItem, CostImportResult,
  ExpenseCategory, CreateExpenseCategory,
  FixedExpense, CreateFixedExpense, FixedExpenseListResult,
  PayrollEntry, PayrollPeriod,
  OrderProfitResult, MonthlyProfitResult, MonthlyProfitDetail
} from '../models/finance.model';

@Injectable({ providedIn: 'root' })
export class FinanceService {
  private readonly base = `${environment.apiUrl}/finance`;

  constructor(private http: HttpClient) {}

  private unwrap<T>(obs: Observable<ApiResponse<T>>): Observable<T> {
    return obs.pipe(map(r => r.data));
  }

  private params(filter: { [key: string]: any }): HttpParams {
    let p = new HttpParams();
    Object.keys(filter).forEach(key => {
      const value = filter[key];
      if (value !== null && value !== undefined && value !== '') {
        p = p.set(key, value instanceof Date ? value.toISOString() : String(value));
      }
    });
    return p;
  }

  // ── Chi phí sản xuất hàng hóa ───────────────────────────────────────────
  getOrderCosts(filter: { [key: string]: any }): Observable<OrderCostListResult> {
    return this.unwrap(this.http.get<ApiResponse<OrderCostListResult>>(
      `${this.base}/order-costs`, { params: this.params(filter) }));
  }

  saveOrderCost(orderId: string, dto: UpsertOrderCost): Observable<OrderCostListItem> {
    return this.unwrap(this.http.put<ApiResponse<OrderCostListItem>>(`${this.base}/order-costs/${orderId}`, dto));
  }

  bulkSaveOrderCosts(items: BulkOrderCostItem[]): Observable<number> {
    return this.unwrap(this.http.post<ApiResponse<number>>(`${this.base}/order-costs/bulk`, { items }));
  }

  importOrderCosts(file: File): Observable<ApiResponse<CostImportResult>> {
    const form = new FormData();
    form.append('file', file);
    return this.http.post<ApiResponse<CostImportResult>>(`${this.base}/order-costs/import`, form);
  }

  /** File mẫu .xlsx — tải blob rồi trigger download ở component. */
  downloadImportTemplate(): Observable<Blob> {
    return this.http.get(`${this.base}/order-costs/import-template`, { responseType: 'blob' });
  }

  uploadCostAttachment(orderId: string, file: File): Observable<OrderCostListItem> {
    const form = new FormData();
    form.append('file', file);
    return this.unwrap(this.http.post<ApiResponse<OrderCostListItem>>(
      `${this.base}/order-costs/${orderId}/attachment`, form));
  }

  // ── Đầu mục chi phí ─────────────────────────────────────────────────────
  getExpenseCategories(activeOnly = false): Observable<ExpenseCategory[]> {
    return this.unwrap(this.http.get<ApiResponse<ExpenseCategory[]>>(
      `${this.base}/expense-categories`, { params: this.params({ activeOnly }) }));
  }
  createExpenseCategory(dto: CreateExpenseCategory): Observable<ExpenseCategory> {
    return this.unwrap(this.http.post<ApiResponse<ExpenseCategory>>(`${this.base}/expense-categories`, dto));
  }
  updateExpenseCategory(id: string, dto: CreateExpenseCategory & { id: string }): Observable<ExpenseCategory> {
    return this.unwrap(this.http.put<ApiResponse<ExpenseCategory>>(`${this.base}/expense-categories/${id}`, dto));
  }
  deleteExpenseCategory(id: string): Observable<void> {
    return this.unwrap(this.http.delete<ApiResponse<void>>(`${this.base}/expense-categories/${id}`));
  }

  // ── Chi phí cố định ─────────────────────────────────────────────────────
  getFixedExpenses(filter: { [key: string]: any }): Observable<FixedExpenseListResult> {
    return this.unwrap(this.http.get<ApiResponse<FixedExpenseListResult>>(
      `${this.base}/fixed-expenses`, { params: this.params(filter) }));
  }
  createFixedExpense(dto: CreateFixedExpense): Observable<FixedExpense> {
    return this.unwrap(this.http.post<ApiResponse<FixedExpense>>(`${this.base}/fixed-expenses`, dto));
  }
  updateFixedExpense(id: string, dto: CreateFixedExpense & { id: string }): Observable<FixedExpense> {
    return this.unwrap(this.http.put<ApiResponse<FixedExpense>>(`${this.base}/fixed-expenses/${id}`, dto));
  }
  deleteFixedExpense(id: string): Observable<void> {
    return this.unwrap(this.http.delete<ApiResponse<void>>(`${this.base}/fixed-expenses/${id}`));
  }

  // ── Chi phí nhân sự ─────────────────────────────────────────────────────
  getPayroll(year: number, month: number): Observable<PayrollPeriod> {
    return this.unwrap(this.http.get<ApiResponse<PayrollPeriod>>(
      `${this.base}/payroll`, { params: this.params({ year, month }) }));
  }
  createPayrollEntry(dto: Partial<PayrollEntry>): Observable<PayrollEntry> {
    return this.unwrap(this.http.post<ApiResponse<PayrollEntry>>(`${this.base}/payroll`, dto));
  }
  updatePayrollEntry(id: string, dto: Partial<PayrollEntry>): Observable<PayrollEntry> {
    return this.unwrap(this.http.put<ApiResponse<PayrollEntry>>(`${this.base}/payroll/${id}`, dto));
  }
  deletePayrollEntry(id: string): Observable<void> {
    return this.unwrap(this.http.delete<ApiResponse<void>>(`${this.base}/payroll/${id}`));
  }
  copyPayrollFromPrevious(year: number, month: number): Observable<number> {
    return this.unwrap(this.http.post<ApiResponse<number>>(
      `${this.base}/payroll/copy-from-previous?year=${year}&month=${month}`, {}));
  }

  // ── Báo cáo lãi/lỗ ──────────────────────────────────────────────────────
  getOrderProfit(filter: { [key: string]: any }): Observable<OrderProfitResult> {
    return this.unwrap(this.http.get<ApiResponse<OrderProfitResult>>(
      `${this.base}/reports/order-profit`, { params: this.params(filter) }));
  }
  getMonthlyProfit(year: number, revenueBasis?: string): Observable<MonthlyProfitResult> {
    return this.unwrap(this.http.get<ApiResponse<MonthlyProfitResult>>(
      `${this.base}/reports/monthly-profit`, { params: this.params({ year, revenueBasis }) }));
  }
  getMonthDetail(year: number, month: number, revenueBasis?: string): Observable<MonthlyProfitDetail> {
    return this.unwrap(this.http.get<ApiResponse<MonthlyProfitDetail>>(
      `${this.base}/reports/monthly-profit/${year}/${month}/detail`, { params: this.params({ revenueBasis }) }));
  }
}
