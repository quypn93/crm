import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, map } from 'rxjs';
import { environment } from '../../../environments/environment';
import { ApiService, PagedResult, ApiResponse } from './api.service';
import {
  Order,
  OrderItem,
  OrderStatus,
  PaymentStatus,
  CreateOrderRequest,
  UpdateOrderRequest,
  UpdateOrderStatusRequest,
  UpdatePaymentRequest,
  OrderFilter,
  OrderSummary,
  OrderStatusLabels,
  PaymentStatusLabels,
  OrderStatusColors,
  PaymentStatusColors
} from '../models/order.model';

export interface OrderSearchParams {
  search?: string;
  customerId?: string;
  status?: OrderStatus;
  paymentStatus?: PaymentStatus;
  assignedToUserId?: string;
  orderDateFrom?: string;
  orderDateTo?: string;
  minAmount?: number;
  maxAmount?: number;
  page?: number;
  pageSize?: number;
  sortBy?: string;
  sortOrder?: string;
}

@Injectable({
  providedIn: 'root'
})
export class OrderService {
  constructor(private api: ApiService, private http: HttpClient) {}

  getOrders(params: OrderSearchParams): Observable<PagedResult<Order>> {
    return this.api.get<PagedResult<Order>>('orders', this.api.buildParams(params));
  }

  getOrder(id: string): Observable<Order> {
    return this.api.get<Order>(`orders/${id}`);
  }

  createOrder(order: CreateOrderRequest): Observable<Order> {
    return this.api.post<Order>('orders', order);
  }

  updateOrder(id: string, order: UpdateOrderRequest): Observable<Order> {
    return this.api.put<Order>(`orders/${id}`, order);
  }

  deleteOrder(id: string): Observable<void> {
    return this.api.delete<void>(`orders/${id}`);
  }

  updateOrderStatus(id: string, request: UpdateOrderStatusRequest): Observable<Order> {
    return this.api.put<Order>(`orders/${id}/status`, request);
  }

  updatePayment(id: string, request: UpdatePaymentRequest): Observable<Order> {
    return this.api.put<Order>(`orders/${id}/payment`, request);
  }

  createFromDeal(dealId: string): Observable<Order> {
    return this.api.post<Order>(`orders/from-deal/${dealId}`, {});
  }

  getOrdersByCustomer(customerId: string, page: number = 1, pageSize: number = 10): Observable<PagedResult<Order>> {
    return this.api.get<PagedResult<Order>>(`orders/customer/${customerId}`, this.api.buildParams({ page, pageSize }));
  }

  getSummary(dateFrom?: string, dateTo?: string): Observable<OrderSummary> {
    const params: any = {};
    if (dateFrom) params.dateFrom = dateFrom;
    if (dateTo) params.dateTo = dateTo;
    return this.api.get<OrderSummary>('orders/summary', this.api.buildParams(params));
  }

  // Helper methods
  getStatusLabel(status: OrderStatus): string {
    return OrderStatusLabels[status] || 'Không xác định';
  }

  getPaymentStatusLabel(status: PaymentStatus): string {
    return PaymentStatusLabels[status] || 'Không xác định';
  }

  getStatusColor(status: OrderStatus): string {
    return OrderStatusColors[status] || '#6c757d';
  }

  getPaymentStatusColor(status: PaymentStatus): string {
    return PaymentStatusColors[status] || '#6c757d';
  }

  getStatusOptions(): { value: OrderStatus; label: string }[] {
    return Object.keys(OrderStatus)
      .filter(key => !isNaN(Number(key)))
      .map(key => ({
        value: Number(key) as OrderStatus,
        label: OrderStatusLabels[Number(key) as OrderStatus]
      }));
  }

  getPaymentStatusOptions(): { value: PaymentStatus; label: string }[] {
    return Object.keys(PaymentStatus)
      .filter(key => !isNaN(Number(key)))
      .map(key => ({
        value: Number(key) as PaymentStatus,
        label: PaymentStatusLabels[Number(key) as PaymentStatus]
      }));
  }

  getNextValidStatuses(currentStatus: OrderStatus): OrderStatus[] {
    const transitions: Record<OrderStatus, OrderStatus[]> = {
      [OrderStatus.Draft]: [OrderStatus.Confirmed, OrderStatus.Cancelled],
      [OrderStatus.Confirmed]: [OrderStatus.InProduction, OrderStatus.Cancelled],
      [OrderStatus.InProduction]: [OrderStatus.QualityCheck, OrderStatus.Cancelled],
      [OrderStatus.QualityCheck]: [OrderStatus.ReadyToShip, OrderStatus.InProduction],
      [OrderStatus.ReadyToShip]: [OrderStatus.Shipping],
      [OrderStatus.Shipping]: [OrderStatus.Delivered],
      [OrderStatus.Delivered]: [OrderStatus.Completed],
      [OrderStatus.Completed]: [],
      [OrderStatus.Cancelled]: []
    };
    return transitions[currentStatus] || [];
  }

  // Get allowed statuses from API (role-based)
  getAllowedStatuses(orderId: string): Observable<OrderStatus[]> {
    return this.api.get<OrderStatus[]>(`orders/${orderId}/allowed-statuses`);
  }

  generateQr(orderId: string): Observable<Order> {
    return this.api.post<Order>(`orders/${orderId}/generate-qr`, {});
  }

  uploadDesignImage(orderId: string, file: File): Observable<Order> {
    const form = new FormData();
    form.append('file', file);
    return this.api.post<Order>(`orders/${orderId}/design-image`, form);
  }

  getPublicByToken(token: string): Observable<Order> {
    // Dùng HttpClient trực tiếp để bỏ qua auth interceptor (không gửi token
    // cũ nếu có — tránh 401 kích hoạt logout khi khách quét QR).
    return this.http
      .get<ApiResponse<Order>>(`${environment.apiUrl}/orders/public/by-token/${token}`, {
        headers: { 'X-Skip-Auth': '1' }
      })
      .pipe(map(r => r.data));
  }
}
