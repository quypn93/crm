import { Component, OnInit } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { AuthService } from '../../../core/services/auth.service';
import { OrderService } from '../../../core/services/order.service';
import { Order } from '../../../core/models/order.model';
import { environment } from '../../../../environments/environment';
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
  publicOrder: Order | null = null;
  isLoading = true;
  isConfirming = false;
  errorMessage = '';
  successMessage = '';
  currentUserRoles: string[] = [];
  confirmingStepId: string | null = null;
  confirmNote = '';
  isAuthenticated = false;

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private productionService: ProductionService,
    private orderService: OrderService,
    private authService: AuthService
  ) {}

  goToStaffLogin(): void {
    this.router.navigate(['/auth/login'], {
      queryParams: { returnUrl: `/scan/${this.token}` }
    });
  }

  logout(): void {
    if (!confirm('Bạn có chắc muốn đăng xuất?')) return;
    this.authService.logout();
    // Reload trạng thái trên trang scan hiện tại (giữ URL token).
    this.isAuthenticated = false;
    this.currentUserRoles = [];
    this.progress = null;
  }

  ngOnInit(): void {
    this.token = this.route.snapshot.paramMap.get('token')
              || this.route.parent?.snapshot.paramMap.get('token')
              || '';
    this.isAuthenticated = !!this.authService.getCurrentUser();
    this.currentUserRoles = this.authService.getCurrentUser()?.roles || [];
    this.isLoading = false;
    this.loadPublicOrder();
    if (this.isAuthenticated) this.loadProgress();
  }

  loadPublicOrder(): void {
    this.orderService.getPublicByToken(this.token).subscribe({
      next: (o) => { this.publicOrder = o; },
      error: () => { /* token invalid */ }
    });
  }

  loadProgress(): void {
    this.errorMessage = '';
    this.productionService.getProgressByToken(this.token).subscribe({
      next: (p) => { this.progress = p; },
      error: () => { /* silently ignore — token hết hạn hoặc không có quyền */ }
    });
  }

  resolveImageUrl(path?: string): string {
    if (!path) return '';
    if (path.startsWith('http')) return path;
    return environment.apiUrl.replace(/\/api\/?$/, '') + path;
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
