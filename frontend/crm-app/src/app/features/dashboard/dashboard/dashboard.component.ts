import { Component, OnInit } from '@angular/core';
import { DashboardService, DashboardStats, ProductionDashboard, QualityDashboard, DeliveryDashboard } from '../../../core/services/dashboard.service';
import { AuthService, RoleNames } from '../../../core/services/auth.service';

type DashboardType = 'sales' | 'production' | 'quality' | 'delivery';

@Component({
  selector: 'app-dashboard',
  templateUrl: './dashboard.component.html',
  styleUrls: ['./dashboard.component.scss']
})
export class DashboardComponent implements OnInit {
  // General stats (for sales/admin)
  stats: DashboardStats | null = null;

  // Role-specific dashboard data
  productionData: ProductionDashboard | null = null;
  qualityData: QualityDashboard | null = null;
  deliveryData: DeliveryDashboard | null = null;

  isLoading = true;
  dashboardType: DashboardType = 'sales';

  constructor(
    private dashboardService: DashboardService,
    private authService: AuthService
  ) {}

  ngOnInit(): void {
    this.determineDashboardType();
    this.loadDashboard();
  }

  private determineDashboardType(): void {
    const primaryRole = this.authService.getPrimaryRole();

    switch (primaryRole) {
      case RoleNames.ProductionManager:
        this.dashboardType = 'production';
        break;
      case RoleNames.QualityControl:
        this.dashboardType = 'quality';
        break;
      case RoleNames.DeliveryManager:
        this.dashboardType = 'delivery';
        break;
      default:
        this.dashboardType = 'sales';
        break;
    }
  }

  private loadDashboard(): void {
    this.isLoading = true;

    switch (this.dashboardType) {
      case 'production':
        this.loadProductionDashboard();
        break;
      case 'quality':
        this.loadQualityDashboard();
        break;
      case 'delivery':
        this.loadDeliveryDashboard();
        break;
      default:
        this.loadSalesDashboard();
        break;
    }
  }

  private loadSalesDashboard(): void {
    this.dashboardService.getStats().subscribe({
      next: (data) => {
        this.stats = data;
        this.isLoading = false;
      },
      error: () => {
        this.isLoading = false;
      }
    });
  }

  private loadProductionDashboard(): void {
    this.dashboardService.getProductionDashboard().subscribe({
      next: (data) => {
        this.productionData = data;
        this.isLoading = false;
      },
      error: () => {
        this.isLoading = false;
      }
    });
  }

  private loadQualityDashboard(): void {
    this.dashboardService.getQualityDashboard().subscribe({
      next: (data) => {
        this.qualityData = data;
        this.isLoading = false;
      },
      error: () => {
        this.isLoading = false;
      }
    });
  }

  private loadDeliveryDashboard(): void {
    this.dashboardService.getDeliveryDashboard().subscribe({
      next: (data) => {
        this.deliveryData = data;
        this.isLoading = false;
      },
      error: () => {
        this.isLoading = false;
      }
    });
  }

  formatCurrency(value: number): string {
    return new Intl.NumberFormat('vi-VN', {
      style: 'currency',
      currency: 'VND'
    }).format(value);
  }

  // Helper methods for role-specific dashboards
  isSalesDashboard(): boolean {
    return this.dashboardType === 'sales';
  }

  isProductionDashboard(): boolean {
    return this.dashboardType === 'production';
  }

  isQualityDashboard(): boolean {
    return this.dashboardType === 'quality';
  }

  isDeliveryDashboard(): boolean {
    return this.dashboardType === 'delivery';
  }
}
