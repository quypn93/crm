import { Component, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { OrderService } from '../../../core/services/order.service';
import { Order, OrderStatusLabels } from '../../../core/models/order.model';

@Component({
  selector: 'app-my-warehouse-orders',
  template: `
  <div class="page">
    <div class="page-header">
      <div>
        <h1>Đơn kho của tôi</h1>
        <p class="subtitle">Các đơn đã được gắn vào kho bạn phụ trách (sau khi xử lý vận đơn).</p>
      </div>
    </div>

    <div class="loading" *ngIf="isLoading">Đang tải...</div>

    <table class="data-table" *ngIf="!isLoading">
      <thead>
        <tr><th>Mã đơn</th><th>Khách hàng</th><th>Trạng thái</th><th>Vận đơn</th><th>SL</th><th>Ngày đặt</th></tr>
      </thead>
      <tbody>
        <tr *ngFor="let o of orders" (click)="open(o)" class="row">
          <td class="code">{{ o.orderNumber }}</td>
          <td>{{ o.customerName || 'N/A' }}</td>
          <td>{{ statusLabel(o.status) }}</td>
          <td>{{ o.viettelPostLabel || '—' }}</td>
          <td>{{ o.itemsCount ?? '' }}</td>
          <td>{{ o.orderDate | date:'dd/MM/yyyy' }}</td>
        </tr>
        <tr *ngIf="orders.length === 0"><td colspan="6" class="no-data">Chưa có đơn nào thuộc kho của bạn</td></tr>
      </tbody>
    </table>
  </div>
  `,
  styles: [`
    .page { padding: 20px; }
    .page-header { margin-bottom:16px; }
    .subtitle { color:#6b7280; font-size:13px; margin:4px 0 0; }
    .data-table { width:100%; border-collapse:collapse; background:#fff; border-radius:8px; overflow:hidden; }
    .data-table th, .data-table td { padding:12px; border-bottom:1px solid #eee; text-align:left; font-size:14px; }
    .row { cursor:pointer; }
    .row:hover { background:#f8fafc; }
    .code { font-weight:600; color:#4f46e5; }
    .no-data { text-align:center; color:#9ca3af; }
    .loading { color:#6b7280; padding:20px; }
  `]
})
export class MyWarehouseOrdersComponent implements OnInit {
  orders: Order[] = [];
  isLoading = false;

  constructor(private orderService: OrderService, private router: Router) {}

  ngOnInit(): void {
    this.isLoading = true;
    this.orderService.getMyWarehouseOrders().subscribe({
      next: o => { this.orders = o || []; this.isLoading = false; },
      error: () => { this.orders = []; this.isLoading = false; }
    });
  }

  statusLabel(s: number): string {
    return (OrderStatusLabels as any)[s] || String(s);
  }

  open(o: Order): void {
    this.router.navigate(['/orders', o.id]);
  }
}
