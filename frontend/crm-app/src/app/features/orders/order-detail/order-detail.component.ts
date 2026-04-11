import { Component, OnInit } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { forkJoin } from 'rxjs';
import { OrderService } from '../../../core/services/order.service';
import { AuthService } from '../../../core/services/auth.service';
import { ProductionService, OrderProductionProgress } from '../../../core/services/production.service';
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
    private router: Router
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
    return this.authService.canDeleteOrders();
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

    if (confirm(`Bạn có chắc muốn xóa đơn hàng "${this.order.orderNumber}"?`)) {
      this.orderService.deleteOrder(this.order.id).subscribe({
        next: () => {
          this.router.navigate(['/orders']);
        },
        error: (error) => {
          this.errorMessage = error.error?.message || 'Xóa đơn hàng thất bại';
        }
      });
    }
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

  parsePersonNames(json?: string): { size: string; names: string[] }[] {
    if (!json) return [];
    try {
      const obj = JSON.parse(json);
      return Object.entries(obj).map(([size, names]) => ({ size, names: names as string[] }));
    } catch { return []; }
  }

  parseGiftItems(json?: string): { description: string }[] {
    if (!json) return [];
    try { return JSON.parse(json); } catch { return [{ description: json }]; }
  }

  printOrder(): void {
    window.print();
  }
}
