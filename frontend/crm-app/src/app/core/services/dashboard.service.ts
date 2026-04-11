import { Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { ApiService } from './api.service';

export interface DashboardStats {
  totalCustomers: number;
  totalDeals: number;
  totalTasks: number;
  totalRevenue: number;
  wonDealsCount: number;
  lostDealsCount: number;
  pendingTasksCount: number;
  overdueTasksCount: number;
  conversionRate: number;
  newCustomersThisMonth: number;
  dealsInPipeline: number;
  pipelineValue: number;
}

export interface RecentOrder {
  id: string;
  orderNumber: string;
  customerName: string;
  status: number;
  statusName: string;
  orderDate: string;
  requiredDate?: string;
  totalAmount: number;
  totalItems: number;
  deliveryAddress?: string;
  deliveryPhone?: string;
}

export interface OrderStatusCount {
  status: number;
  statusName: string;
  count: number;
}

export interface ProductionDashboard {
  ordersWaitingProduction: number;
  ordersInProduction: number;
  ordersCompletedToday: number;
  totalItemsInProduction: number;
  averageProductionDays: number;
  statusBreakdown: OrderStatusCount[];
  recentOrders: RecentOrder[];
}

export interface QualityDashboard {
  ordersWaitingQC: number;
  ordersPassedToday: number;
  ordersFailedToday: number;
  passRate: number;
  pendingQCOrders: RecentOrder[];
}

export interface DeliveryDashboard {
  ordersReadyToShip: number;
  ordersShipping: number;
  ordersDeliveredToday: number;
  totalValueShipping: number;
  pendingDeliveryOrders: RecentOrder[];
}

@Injectable({
  providedIn: 'root'
})
export class DashboardService {
  constructor(private api: ApiService) {}

  // General dashboard
  getStats(): Observable<DashboardStats> {
    return this.api.get<DashboardStats>('dashboard');
  }

  getMyStats(): Observable<DashboardStats> {
    return this.api.get<DashboardStats>('dashboard/my-stats');
  }

  // Role-specific dashboards
  getProductionDashboard(): Observable<ProductionDashboard> {
    return this.api.get<ProductionDashboard>('dashboard/production');
  }

  getQualityDashboard(): Observable<QualityDashboard> {
    return this.api.get<QualityDashboard>('dashboard/quality');
  }

  getDeliveryDashboard(): Observable<DeliveryDashboard> {
    return this.api.get<DeliveryDashboard>('dashboard/delivery');
  }
}
