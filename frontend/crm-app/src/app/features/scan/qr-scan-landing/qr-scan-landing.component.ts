import { Component, OnInit } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import { AuthService } from '../../../core/services/auth.service';
import {
  ProductionService,
  OrderProductionProgress,
  OrderProductionStep
} from '../../../core/services/production.service';

@Component({
  selector: 'app-qr-scan-landing',
  templateUrl: './qr-scan-landing.component.html',
  styleUrls: ['./qr-scan-landing.component.scss']
})
export class QrScanLandingComponent implements OnInit {
  token = '';
  progress: OrderProductionProgress | null = null;
  isLoading = true;
  isConfirming = false;
  errorMessage = '';
  successMessage = '';
  currentUserRoles: string[] = [];
  confirmingStepId: string | null = null;
  confirmNote = '';

  constructor(
    private route: ActivatedRoute,
    private productionService: ProductionService,
    private authService: AuthService
  ) {}

  ngOnInit(): void {
    this.token = this.route.snapshot.paramMap.get('token') || '';
    this.currentUserRoles = this.authService.getCurrentUser()?.roles || [];
    this.loadProgress();
  }

  loadProgress(): void {
    this.isLoading = true;
    this.errorMessage = '';
    this.productionService.getProgressByToken(this.token).subscribe({
      next: (p) => { this.progress = p; this.isLoading = false; },
      error: (err) => {
        this.errorMessage = err.error?.message || 'Không thể tải thông tin đơn hàng.';
        this.isLoading = false;
      }
    });
  }

  canCompleteStep(step: OrderProductionStep): boolean {
    if (step.isCompleted) return false;
    // Chỉ khâu đầu tiên chưa hoàn thành mới có thể confirm
    const firstPending = this.progress?.steps.find(s => !s.isCompleted);
    if (firstPending?.id !== step.id) return false;
    if (!step.responsibleRole) return true;
    return this.currentUserRoles.includes(step.responsibleRole)
      || this.currentUserRoles.includes('Admin')
      || this.currentUserRoles.includes('ProductionManager');
  }

  startConfirm(step: OrderProductionStep): void {
    this.confirmingStepId = step.id;
    this.confirmNote = '';
    this.successMessage = '';
  }

  cancelConfirm(): void {
    this.confirmingStepId = null;
    this.confirmNote = '';
  }

  confirmStep(step: OrderProductionStep): void {
    this.isConfirming = true;
    this.productionService.completeStepByToken(this.token, step.productionStageId, { notes: this.confirmNote || undefined }).subscribe({
      next: () => {
        this.isConfirming = false;
        this.confirmingStepId = null;
        this.successMessage = `Đã hoàn thành khâu "${step.stageName}"!`;
        this.loadProgress();
      },
      error: (err) => {
        this.isConfirming = false;
        this.errorMessage = err.error?.message || 'Xác nhận thất bại.';
      }
    });
  }

  getRoleLabel(role?: string): string {
    return this.productionService.getRoleLabel(role);
  }

  getCurrentUser() {
    return this.authService.getCurrentUser();
  }
}
