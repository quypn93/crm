import { Component, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { OrderService, OrderSearchParams } from '../../../core/services/order.service';
import { AuthService } from '../../../core/services/auth.service';
import { Order, OrderStatus, PaymentStatus, OrderStatusLabels, PaymentStatusLabels, OrderStatusColors, PaymentStatusColors, UpdateOrderStatusRequest } from '../../../core/models/order.model';

@Component({
  selector: 'app-order-list',
  templateUrl: './order-list.component.html',
  styleUrls: ['./order-list.component.scss']
})
export class OrderListComponent implements OnInit {
  orders: Order[] = [];
  isLoading = false;
  searchTerm = '';
  currentPage = 1;
  pageSize = 10;
  totalItems = 0;
  totalPages = 0;

  // Filters
  selectedStatus: OrderStatus | null = null;
  selectedPaymentStatus: PaymentStatus | null = null;

  statusOptions = this.orderService.getStatusOptions();
  paymentStatusOptions = this.orderService.getPaymentStatusOptions();

  readonly OrderStatus = OrderStatus;

  constructor(
    private orderService: OrderService,
    private authService: AuthService,
    private router: Router
  ) {}

  ngOnInit(): void {
    this.loadOrders();
  }

  loadOrders(): void {
    this.isLoading = true;
    const params: OrderSearchParams = {
      search: this.searchTerm,
      page: this.currentPage,
      pageSize: this.pageSize
    };

    if (this.selectedStatus !== null) {
      params.status = this.selectedStatus;
    }
    if (this.selectedPaymentStatus !== null) {
      params.paymentStatus = this.selectedPaymentStatus;
    }

    this.orderService.getOrders(params).subscribe({
      next: (response) => {
        this.orders = response?.items || [];
        this.totalItems = response?.totalCount || 0;
        this.totalPages = response?.totalPages || 0;
        this.isLoading = false;
      },
      error: () => {
        this.orders = [];
        this.isLoading = false;
      }
    });
  }

  onSearch(): void {
    this.currentPage = 1;
    this.loadOrders();
  }

  onFilterChange(): void {
    this.currentPage = 1;
    this.loadOrders();
  }

  clearFilters(): void {
    this.searchTerm = '';
    this.selectedStatus = null;
    this.selectedPaymentStatus = null;
    this.currentPage = 1;
    this.loadOrders();
  }

  onPageChange(page: number): void {
    this.currentPage = page;
    this.loadOrders();
  }

  viewOrder(id: string): void {
    this.router.navigate(['/orders', id]);
  }

  editOrder(id: string): void {
    this.router.navigate(['/orders', id, 'edit']);
  }

  deleteOrder(order: Order): void {
    if (confirm(`Bạn có chắc muốn xóa đơn hàng "${order.orderNumber}"?`)) {
      this.orderService.deleteOrder(order.id).subscribe({
        next: () => {
          this.loadOrders();
        }
      });
    }
  }

  addOrder(): void {
    this.router.navigate(['/orders/new']);
  }

  canConfirmOrder(): boolean {
    return this.authService.hasAnyRole(['Admin', 'SalesManager', 'SalesRep']);
  }

  canEditOrderInList(order: Order): boolean {
    const editableStatuses = [OrderStatus.Draft, OrderStatus.Confirmed];
    return editableStatuses.includes(order.status) &&
           this.authService.hasAnyRole(['Admin', 'SalesManager', 'SalesRep']);
  }

  canDeleteOrderInList(): boolean {
    return this.authService.hasAnyRole(['Admin', 'SalesManager', 'SalesRep']);
  }

  confirmOrder(order: Order): void {
    const req: UpdateOrderStatusRequest = { status: OrderStatus.Confirmed, notes: '' };
    this.orderService.updateOrderStatus(order.id, req).subscribe({
      next: () => this.loadOrders()
    });
  }

  moveToProduction(order: Order): void {
    const req: UpdateOrderStatusRequest = { status: OrderStatus.InProduction, notes: '' };
    this.orderService.updateOrderStatus(order.id, req).subscribe({
      next: () => this.loadOrders()
    });
  }

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

  formatCurrency(value: number): string {
    return new Intl.NumberFormat('vi-VN', { style: 'currency', currency: 'VND' }).format(value);
  }

  getPages(): number[] {
    const pages: number[] = [];
    const start = Math.max(1, this.currentPage - 2);
    const end = Math.min(this.totalPages, this.currentPage + 2);
    for (let i = start; i <= end; i++) {
      pages.push(i);
    }
    return pages;
  }
}
