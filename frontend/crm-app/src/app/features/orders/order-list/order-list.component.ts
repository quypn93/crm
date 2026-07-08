import { Component, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { OrderService, OrderSearchParams } from '../../../core/services/order.service';
import { AuthService } from '../../../core/services/auth.service';
import { ToastService } from '../../../core/services/toast.service';
import { Order, OrderStatus, PaymentStatus, OrderStatusLabels, PaymentStatusLabels, OrderStatusColors, PaymentStatusColors, UpdateOrderStatusRequest } from '../../../core/models/order.model';

@Component({
  selector: 'app-order-list',
  templateUrl: './order-list.component.html',
  styleUrls: ['./order-list.component.scss']
})
export class OrderListComponent implements OnInit {
  orders: Order[] = [];
  isLoading = false;
  isExporting = false;
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

  canCreateOrder = false;

  constructor(
    private orderService: OrderService,
    private authService: AuthService,
    private router: Router,
    private toast: ToastService
  ) {}

  ngOnInit(): void {
    this.canCreateOrder = this.authService.canCreateOrders();
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

  exportExcel(): void {
    if (this.isExporting) return;
    this.isExporting = true;

    // Lấy toàn bộ đơn theo bộ lọc hiện tại (không chỉ trang đang xem).
    const params: OrderSearchParams = {
      search: this.searchTerm,
      page: 1,
      pageSize: 100000
    };
    if (this.selectedStatus !== null) params.status = this.selectedStatus;
    if (this.selectedPaymentStatus !== null) params.paymentStatus = this.selectedPaymentStatus;

    this.orderService.getOrders(params).subscribe({
      next: (response) => {
        const items = response?.items || [];
        if (items.length === 0) {
          this.toast.error('Không có đơn hàng nào để xuất.');
          this.isExporting = false;
          return;
        }
        this.downloadCsv(items);
        this.toast.success(`Đã xuất ${items.length} đơn hàng.`);
        this.isExporting = false;
      },
      error: () => {
        this.toast.error('Không thể xuất dữ liệu.');
        this.isExporting = false;
      }
    });
  }

  private downloadCsv(orders: Order[]): void {
    const headers = ['Mã đơn', 'Số sản phẩm', 'Khách hàng', 'Trạng thái', 'Thanh toán', 'Tổng tiền', 'Còn nợ', 'Ngày tạo'];

    const escape = (v: any): string => {
      const s = v === null || v === undefined ? '' : String(v);
      return /[",\n;]/.test(s) ? `"${s.replace(/"/g, '""')}"` : s;
    };

    const formatDate = (d: any): string => {
      if (!d) return '';
      const date = new Date(d);
      if (isNaN(date.getTime())) return '';
      const dd = String(date.getDate()).padStart(2, '0');
      const mm = String(date.getMonth() + 1).padStart(2, '0');
      return `${dd}/${mm}/${date.getFullYear()}`;
    };

    const rows = orders.map(o => [
      o.orderNumber,
      o.itemsCount,
      o.customerName || 'N/A',
      this.getStatusLabel(o.status),
      this.getPaymentStatusLabel(o.paymentStatus),
      o.totalAmount ?? 0,
      (o.totalAmount ?? 0) - (o.paidAmount ?? 0),
      formatDate(o.createdAt)
    ]);

    const csv = [headers, ...rows]
      .map(row => row.map(escape).join(','))
      .join('\r\n');

    // BOM UTF-8 để Excel đọc đúng tiếng Việt.
    const blob = new Blob(['﻿' + csv], { type: 'text/csv;charset=utf-8;' });
    const url = URL.createObjectURL(blob);
    const now = new Date();
    const stamp = `${now.getFullYear()}${String(now.getMonth() + 1).padStart(2, '0')}${String(now.getDate()).padStart(2, '0')}`;
    const link = document.createElement('a');
    link.href = url;
    link.download = `don-hang-${stamp}.csv`;
    link.click();
    URL.revokeObjectURL(url);
  }

  viewOrder(id: string): void {
    this.router.navigate(['/orders', id]);
  }

  editOrder(id: string): void {
    this.router.navigate(['/orders', id, 'edit']);
  }

  deleteOrder(order: Order): void {
    if (!confirm(`Bạn có chắc muốn xóa đơn hàng "${order.orderNumber}"?\n\nChỉ có thể xóa đơn ở trạng thái Nháp.`)) return;
    this.orderService.deleteOrder(order.id).subscribe({
      next: () => {
        this.toast.success(`Đã xóa đơn hàng ${order.orderNumber}.`);
        this.loadOrders();
      },
      error: (err) => {
        const msg = err?.error?.message || 'Không thể xóa đơn hàng.';
        this.toast.error(msg);
      }
    });
  }

  cancelOrder(order: Order): void {
    if (!confirm(`Bạn có chắc muốn hủy đơn hàng "${order.orderNumber}"?\n\nTrạng thái sẽ chuyển sang "Đã hủy" và không thể hoàn tác.`)) return;
    const req: UpdateOrderStatusRequest = { status: OrderStatus.Cancelled, notes: 'Hủy đơn từ danh sách' };
    this.orderService.updateOrderStatus(order.id, req).subscribe({
      next: () => {
        this.toast.success(`Đã hủy đơn hàng ${order.orderNumber}.`);
        this.loadOrders();
      },
      error: (err) => {
        const msg = err?.error?.message || 'Không thể hủy đơn hàng.';
        this.toast.error(msg);
      }
    });
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

  canDeleteOrderInList(order: Order): boolean {
    // Chỉ cho xóa đơn ở trạng thái Nháp — khớp với ràng buộc backend.
    return order.status === OrderStatus.Draft &&
           this.authService.hasAnyRole(['Admin', 'SalesManager', 'SalesRep']);
  }

  canCancelOrderInList(order: Order): boolean {
    // Hủy thay vì xóa cho đơn đã Confirmed/InProduction. InProduction → chỉ Admin/SalesManager.
    if (order.status === OrderStatus.Confirmed) {
      return this.authService.hasAnyRole(['Admin', 'SalesManager', 'SalesRep']);
    }
    if (order.status === OrderStatus.InProduction) {
      return this.authService.hasAnyRole(['Admin', 'SalesManager']);
    }
    return false;
  }

  confirmOrder(order: Order): void {
    const req: UpdateOrderStatusRequest = { status: OrderStatus.Confirmed, notes: '' };
    this.orderService.updateOrderStatus(order.id, req).subscribe({
      next: () => this.loadOrders()
    });
  }

  moveToProduction(order: Order): void {
    const confirmed = confirm(
      `Chuyển đơn ${order.orderNumber} sang trạng thái "Đang sản xuất"?\n\n` +
      `Sau khi chuyển, đơn hàng sẽ KHÔNG thể chỉnh sửa được nữa.`
    );
    if (!confirmed) return;

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
