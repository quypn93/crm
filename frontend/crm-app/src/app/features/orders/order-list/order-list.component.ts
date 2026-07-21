import { Component, OnInit } from '@angular/core';
import { Router } from '@angular/router';
import { forkJoin } from 'rxjs';
import * as ExcelJS from 'exceljs';
import { OrderService, OrderSearchParams } from '../../../core/services/order.service';
import { AuthService } from '../../../core/services/auth.service';
import { ToastService } from '../../../core/services/toast.service';
import { UserManagementService, UserListItem } from '../../../core/services/user-management.service';
import { Order, OrderStatus, PaymentStatus, OrderStatusLabels, PaymentStatusLabels, OrderStatusColors, PaymentStatusColors, UpdateOrderStatusRequest } from '../../../core/models/order.model';

// Một trường có thể chọn khi xuất dữ liệu đơn hàng
interface ExportField {
  key: string;
  label: string;
  money?: boolean;
  value: (o: Order) => string | number;
}

@Component({
  selector: 'app-order-list',
  templateUrl: './order-list.component.html',
  styleUrls: ['./order-list.component.scss']
})
export class OrderListComponent implements OnInit {
  orders: Order[] = [];
  isLoading = false;
  isExporting = false;
  showExportMenu = false;
  searchTerm = '';
  currentPage = 1;
  pageSize = 10;
  totalItems = 0;
  totalPages = 0;

  // Filters
  selectedStatus: OrderStatus | null = null;
  selectedPaymentStatus: PaymentStatus | null = null;
  dateFrom = '';
  dateTo = '';
  customerNameFilter = '';
  selectedCreatedBy = '';
  minQuantity: number | null = null;
  maxQuantity: number | null = null;

  statusOptions = this.orderService.getStatusOptions();
  paymentStatusOptions = this.orderService.getPaymentStatusOptions();
  creatorOptions: UserListItem[] = [];

  readonly OrderStatus = OrderStatus;

  canCreateOrder = false;
  // "Tài khoản tổng" (Admin/SalesManager) mới thấy filter Nhân viên tạo đơn
  isManagerAccount = false;

  constructor(
    private orderService: OrderService,
    private authService: AuthService,
    private userService: UserManagementService,
    private router: Router,
    private toast: ToastService
  ) {}

  ngOnInit(): void {
    this.canCreateOrder = this.authService.canCreateOrders();
    this.isManagerAccount = this.authService.hasAnyRole(['Admin', 'SalesManager']);
    if (this.isManagerAccount) {
      // Chỉ liệt kê người có khả năng tạo đơn (Admin, SalesManager, SalesRep)
      const creatorRoles = ['Admin', 'SalesManager', 'SalesRep'];
      forkJoin(creatorRoles.map(role =>
        this.userService.getUsers({ page: 1, pageSize: 200, isActive: true, role })
      )).subscribe({
        next: (results) => {
          const byId = new Map<string, UserListItem>();
          results.forEach(res => (res?.items || []).forEach(u => byId.set(u.id, u)));
          this.creatorOptions = Array.from(byId.values())
            .sort((a, b) => (a.fullName || '').localeCompare(b.fullName || '', 'vi'));
        },
        error: () => { /* not critical */ }
      });
    }
    this.loadOrders();
  }

  private buildFilterParams(): OrderSearchParams {
    const params: OrderSearchParams = { search: this.searchTerm };

    if (this.selectedStatus !== null) params.status = this.selectedStatus;
    if (this.selectedPaymentStatus !== null) params.paymentStatus = this.selectedPaymentStatus;
    // Ngày chọn là ngày địa phương → quy đổi mốc đầu/cuối ngày sang UTC (CreatedAt lưu UTC)
    if (this.dateFrom) params.orderDateFrom = new Date(`${this.dateFrom}T00:00:00`).toISOString();
    if (this.dateTo) params.orderDateTo = new Date(`${this.dateTo}T23:59:59.999`).toISOString();
    if (this.customerNameFilter.trim()) params.customerName = this.customerNameFilter.trim();
    if (this.isManagerAccount && this.selectedCreatedBy) params.createdBy = this.selectedCreatedBy;
    if (this.minQuantity !== null && this.minQuantity !== undefined && String(this.minQuantity) !== '') params.minQuantity = this.minQuantity;
    if (this.maxQuantity !== null && this.maxQuantity !== undefined && String(this.maxQuantity) !== '') params.maxQuantity = this.maxQuantity;

    return params;
  }

  loadOrders(): void {
    this.isLoading = true;
    const params: OrderSearchParams = {
      ...this.buildFilterParams(),
      page: this.currentPage,
      pageSize: this.pageSize
    };

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
    this.dateFrom = '';
    this.dateTo = '';
    this.customerNameFilter = '';
    this.selectedCreatedBy = '';
    this.minQuantity = null;
    this.maxQuantity = null;
    this.currentPage = 1;
    this.loadOrders();
  }

  onPageChange(page: number): void {
    this.currentPage = page;
    this.loadOrders();
  }

  toggleExportMenu(): void {
    this.showExportMenu = !this.showExportMenu;
  }

  // ===== Chọn cột xuất dữ liệu =====
  // Danh mục tất cả các trường có thể xuất; thứ tự cột trong file = thứ tự chọn.
  readonly exportFieldCatalog: ExportField[] = [
    { key: 'orderNumber', label: 'Mã đơn', value: o => o.orderNumber },
    { key: 'itemsCount', label: 'Số sản phẩm', value: o => o.itemsCount },
    { key: 'customerName', label: 'Khách hàng', value: o => o.customerName || 'N/A' },
    { key: 'createdByUserName', label: 'Người tạo đơn', value: o => o.createdByUserName || '' },
    { key: 'designerUserName', label: 'NV thiết kế', value: o => o.designerUserName || '' },
    { key: 'shipperUserName', label: 'NV giao hàng', value: o => o.shipperUserName || '' },
    { key: 'status', label: 'Trạng thái', value: o => this.getStatusLabel(o.status) },
    { key: 'paymentStatus', label: 'Thanh toán', value: o => this.getPaymentStatusLabel(o.paymentStatus) },
    { key: 'subTotal', label: 'Tiền hàng', money: true, value: o => o.subTotal ?? 0 },
    { key: 'discountAmount', label: 'Giảm giá', money: true, value: o => o.discountAmount ?? 0 },
    { key: 'taxAmount', label: 'VAT', money: true, value: o => o.taxAmount ?? 0 },
    { key: 'shippingFee', label: 'Phí vận chuyển', money: true, value: o => o.shippingFee ?? 0 },
    { key: 'totalAmount', label: 'Tổng tiền', money: true, value: o => o.totalAmount ?? 0 },
    { key: 'paidAmount', label: 'Đã thanh toán', money: true, value: o => o.paidAmount ?? 0 },
    { key: 'remaining', label: 'Còn nợ', money: true, value: o => (o.totalAmount ?? 0) - (o.paidAmount ?? 0) },
    { key: 'depositCode', label: 'Mã cọc tiền', value: o => o.depositCode || '' },
    { key: 'orderTypeName', label: 'Dạng đơn', value: o => o.orderTypeName || '' },
    { key: 'deliveryMethodName', label: 'Hình thức giao', value: o => o.deliveryMethodName || '' },
    { key: 'shippingContactName', label: 'Người nhận', value: o => o.shippingContactName || '' },
    { key: 'shippingPhone', label: 'SĐT người nhận', value: o => o.shippingPhone || '' },
    { key: 'shippingAddress', label: 'Địa chỉ giao', value: o => o.shippingAddress || '' },
    { key: 'productionDaysOptionName', label: 'Thời gian SX', value: o => o.productionDaysOptionName || '' },
    { key: 'createdAt', label: 'Ngày tạo', value: o => this.formatDateShort(o.createdAt) },
    { key: 'completionDate', label: 'Ngày xong', value: o => this.formatDateShort(o.completionDate) },
    { key: 'returnDate', label: 'Ngày trả hàng', value: o => this.formatDateShort(o.returnDate) },
    { key: 'notes', label: 'Ghi chú', value: o => o.notes || '' }
  ];

  private static readonly EXPORT_FIELDS_STORAGE_KEY = 'crm.orderExportFields';
  private static readonly DEFAULT_EXPORT_FIELDS = [
    'orderNumber', 'itemsCount', 'customerName', 'createdByUserName',
    'status', 'paymentStatus', 'totalAmount', 'remaining', 'createdAt'
  ];

  showColumnPicker = false;
  pendingFormat: 'xlsx' | 'csv' = 'xlsx';
  selectedFieldKeys: string[] = [];
  fieldSearch = '';

  get filteredCatalog(): ExportField[] {
    const term = this.fieldSearch.trim().toLowerCase();
    if (!term) return this.exportFieldCatalog;
    return this.exportFieldCatalog.filter(f => f.label.toLowerCase().includes(term));
  }

  get selectedFields(): ExportField[] {
    return this.selectedFieldKeys
      .map(k => this.exportFieldCatalog.find(f => f.key === k)!)
      .filter(Boolean);
  }

  get allFilteredSelected(): boolean {
    return this.filteredCatalog.length > 0 && this.filteredCatalog.every(f => this.selectedFieldKeys.includes(f.key));
  }

  isFieldSelected(key: string): boolean { return this.selectedFieldKeys.includes(key); }

  toggleField(key: string): void {
    const i = this.selectedFieldKeys.indexOf(key);
    if (i >= 0) this.selectedFieldKeys.splice(i, 1);
    else this.selectedFieldKeys.push(key);
  }

  toggleAllFields(): void {
    if (this.allFilteredSelected) {
      const keys = new Set(this.filteredCatalog.map(f => f.key));
      this.selectedFieldKeys = this.selectedFieldKeys.filter(k => !keys.has(k));
    } else {
      this.filteredCatalog.forEach(f => {
        if (!this.selectedFieldKeys.includes(f.key)) this.selectedFieldKeys.push(f.key);
      });
    }
  }

  export(format: 'xlsx' | 'csv'): void {
    this.showExportMenu = false;
    if (this.isExporting) return;
    this.pendingFormat = format;
    this.fieldSearch = '';
    // Nhớ lựa chọn cột lần trước của người dùng.
    try {
      const saved = JSON.parse(localStorage.getItem(OrderListComponent.EXPORT_FIELDS_STORAGE_KEY) || 'null');
      this.selectedFieldKeys = Array.isArray(saved) && saved.length
        ? saved.filter((k: string) => this.exportFieldCatalog.some(f => f.key === k))
        : [...OrderListComponent.DEFAULT_EXPORT_FIELDS];
    } catch {
      this.selectedFieldKeys = [...OrderListComponent.DEFAULT_EXPORT_FIELDS];
    }
    this.showColumnPicker = true;
  }

  confirmExport(): void {
    const fields = this.selectedFields;
    if (!fields.length || this.isExporting) return;
    this.showColumnPicker = false;
    this.isExporting = true;
    try { localStorage.setItem(OrderListComponent.EXPORT_FIELDS_STORAGE_KEY, JSON.stringify(this.selectedFieldKeys)); } catch { /* noop */ }

    // Lấy toàn bộ đơn theo bộ lọc hiện tại (không chỉ trang đang xem).
    const params: OrderSearchParams = {
      ...this.buildFilterParams(),
      page: 1,
      pageSize: 100000
    };

    this.orderService.getOrders(params).subscribe({
      next: (response) => {
        const items = response?.items || [];
        if (items.length === 0) {
          this.toast.error('Không có đơn hàng nào để xuất.');
          this.isExporting = false;
          return;
        }
        const done = () => {
          this.toast.success(`Đã xuất ${items.length} đơn hàng.`);
          this.isExporting = false;
        };
        if (this.pendingFormat === 'xlsx') {
          this.downloadXlsx(items, fields).then(done).catch(() => {
            this.toast.error('Không thể tạo file Excel.');
            this.isExporting = false;
          });
        } else {
          this.downloadCsv(items, fields);
          done();
        }
      },
      error: () => {
        this.toast.error('Không thể xuất dữ liệu.');
        this.isExporting = false;
      }
    });
  }

  private buildRows(orders: Order[], fields: ExportField[]): (string | number)[][] {
    return orders.map(o => fields.map(f => f.value(o)));
  }

  private formatDateShort(d: any): string {
    if (!d) return '';
    const date = new Date(d);
    if (isNaN(date.getTime())) return '';
    const dd = String(date.getDate()).padStart(2, '0');
    const mm = String(date.getMonth() + 1).padStart(2, '0');
    return `${dd}/${mm}/${date.getFullYear()}`;
  }

  private fileStamp(): string {
    const now = new Date();
    return `${now.getFullYear()}${String(now.getMonth() + 1).padStart(2, '0')}${String(now.getDate()).padStart(2, '0')}`;
  }

  private triggerDownload(blob: Blob, filename: string): void {
    const url = URL.createObjectURL(blob);
    const link = document.createElement('a');
    link.href = url;
    link.download = filename;
    link.click();
    URL.revokeObjectURL(url);
  }

  private async downloadXlsx(orders: Order[], fields: ExportField[]): Promise<void> {
    const workbook = new ExcelJS.Workbook();
    const sheet = workbook.addWorksheet('Đơn hàng');

    const headerRow = sheet.addRow(fields.map(f => f.label));
    headerRow.eachCell(cell => {
      cell.font = { bold: true, color: { argb: 'FFFFFFFF' } };
      cell.fill = { type: 'pattern', pattern: 'solid', fgColor: { argb: 'FF6D5AE6' } };
      cell.alignment = { vertical: 'middle', horizontal: 'center' };
    });

    this.buildRows(orders, fields).forEach(r => sheet.addRow(r));

    fields.forEach((f, i) => {
      const col = sheet.getColumn(i + 1);
      if (f.money) col.numFmt = '#,##0" ₫"';
      col.width = f.money ? 16 : Math.max(12, Math.min(30, f.label.length + 8));
    });

    const buffer = await workbook.xlsx.writeBuffer();
    const blob = new Blob([buffer], {
      type: 'application/vnd.openxmlformats-officedocument.spreadsheetml.sheet'
    });
    this.triggerDownload(blob, `don-hang-${this.fileStamp()}.xlsx`);
  }

  private downloadCsv(orders: Order[], fields: ExportField[]): void {
    const escape = (v: any): string => {
      const s = v === null || v === undefined ? '' : String(v);
      return /[",\n;]/.test(s) ? `"${s.replace(/"/g, '""')}"` : s;
    };

    const csv = [fields.map(f => f.label), ...this.buildRows(orders, fields)]
      .map(row => row.map(escape).join(','))
      .join('\r\n');

    // BOM UTF-8 để Excel đọc đúng tiếng Việt.
    const blob = new Blob(['﻿' + csv], { type: 'text/csv;charset=utf-8;' });
    this.triggerDownload(blob, `don-hang-${this.fileStamp()}.csv`);
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
      next: () => this.loadOrders(),
      error: (err) => {
        this.toast.error(err?.error?.message || 'Xác nhận đơn thất bại.');
      }
    });
  }

  moveToProduction(order: Order): void {
    // Gửi xưởng: phải có đủ cả ảnh VÀ file thiết kế (backend cũng chặn)
    if (!order.designImageUrl || !order.designFileUrl) {
      const noImage = !order.designImageUrl;
      const noFile = !order.designFileUrl;
      const missing = noImage && noFile ? 'ảnh và file thiết kế' : (noImage ? 'ảnh thiết kế' : 'file thiết kế');
      this.toast.error(`Đơn ${order.orderNumber} còn thiếu ${missing}. Thiết kế cần upload đủ cả ảnh và file trước khi gửi xưởng.`);
      return;
    }

    const confirmed = confirm(
      `Chuyển đơn ${order.orderNumber} sang trạng thái "Đang sản xuất"?\n\n` +
      `Sau khi chuyển, đơn hàng sẽ KHÔNG thể chỉnh sửa được nữa.`
    );
    if (!confirmed) return;

    const req: UpdateOrderStatusRequest = { status: OrderStatus.InProduction, notes: '' };
    this.orderService.updateOrderStatus(order.id, req).subscribe({
      next: () => {
        this.toast.success(`Đơn ${order.orderNumber} đã chuyển sang sản xuất.`);
        this.loadOrders();
      },
      error: (err) => {
        this.toast.error(err?.error?.message || 'Chuyển sang sản xuất thất bại.');
      }
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
