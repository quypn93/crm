import { Component, OnInit } from '@angular/core';
import { FinanceService } from '../../../core/services/finance.service';
import { ToastService } from '../../../core/services/toast.service';
import {
  MonthlyProfit, MonthlyProfitDetail, MonthlyProfitResult, OrderProfitResult
} from '../../../core/models/finance.model';

@Component({
  selector: 'app-profit-report',
  template: `
    <div class="page-container">
      <div class="page-header">
        <h1>Báo cáo lãi/lỗ</h1>
        <label class="basis">
          <span>Ghi nhận doanh thu theo</span>
          <select [(ngModel)]="revenueBasis" (change)="reload()">
            <option value="confirmed">Ngày xác nhận đơn</option>
            <option value="order">Ngày tạo đơn</option>
            <option value="completed">Ngày hoàn thành SX</option>
            <option value="delivered">Ngày giao hàng</option>
          </select>
        </label>
      </div>

      <div class="tabs">
        <button class="tab" [class.active]="tab === 'monthly'" (click)="switchTab('monthly')">Theo tháng</button>
        <button class="tab" [class.active]="tab === 'orders'" (click)="switchTab('orders')">Theo đơn hàng</button>
      </div>

      <!-- ══ TAB THEO THÁNG ══ -->
      <ng-container *ngIf="tab === 'monthly'">
        <div class="filter-bar">
          <label class="filter-date">
            <span>Năm</span>
            <select [(ngModel)]="year" (change)="loadMonthly()">
              <option *ngFor="let y of years" [value]="y">{{ y }}</option>
            </select>
          </label>
        </div>

        <div class="kpi-row">
          <div class="kpi"><span class="kpi-label">Doanh thu năm</span><span class="kpi-value">{{ monthly.total.revenue | number }} đ</span></div>
          <div class="kpi"><span class="kpi-label">Giá vốn</span><span class="kpi-value cost">{{ monthly.total.cogs | number }} đ</span></div>
          <div class="kpi"><span class="kpi-label">Nhân sự</span><span class="kpi-value cost">{{ monthly.total.payrollCost | number }} đ</span></div>
          <div class="kpi"><span class="kpi-label">Cố định</span><span class="kpi-value cost">{{ monthly.total.fixedCost | number }} đ</span></div>
          <div class="kpi total">
            <span class="kpi-label">Lãi ròng năm</span>
            <span class="kpi-value" [class.profit]="monthly.total.netProfit >= 0" [class.loss]="monthly.total.netProfit < 0">
              {{ monthly.total.netProfit | number }} đ
            </span>
          </div>
        </div>

        <div class="chart" *ngIf="maxAbsNet > 0">
          <div class="bar-col" *ngFor="let m of monthly.months" [title]="m.label + ': ' + (m.netProfit | number) + ' đ'">
            <div class="bar-area">
              <div class="bar" [class.profit-bar]="m.netProfit >= 0" [class.loss-bar]="m.netProfit < 0"
                   [style.height.%]="barHeight(m)"></div>
            </div>
            <span class="bar-label">{{ m.month }}</span>
          </div>
        </div>

        <div class="table-wrap">
          <table class="table">
            <thead>
              <tr>
                <th>Tháng</th>
                <th class="num">Số đơn</th>
                <th class="num">Doanh thu</th>
                <th class="num">Giá vốn</th>
                <th class="num">Lãi gộp</th>
                <th class="num">Nhân sự</th>
                <th class="num">Cố định</th>
                <th class="num">Lãi ròng</th>
                <th class="num">Biên %</th>
              </tr>
            </thead>
            <tbody>
              <tr *ngIf="loadingMonthly"><td colspan="9" class="empty">Đang tải...</td></tr>
              <tr *ngFor="let m of monthly.months" class="clickable" [class.selected]="detail?.summary?.month === m.month"
                  (click)="openDetail(m)">
                <td class="strong">{{ m.label }}</td>
                <td class="num">
                  {{ m.orderCount | number }}
                  <span class="badge missing" *ngIf="m.ordersWithoutCost > 0"
                        [title]="m.ordersWithoutCost + ' đơn chưa nhập cost'">{{ m.ordersWithoutCost }}</span>
                </td>
                <td class="num">{{ m.revenue | number }}</td>
                <td class="num cost">{{ m.cogs | number }}</td>
                <td class="num">{{ m.grossProfit | number }}</td>
                <td class="num cost">{{ m.payrollCost | number }}</td>
                <td class="num cost">{{ m.fixedCost | number }}</td>
                <td class="num strong" [class.profit]="m.netProfit >= 0" [class.loss]="m.netProfit < 0">
                  {{ m.netProfit | number }}
                </td>
                <td class="num" [class.profit]="m.netMargin >= 0" [class.loss]="m.netMargin < 0">{{ m.netMargin }}%</td>
              </tr>
            </tbody>
            <tfoot>
              <tr>
                <td class="strong">Cả năm</td>
                <td class="num">{{ monthly.total.orderCount | number }}</td>
                <td class="num">{{ monthly.total.revenue | number }}</td>
                <td class="num">{{ monthly.total.cogs | number }}</td>
                <td class="num">{{ monthly.total.grossProfit | number }}</td>
                <td class="num">{{ monthly.total.payrollCost | number }}</td>
                <td class="num">{{ monthly.total.fixedCost | number }}</td>
                <td class="num" [class.profit]="monthly.total.netProfit >= 0" [class.loss]="monthly.total.netProfit < 0">
                  {{ monthly.total.netProfit | number }}
                </td>
                <td class="num">{{ monthly.total.netMargin }}%</td>
              </tr>
            </tfoot>
          </table>
        </div>

        <!-- Bóc tách 1 tháng -->
        <div class="detail-panel" *ngIf="detail as d">
          <div class="detail-header">
            <h3>Chi tiết tháng {{ d.summary.label }}</h3>
            <button class="btn-close" (click)="detail = null">×</button>
          </div>
          <div class="detail-body">
            <div class="detail-col">
              <h4>Chi phí cố định theo đầu mục</h4>
              <div class="cat-row" *ngFor="let c of d.fixedByCategory">
                <span>{{ c.categoryName }}</span><span class="amount">{{ c.amount | number }}</span>
              </div>
              <p class="muted" *ngIf="!d.fixedByCategory.length">Không có chi phí cố định.</p>
            </div>
            <div class="detail-col">
              <h4>Chi phí nhân sự</h4>
              <div class="cat-row" *ngFor="let p of d.payrollEntries">
                <span>{{ p.employeeName }}</span><span class="amount">{{ p.totalAmount | number }}</span>
              </div>
              <p class="muted" *ngIf="!d.payrollEntries.length">Chưa nhập lương tháng này.</p>
            </div>
            <div class="detail-col wide">
              <h4>Đơn hàng trong tháng ({{ d.orders.length }})</h4>
              <div class="cat-row" *ngFor="let o of d.orders">
                <span>
                  <a [routerLink]="['/orders', o.orderId]" target="_blank">{{ o.orderNumber }}</a>
                  <span class="badge missing" *ngIf="!o.hasCost">chưa nhập cost</span>
                </span>
                <span class="amount" [class.profit]="o.profit >= 0" [class.loss]="o.profit < 0">{{ o.profit | number }}</span>
              </div>
              <p class="muted" *ngIf="!d.orders.length">Không có đơn nào.</p>
            </div>
          </div>
        </div>
      </ng-container>

      <!-- ══ TAB THEO ĐƠN HÀNG ══ -->
      <ng-container *ngIf="tab === 'orders'">
        <div class="filter-bar">
          <input type="text" class="filter-search" [(ngModel)]="search" (keyup.enter)="loadOrders()"
                 placeholder="Tìm mã đơn, tên khách hàng...">
          <label class="filter-date"><span>Từ ngày</span><input type="date" [(ngModel)]="dateFrom" (change)="loadOrders()"></label>
          <label class="filter-date"><span>Đến ngày</span><input type="date" [(ngModel)]="dateTo" (change)="loadOrders()"></label>
        </div>

        <div class="kpi-row">
          <div class="kpi"><span class="kpi-label">Số đơn</span><span class="kpi-value">{{ orders.totalCount | number }}</span></div>
          <div class="kpi"><span class="kpi-label">Doanh thu</span><span class="kpi-value">{{ orders.totalRevenue | number }} đ</span></div>
          <div class="kpi"><span class="kpi-label">Tổng chi phí</span><span class="kpi-value cost">{{ orders.totalCost | number }} đ</span></div>
          <div class="kpi total">
            <span class="kpi-label">Lãi (đơn đã có cost)</span>
            <span class="kpi-value" [class.profit]="orders.totalProfit >= 0" [class.loss]="orders.totalProfit < 0">
              {{ orders.totalProfit | number }} đ
            </span>
          </div>
          <div class="kpi"><span class="kpi-label">Biên TB</span><span class="kpi-value">{{ orders.averageMargin }}%</span></div>
          <div class="kpi warn" *ngIf="orders.ordersWithoutCost > 0">
            <span class="kpi-label">Chưa nhập cost</span>
            <span class="kpi-value">{{ orders.ordersWithoutCost }} đơn</span>
          </div>
        </div>

        <div class="table-wrap">
          <table class="table">
            <thead>
              <tr>
                <th>Mã đơn</th>
                <th>Khách hàng</th>
                <th>Ngày</th>
                <th>Trạng thái</th>
                <th class="num">Doanh thu</th>
                <th class="num">Giá cost</th>
                <th class="num">CP ship</th>
                <th class="num">CP gửi đi</th>
                <th class="num">Tổng chi phí</th>
                <th class="num">Lãi/lỗ</th>
                <th class="num">Biên %</th>
              </tr>
            </thead>
            <tbody>
              <tr *ngIf="loadingOrders"><td colspan="11" class="empty">Đang tải...</td></tr>
              <tr *ngIf="!loadingOrders && orders.items.length === 0">
                <td colspan="11" class="empty">Không có đơn hàng nào khớp bộ lọc.</td>
              </tr>
              <tr *ngFor="let o of orders.items" [class.loss-row]="o.hasCost && o.profit < 0">
                <td>
                  <a [routerLink]="['/orders', o.orderId]" target="_blank" class="order-link">{{ o.orderNumber }}</a>
                  <span class="badge missing" *ngIf="!o.hasCost">Chưa nhập</span>
                </td>
                <td>{{ o.customerName || '—' }}</td>
                <td class="nowrap">{{ o.revenueDate | date:'dd/MM/yyyy' }}</td>
                <td class="muted">{{ o.statusName }}</td>
                <td class="num">{{ o.revenue | number }}</td>
                <td class="num">{{ o.costAmount | number }}</td>
                <td class="num">{{ o.shippingCost | number }}</td>
                <td class="num">{{ o.outboundShippingCost | number }}</td>
                <td class="num cost">{{ o.totalCost | number }}</td>
                <td class="num strong" [class.profit]="o.profit >= 0" [class.loss]="o.profit < 0">
                  {{ o.hasCost ? (o.profit | number) : '—' }}
                </td>
                <td class="num">{{ o.hasCost ? o.profitMargin + '%' : '—' }}</td>
              </tr>
            </tbody>
          </table>
        </div>

        <div class="pager" *ngIf="orders.totalPages > 1">
          <button class="btn btn-sm" [disabled]="page <= 1" (click)="goPage(page - 1)">‹ Trước</button>
          <span>Trang {{ page }} / {{ orders.totalPages }}</span>
          <button class="btn btn-sm" [disabled]="page >= orders.totalPages" (click)="goPage(page + 1)">Sau ›</button>
        </div>
      </ng-container>
    </div>
  `,
  styles: [`
    .page-container { padding:24px; }
    .page-header { display:flex; justify-content:space-between; align-items:center; margin-bottom:14px; flex-wrap:wrap; gap:12px; }
    .page-header h1 { margin:0; font-size:22px; font-weight:600; }
    .basis { display:flex; align-items:center; gap:8px; font-size:13px; color:#64748b; }
    .basis select { padding:7px 10px; border:1px solid #cbd5e1; border-radius:6px; font-size:13px; }
    .tabs { display:flex; gap:4px; border-bottom:2px solid #e2e8f0; margin-bottom:16px; }
    .tab { background:none; border:none; padding:10px 18px; cursor:pointer; font-size:14px; color:#64748b; border-bottom:2px solid transparent; margin-bottom:-2px; }
    .tab.active { color:#4f46e5; border-bottom-color:#4f46e5; font-weight:600; }
    .filter-bar { display:flex; align-items:center; gap:12px; flex-wrap:wrap; margin-bottom:14px; }
    .filter-search { flex:1; min-width:220px; padding:8px 12px; border:1px solid #cbd5e1; border-radius:6px; font-size:14px; box-sizing:border-box; }
    .filter-date { display:flex; align-items:center; gap:6px; }
    .filter-date span { font-size:13px; color:#64748b; white-space:nowrap; }
    .filter-date input, .filter-date select { padding:8px 10px; border:1px solid #cbd5e1; border-radius:6px; font-size:14px; }
    .kpi-row { display:flex; gap:12px; flex-wrap:wrap; margin-bottom:16px; }
    .kpi { background:#fff; border-radius:8px; padding:12px 16px; box-shadow:0 1px 3px rgba(0,0,0,.06); min-width:140px; display:flex; flex-direction:column; gap:4px; }
    .kpi.total { background:#eef2ff; }
    .kpi.warn { background:#fffbeb; }
    .kpi-label { font-size:12px; color:#64748b; }
    .kpi-value { font-size:17px; font-weight:600; color:#1e293b; }
    .kpi-value.cost { color:#b45309; }
    .chart { display:flex; align-items:flex-end; gap:6px; height:150px; background:#fff; border-radius:8px; padding:14px; margin-bottom:16px; box-shadow:0 1px 3px rgba(0,0,0,.06); }
    .bar-col { flex:1; display:flex; flex-direction:column; align-items:center; height:100%; gap:4px; }
    .bar-area { flex:1; width:100%; display:flex; align-items:flex-end; justify-content:center; }
    .bar { width:70%; min-height:2px; border-radius:3px 3px 0 0; transition:height .2s; }
    .bar.profit-bar { background:#22c55e; }
    .bar.loss-bar { background:#ef4444; }
    .bar-label { font-size:11px; color:#94a3b8; }
    .table-wrap { overflow-x:auto; }
    .table { width:100%; border-collapse:collapse; font-size:13px; background:#fff; border-radius:8px; overflow:hidden; box-shadow:0 1px 3px rgba(0,0,0,.06); }
    .table th { background:#f8fafc; font-weight:600; color:#475569; white-space:nowrap; }
    .table th, .table td { padding:9px 10px; border-bottom:1px solid #e2e8f0; text-align:left; }
    .table th.num, .table td.num { text-align:right; }
    .table tfoot td { background:#f8fafc; font-weight:600; }
    .table tr.clickable { cursor:pointer; }
    .table tr.clickable:hover { background:#f8fafc; }
    .table tr.selected { background:#eef2ff; }
    .table tr.loss-row { background:#fef2f2; }
    .empty { text-align:center; color:#94a3b8; padding:24px; }
    .nowrap { white-space:nowrap; }
    .strong { font-weight:600; }
    .muted { color:#94a3b8; }
    .cost { color:#b45309; }
    .profit { color:#16a34a; }
    .loss { color:#dc2626; }
    .order-link { color:#4f46e5; font-weight:600; text-decoration:none; }
    .order-link:hover { text-decoration:underline; }
    .badge { padding:1px 6px; border-radius:10px; font-size:10px; margin-left:6px; }
    .badge.missing { background:#fef3c7; color:#92400e; }
    .detail-panel { background:#fff; border-radius:8px; margin-top:16px; box-shadow:0 1px 3px rgba(0,0,0,.06); }
    .detail-header { display:flex; justify-content:space-between; align-items:center; padding:14px 18px; border-bottom:1px solid #e2e8f0; }
    .detail-header h3 { margin:0; font-size:16px; }
    .detail-body { display:flex; gap:20px; padding:18px; flex-wrap:wrap; }
    .detail-col { flex:1; min-width:240px; }
    .detail-col.wide { flex:2; min-width:320px; }
    .detail-col h4 { margin:0 0 10px; font-size:13px; color:#475569; text-transform:uppercase; letter-spacing:.03em; }
    .cat-row { display:flex; justify-content:space-between; gap:12px; padding:6px 0; border-bottom:1px solid #f1f5f9; font-size:13px; }
    .cat-row a { color:#4f46e5; text-decoration:none; font-weight:600; }
    .amount { font-weight:600; white-space:nowrap; }
    .pager { display:flex; align-items:center; gap:12px; justify-content:center; margin-top:16px; font-size:13px; color:#475569; }
    .btn { padding:8px 16px; border:none; border-radius:6px; cursor:pointer; font-size:14px; }
    .btn:disabled { opacity:.5; cursor:not-allowed; }
    .btn-sm { padding:4px 10px; font-size:12px; background:#e2e8f0; }
    .btn-close { background:none; border:none; font-size:24px; cursor:pointer; color:#64748b; }
  `]
})
export class ProfitReportComponent implements OnInit {
  tab: 'monthly' | 'orders' = 'monthly';
  revenueBasis = 'confirmed';

  // Tab tháng
  year = new Date().getFullYear();
  years: number[] = [];
  monthly: MonthlyProfitResult = { year: 0, revenueBasis: '', months: [], total: this.emptyMonth() };
  detail: MonthlyProfitDetail | null = null;
  loadingMonthly = false;

  // Tab đơn hàng
  orders: OrderProfitResult = {
    items: [], totalCount: 0, page: 1, pageSize: 100, totalPages: 1,
    totalRevenue: 0, totalCost: 0, totalProfit: 0, averageMargin: 0,
    ordersWithoutCost: 0, revenueWithoutCost: 0
  };
  search = '';
  dateFrom = '';
  dateTo = '';
  page = 1;
  loadingOrders = false;

  constructor(private finance: FinanceService, private toast: ToastService) {}

  ngOnInit(): void {
    const current = new Date().getFullYear();
    this.years = [current - 3, current - 2, current - 1, current, current + 1];

    const now = new Date();
    this.dateFrom = this.toDateInput(new Date(now.getFullYear(), now.getMonth(), 1));
    this.dateTo = this.toDateInput(new Date(now.getFullYear(), now.getMonth() + 1, 0));

    this.loadMonthly();
  }

  switchTab(tab: 'monthly' | 'orders'): void {
    this.tab = tab;
    if (tab === 'monthly' && this.monthly.months.length === 0) this.loadMonthly();
    if (tab === 'orders' && this.orders.items.length === 0) this.loadOrders();
  }

  reload(): void {
    this.detail = null;
    if (this.tab === 'monthly') this.loadMonthly();
    else this.loadOrders();
  }

  loadMonthly(): void {
    this.loadingMonthly = true;
    this.detail = null;
    this.finance.getMonthlyProfit(Number(this.year), this.revenueBasis).subscribe({
      next: res => { this.monthly = res; this.loadingMonthly = false; },
      error: () => { this.loadingMonthly = false; this.toast.error('Không tải được báo cáo theo tháng.'); }
    });
  }

  loadOrders(): void {
    this.loadingOrders = true;
    const filter: { [key: string]: any } = { page: this.page, pageSize: 100, revenueBasis: this.revenueBasis };
    if (this.search.trim()) filter['search'] = this.search.trim();
    if (this.dateFrom) filter['dateFrom'] = new Date(`${this.dateFrom}T00:00:00`).toISOString();
    if (this.dateTo) filter['dateTo'] = new Date(`${this.dateTo}T23:59:59.999`).toISOString();

    this.finance.getOrderProfit(filter).subscribe({
      next: res => { this.orders = res; this.loadingOrders = false; },
      error: () => { this.loadingOrders = false; this.toast.error('Không tải được báo cáo theo đơn.'); }
    });
  }

  goPage(p: number): void { this.page = p; this.loadOrders(); }

  openDetail(m: MonthlyProfit): void {
    if (this.detail?.summary?.month === m.month) { this.detail = null; return; }
    this.finance.getMonthDetail(Number(this.year), m.month, this.revenueBasis).subscribe({
      next: d => this.detail = d,
      error: () => this.toast.error('Không tải được chi tiết tháng.')
    });
  }

  /** Cột biểu đồ scale theo giá trị tuyệt đối lớn nhất để tháng lỗ vẫn nhìn được. */
  get maxAbsNet(): number {
    return this.monthly.months.reduce((max, m) => Math.max(max, Math.abs(m.netProfit)), 0);
  }

  barHeight(m: MonthlyProfit): number {
    const max = this.maxAbsNet;
    return max > 0 ? Math.abs(m.netProfit) / max * 100 : 0;
  }

  private toDateInput(d: Date): string {
    const mm = String(d.getMonth() + 1).padStart(2, '0');
    const day = String(d.getDate()).padStart(2, '0');
    return `${d.getFullYear()}-${mm}-${day}`;
  }

  private emptyMonth(): MonthlyProfit {
    return {
      year: 0, month: 0, label: '', orderCount: 0, ordersWithoutCost: 0,
      revenue: 0, cogs: 0, grossProfit: 0, payrollCost: 0, fixedCost: 0, netProfit: 0, netMargin: 0
    };
  }
}
