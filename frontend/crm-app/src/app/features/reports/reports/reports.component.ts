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

  // Fallback palette for industries (which have no server-provided color)
  private readonly industryPalette = [
    '#6366f1', '#8b5cf6', '#06b6d4', '#10b981',
    '#f59e0b', '#ef4444', '#ec4899', '#14b8a6',
    '#3b82f6', '#a855f7'
  ];

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

  formatCompact(value: number): string {
    if (value >= 1_000_000_000) return (value / 1_000_000_000).toFixed(1).replace(/\.0$/, '') + 'B';
    if (value >= 1_000_000) return (value / 1_000_000).toFixed(1).replace(/\.0$/, '') + 'M';
    if (value >= 1_000) return (value / 1_000).toFixed(1).replace(/\.0$/, '') + 'K';
    return value.toString();
  }

  getBarHeight(value: number): number {
    if (!this.revenueByMonth?.length) return 0;
    const maxValue = Math.max(...this.revenueByMonth.map(r => r.revenue));
    return maxValue > 0 ? (value / maxValue) * 100 : 0;
  }

  // ---- Summary KPIs ----
  get totalDeals(): number {
    return this.dealsByStage.reduce((sum, s) => sum + s.count, 0);
  }

  get totalDealValue(): number {
    return this.dealsByStage.reduce((sum, s) => sum + s.totalValue, 0);
  }

  get totalCustomers(): number {
    return this.customersByIndustry.reduce((sum, s) => sum + s.count, 0);
  }

  get totalRevenue(): number {
    return this.revenueByMonth.reduce((sum, r) => sum + r.revenue, 0);
  }

  get avgDealValue(): number {
    return this.totalDeals > 0 ? this.totalDealValue / this.totalDeals : 0;
  }

  // ---- Donut chart helpers ----
  industryColor(index: number): string {
    return this.industryPalette[index % this.industryPalette.length];
  }

  dealsDonutBackground(): string {
    return this.buildConicGradient(
      this.dealsByStage.map(d => ({ value: d.count, color: d.stageColor || '#cbd5e1' }))
    );
  }

  industryDonutBackground(): string {
    return this.buildConicGradient(
      this.customersByIndustry.map((c, i) => ({ value: c.count, color: this.industryColor(i) }))
    );
  }

  private buildConicGradient(slices: { value: number; color: string }[]): string {
    const total = slices.reduce((s, x) => s + x.value, 0);
    if (total <= 0) {
      return 'conic-gradient(var(--gray-200) 0deg 360deg)';
    }
    let acc = 0;
    const parts: string[] = [];
    for (const slice of slices) {
      const start = (acc / total) * 360;
      acc += slice.value;
      const end = (acc / total) * 360;
      parts.push(`${slice.color} ${start}deg ${end}deg`);
    }
    return `conic-gradient(${parts.join(', ')})`;
  }
}
