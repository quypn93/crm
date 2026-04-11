import { Component, OnInit } from '@angular/core';
import { ProductionService, OrderProductionProgress, OrderProductionStep } from '../../../core/services/production.service';
import { AuthService } from '../../../core/services/auth.service';

@Component({
  selector: 'app-production-dashboard',
  templateUrl: './production-dashboard.component.html',
  styleUrls: ['./production-dashboard.component.scss']
})
export class ProductionDashboardComponent implements OnInit {
  orders: OrderProductionProgress[] = [];
  isLoading = true;
  errorMessage = '';

  // Complete step modal
  showCompleteModal = false;
  completing: { order: OrderProductionProgress; step: OrderProductionStep } | null = null;
  completeNotes = '';
  isCompleting = false;
  completeError = '';

  constructor(
    private productionService: ProductionService,
    private authService: AuthService
  ) {}

  ngOnInit(): void { this.load(); }

  load(): void {
    this.isLoading = true;
    this.errorMessage = '';
    this.productionService.getDashboard().subscribe({
      next: (data) => { this.orders = data; this.isLoading = false; },
      error: () => { this.errorMessage = 'Không thể tải danh sách sản xuất.'; this.isLoading = false; }
    });
  }

  get inProgress(): OrderProductionProgress[] {
    return this.orders.filter(o => !o.isFullyCompleted);
  }

  get completed(): OrderProductionProgress[] {
    return this.orders.filter(o => o.isFullyCompleted);
  }

  // Trả về khâu tiếp theo chưa hoàn thành (để hiện nút)
  nextStep(order: OrderProductionProgress): OrderProductionStep | null {
    return order.steps.find(s => !s.isCompleted) ?? null;
  }

  openCompleteModal(order: OrderProductionProgress, step: OrderProductionStep): void {
    this.completing = { order, step };
    this.completeNotes = '';
    this.completeError = '';
    this.showCompleteModal = true;
  }

  closeCompleteModal(): void {
    this.showCompleteModal = false;
    this.completing = null;
  }

  confirmComplete(): void {
    if (!this.completing) return;
    this.isCompleting = true;
    this.completeError = '';
    const { order, step } = this.completing;
    this.productionService.completeStep(order.orderId, step.productionStageId, { notes: this.completeNotes }).subscribe({
      next: () => {
        this.isCompleting = false;
        this.closeCompleteModal();
        this.load();
      },
      error: (err) => {
        this.isCompleting = false;
        this.completeError = err.error?.message || 'Hoàn thành thất bại. Vui lòng thử lại.';
      }
    });
  }

  canComplete(): boolean {
    return this.authService.hasAnyRole(['Admin', 'ProductionManager', 'ProductionStaff', 'QualityControl', 'QualityManager']);
  }
}
