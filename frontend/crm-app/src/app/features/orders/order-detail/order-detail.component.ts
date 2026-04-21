import { Component, OnInit } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { forkJoin } from 'rxjs';
import { OrderService } from '../../../core/services/order.service';
import { AuthService } from '../../../core/services/auth.service';
import { ToastService } from '../../../core/services/toast.service';
import { ProductionService, OrderProductionProgress } from '../../../core/services/production.service';
import { environment } from '../../../../environments/environment';
import {
  Order,
  OrderStatus,
  PaymentStatus,
  OrderStatusLabels,
  PaymentStatusLabels,
  OrderStatusColors,
  PaymentStatusColors,
  UpdateOrderStatusRequest,
  UpdatePaymentRequest
} from '../../../core/models/order.model';

@Component({
  selector: 'app-order-detail',
  templateUrl: './order-detail.component.html',
  styleUrls: ['./order-detail.component.scss']
})
export class OrderDetailComponent implements OnInit {
  order: Order | null = null;
  isLoading = true;
  errorMessage = '';
  productionProgress: OrderProductionProgress | null = null;
  isGeneratingQr = false;

  // Status update modal
  showStatusModal = false;
  newStatus: OrderStatus | null = null;
  statusNotes = '';
  availableStatuses: OrderStatus[] = [];

  // Payment update modal
  showPaymentModal = false;
  paymentAmount = 0;
  paymentMethod = '';
  paymentNotes = '';

  // Complete production step modal
  showCompleteStepModal = false;
  completingStageId = '';
  completingStageName = '';
  completeStepNotes = '';
  isCompletingStep = false;
  completeStepError = '';

  constructor(
    private orderService: OrderService,
    private authService: AuthService,
    private productionService: ProductionService,
    private route: ActivatedRoute,
    private router: Router,
    private toast: ToastService
  ) {}

  ngOnInit(): void {
    const id = this.route.snapshot.paramMap.get('id');
    if (id) {
      this.loadOrder(id);
    }
  }

  loadOrder(id: string): void {
    this.isLoading = true;

    // Load order and allowed statuses in parallel
    forkJoin({
      order: this.orderService.getOrder(id),
      allowedStatuses: this.orderService.getAllowedStatuses(id)
    }).subscribe({
      next: (result) => {
        this.order = result.order;
        this.availableStatuses = result.allowedStatuses;
        this.isLoading = false;
        // Load production progress for any non-draft order
        if (result.order.status > OrderStatus.Draft) {
          this.loadProductionProgress(result.order.id);
        }
      },
      error: () => {
        this.errorMessage = 'Không thể tải thông tin đơn hàng';
        this.isLoading = false;
      }
    });
  }

  // Permission checks for UI
  canUpdateStatus(): boolean {
    return this.availableStatuses.length > 0;
  }

  canUpdatePayment(): boolean {
    return this.authService.canUpdatePayment();
  }

  canDeleteOrder(): boolean {
    // Chỉ cho xóa đơn Nháp (khớp ràng buộc backend)
    return this.order?.status === OrderStatus.Draft && this.authService.canDeleteOrders();
  }

  canCancelOrder(): boolean {
    if (!this.order) return false;
    if (this.order.status === OrderStatus.Confirmed) {
      return this.authService.hasAnyRole(['Admin', 'SalesManager', 'SalesRep']);
    }
    if (this.order.status === OrderStatus.InProduction) {
      return this.authService.hasAnyRole(['Admin', 'SalesManager']);
    }
    return false;
  }

  canEditOrder(): boolean {
    const editableStatuses = [OrderStatus.Draft, OrderStatus.Confirmed];
    return editableStatuses.includes(this.order?.status as OrderStatus) &&
           this.authService.hasAnyRole(['Admin', 'SalesManager', 'SalesRep']);
  }

  canConfirmOrder(): boolean {
    return this.order?.status === OrderStatus.Draft &&
           this.authService.hasAnyRole(['Admin', 'SalesManager', 'SalesRep']);
  }

  confirmOrder(): void {
    if (!this.order) return;
    const req: UpdateOrderStatusRequest = { status: OrderStatus.Confirmed, notes: 'Sale xác nhận đơn hàng' };
    this.orderService.updateOrderStatus(this.order.id, req).subscribe({
      next: (updated) => {
        this.order = updated;
        this.orderService.getAllowedStatuses(updated.id).subscribe(s => this.availableStatuses = s);
        this.loadProductionProgress(updated.id);
      },
      error: (err) => { this.errorMessage = err.error?.message || 'Xác nhận đơn thất bại'; }
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

  editOrder(): void {
    if (this.order) {
      this.router.navigate(['/orders', this.order.id, 'edit']);
    }
  }

  goBack(): void {
    this.router.navigate(['/orders']);
  }

  // Status Update
  openStatusModal(): void {
    if (this.availableStatuses.length > 0) {
      this.newStatus = this.availableStatuses[0];
      this.statusNotes = '';
      this.showStatusModal = true;
    }
  }

  closeStatusModal(): void {
    this.showStatusModal = false;
    this.newStatus = null;
    this.statusNotes = '';
  }

  updateStatus(): void {
    if (!this.order || this.newStatus === null) return;

    // Xác nhận trước khi chuyển sang InProduction — sau bước này không sửa được đơn nữa.
    if (this.newStatus === OrderStatus.InProduction) {
      const confirmed = confirm(
        `Chuyển đơn ${this.order.orderNumber} sang trạng thái "Đang sản xuất"?\n\n` +
        `Sau khi chuyển, đơn hàng sẽ KHÔNG thể chỉnh sửa được nữa.`
      );
      if (!confirmed) return;
    }

    const request: UpdateOrderStatusRequest = {
      status: this.newStatus,
      notes: this.statusNotes
    };

    this.orderService.updateOrderStatus(this.order.id, request).subscribe({
      next: (updatedOrder) => {
        this.order = updatedOrder;
        // Reload allowed statuses from API
        this.orderService.getAllowedStatuses(updatedOrder.id).subscribe({
          next: (statuses) => {
            this.availableStatuses = statuses;
          }
        });
        this.closeStatusModal();
      },
      error: (error) => {
        this.errorMessage = error.error?.message || 'Cập nhật trạng thái thất bại';
      }
    });
  }

  // Payment Update
  openPaymentModal(): void {
    this.paymentAmount = this.order?.remainingAmount || 0;
    this.paymentMethod = '';
    this.paymentNotes = '';
    this.showPaymentModal = true;
  }

  closePaymentModal(): void {
    this.showPaymentModal = false;
    this.paymentAmount = 0;
    this.paymentMethod = '';
    this.paymentNotes = '';
  }

  updatePayment(): void {
    if (!this.order) return;

    const request: UpdatePaymentRequest = {
      paidAmount: this.paymentAmount,
      paymentMethod: this.paymentMethod,
      paymentNotes: this.paymentNotes
    };

    this.orderService.updatePayment(this.order.id, request).subscribe({
      next: (updatedOrder) => {
        this.order = updatedOrder;
        this.closePaymentModal();
      },
      error: (error) => {
        this.errorMessage = error.error?.message || 'Cập nhật thanh toán thất bại';
      }
    });
  }

  deleteOrder(): void {
    if (!this.order) return;
    const orderNumber = this.order.orderNumber;
    if (!confirm(`Bạn có chắc muốn xóa đơn hàng "${orderNumber}"?\n\nChỉ có thể xóa đơn ở trạng thái Nháp.`)) return;
    this.orderService.deleteOrder(this.order.id).subscribe({
      next: () => {
        this.toast.success(`Đã xóa đơn hàng ${orderNumber}.`);
        this.router.navigate(['/orders']);
      },
      error: (error) => {
        const msg = error?.error?.message || 'Xóa đơn hàng thất bại';
        this.errorMessage = msg;
        this.toast.error(msg);
      }
    });
  }

  cancelOrder(): void {
    if (!this.order) return;
    const orderNumber = this.order.orderNumber;
    if (!confirm(`Bạn có chắc muốn hủy đơn hàng "${orderNumber}"?\n\nTrạng thái sẽ chuyển sang "Đã hủy" và không thể hoàn tác.`)) return;
    const req: UpdateOrderStatusRequest = { status: OrderStatus.Cancelled, notes: 'Hủy đơn từ trang chi tiết' };
    const orderId = this.order.id;
    this.orderService.updateOrderStatus(orderId, req).subscribe({
      next: () => {
        this.toast.success(`Đã hủy đơn hàng ${orderNumber}.`);
        this.loadOrder(orderId);
      },
      error: (error) => {
        const msg = error?.error?.message || 'Hủy đơn hàng thất bại';
        this.errorMessage = msg;
        this.toast.error(msg);
      }
    });
  }

  generateQr(): void {
    if (!this.order) return;
    this.isGeneratingQr = true;
    this.orderService.generateQr(this.order.id).subscribe({
      next: (updated) => { this.order = updated; this.isGeneratingQr = false; },
      error: () => { this.isGeneratingQr = false; alert('Không thể tạo QR code.'); }
    });
  }

  canCompleteProductionStep(): boolean {
    return this.authService.hasAnyRole(['Admin', 'ProductionManager', 'ProductionStaff', 'QualityControl', 'QualityManager']);
  }

  get nextProductionStep() {
    return this.productionProgress?.steps?.find(s => !s.isCompleted) ?? null;
  }

  openCompleteStepModal(stageId: string, stageName: string): void {
    this.completingStageId = stageId;
    this.completingStageName = stageName;
    this.completeStepNotes = '';
    this.completeStepError = '';
    this.showCompleteStepModal = true;
  }

  closeCompleteStepModal(): void {
    this.showCompleteStepModal = false;
    this.completingStageId = '';
    this.completingStageName = '';
  }

  confirmCompleteStep(): void {
    if (!this.order || !this.completingStageId) return;
    this.isCompletingStep = true;
    this.completeStepError = '';
    this.productionService.completeStep(this.order.id, this.completingStageId, { notes: this.completeStepNotes }).subscribe({
      next: () => {
        this.isCompletingStep = false;
        this.closeCompleteStepModal();
        this.loadProductionProgress(this.order!.id);
      },
      error: (err) => {
        this.isCompletingStep = false;
        this.completeStepError = err.error?.message || 'Hoàn thành thất bại.';
      }
    });
  }

  loadProductionProgress(orderId: string): void {
    this.productionService.getOrderProgress(orderId).subscribe({
      next: (p) => { this.productionProgress = p; },
      error: () => { /* not critical */ }
    });
  }

  printQr(): void {
    if (!this.order?.qrCodeImageBase64) return;
    const w = window.open('', '_blank');
    if (!w) return;
    w.document.write(`
      <html><head><title>QR - ${this.order.orderNumber}</title>
      <style>body{display:flex;flex-direction:column;align-items:center;justify-content:center;height:100vh;margin:0;font-family:sans-serif;}
      img{width:200px;height:200px;} p{font-size:18px;font-weight:bold;margin-top:8px;}</style></head>
      <body>
        <img src="data:image/png;base64,${this.order.qrCodeImageBase64}">
        <p>${this.order.orderNumber}</p>
        <script>window.onload=()=>{window.print();window.close();}</script>
      </body></html>`);
    w.document.close();
  }

  // Designer upload
  isUploadingDesign = false;
  designUploadError = '';

  canUploadDesignImage(): boolean {
    return this.authService.hasAnyRole(['Admin', 'Designer']);
  }

  onDesignImageSelected(event: Event): void {
    const input = event.target as HTMLInputElement;
    const file = input.files?.[0];
    if (!file || !this.order) return;
    this.isUploadingDesign = true;
    this.designUploadError = '';
    this.orderService.uploadDesignImage(this.order.id, file).subscribe({
      next: (updated) => {
        this.order = updated;
        this.isUploadingDesign = false;
        input.value = '';
        this.toast.success('Đã cập nhật ảnh thiết kế.');
      },
      error: (err) => {
        this.isUploadingDesign = false;
        const msg = err?.error?.message || 'Upload thất bại.';
        this.designUploadError = msg;
        this.toast.error(msg);
      }
    });
  }

  apiOrigin(): string {
    // Static files (ví dụ /uploads/designs/...) được API serve cùng origin với apiUrl (bỏ đuôi /api).
    const url = environment.apiUrl || '';
    return url.replace(/\/api\/?$/, '');
  }

  resolveImageUrl(path?: string): string {
    if (!path) return '';
    if (path.startsWith('http')) return path;
    // path có thể là '/uploads/...' → prepend origin của API
    const origin = this.apiOrigin();
    return origin + (path.startsWith('/') ? path : '/' + path);
  }

  printOrder(): void {
    window.print();
  }
}
