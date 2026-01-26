export interface ApiResponse<T> {
  success: boolean;
  message?: string;
  data?: T;
  errors?: string[];
}

export interface PaginatedResult<T> {
  items: T[];
  totalCount: number;
  page: number;
  pageSize: number;
  totalPages: number;
  hasPreviousPage: boolean;
  hasNextPage: boolean;
}

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
  revenueGrowth: number;
}

export interface RevenueReport {
  period: string;
  revenue: number;
  dealsCount: number;
}

export interface DealsByStageReport {
  stageName: string;
  stageColor: string;
  count: number;
  totalValue: number;
}
