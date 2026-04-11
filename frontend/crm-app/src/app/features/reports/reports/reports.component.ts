import { Component, OnInit } from '@angular/core';
import { forkJoin } from 'rxjs';
import { ApiService } from '../../../core/services/api.service';

interface DealsByStageReport {
  stageName: string;
  stageColor: string;
  count: number;
  totalValue: number;
  percentage: number;
}

interface RevenueReport {
  month: string;
  year: number;
  revenue: number;
  dealCount: number;
}

interface CustomersByIndustry {
  industry: string;
  count: number;
  percentage: number;
}

@Component({
  selector: 'app-reports',
  templateUrl: './reports.component.html',
  styleUrls: ['./reports.component.scss']
})
export class ReportsComponent implements OnInit {
  dealsByStage: DealsByStageReport[] = [];
  revenueByMonth: RevenueReport[] = [];
  customersByIndustry: CustomersByIndustry[] = [];
  isLoading = true;

  constructor(private api: ApiService) {}

  ngOnInit(): void {
    this.loadReportData();
  }

  loadReportData(): void {
    forkJoin({
      dealsByStage: this.api.get<DealsByStageReport[]>('dashboard/deals-by-stage'),
      revenue: this.api.get<RevenueReport[]>('dashboard/revenue'),
      customersByIndustry: this.api.get<CustomersByIndustry[]>('dashboard/customers-by-industry')
    }).subscribe({
      next: (data) => {
        this.dealsByStage = data.dealsByStage || [];
        this.revenueByMonth = data.revenue || [];
        this.customersByIndustry = data.customersByIndustry || [];
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
      currency: 'VND',
      maximumFractionDigits: 0
    }).format(value);
  }

  getBarHeight(value: number): number {
    if (!this.revenueByMonth?.length) return 0;
    const maxValue = Math.max(...this.revenueByMonth.map(r => r.revenue));
    return maxValue > 0 ? (value / maxValue) * 100 : 0;
  }
}
