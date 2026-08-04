import { Component, ElementRef, OnInit, ViewChild } from '@angular/core';
import { FinanceService } from '../../../core/services/finance.service';
import { ToastService } from '../../../core/services/toast.service';
import {
  OrderCostListItem, OrderCostSummary, BulkOrderCostItem, CostImportResult
} from '../../../core/models/finance.model';

@Component({
  selector: 'app-order-costs',
  template: `
    <div class="page-container">
      <div class="page-header">
        <h1>Chi phí sản xuất hàng hóa</h1>
        <div class="header-actions">
          <button class="btn btn-secondary" (click)="downloadTemplate()">Tải file mẫu</button>
          <button class="btn btn-secondary" (click)="fileInput.click()">Tải file giá cost</button>
          <input #fileInput type="file" accept=".xlsx,.csv" hidden (change)="onImportFile($event)">
          <button class="btn btn-primary" (click)="saveAll()" [disabled]="dirtyCount === 0 || saving">
            {{ saving ? 'Đang lưu...' : 'Lưu' + (dirtyCount ? ' (' + dirtyCount + ')' : '') }}
          </button>
        </div>
      </div>

      <p class="hint">
        Đơn hàng từ trạng thái <strong>Đang sản xuất</strong> trở đi tự động xuất hiện ở đây.
        Nhập trực tiếp vào bảng rồi bấm <strong>Lưu</strong>, hoặc tải file giá cost lên.
      </p>

      <div class="filter-bar">
        <input type="text" class="filter-search" [(ngModel)]="search" (keyup.enter)="load()"
               placeholder="Tìm mã đơn, tên khách hàng...">
        <label class="filter-date"><span>Từ ngày</span><input type="date" [(ngModel)]="dateFrom" (change)="load()"></label>
        <label class="filter-date"><span>Đến ngày</span><input type="date" [(ngModel)]="dateTo" (change)="load()"></label>
        <label class="filter-check">
          <input type="checkbox" [(ngModel)]="onlyMissing" (change)="load()">
          <span>Chỉ đơn chưa nhập cost</span>
        </label>
        <button class="btn btn-secondary" (click)="clearFilters()">Xóa lọc</button>
      </div>

      <div class="kpi-row">
        <div class="kpi"><span class="kpi-label">Số đơn</span><span class="kpi-value">{{ summary.totalOrders | number }}</span></div>
        <div class="kpi"><span class="kpi-label">Doanh thu</span><span class="kpi-value">{{ summary.totalRevenue | number }} đ</span></div>
        <div class="kpi"><span class="kpi-label">Tổng chi phí</span><span class="kpi-value cost">{{ summary.totalCost | number }} đ</span></div>
        <div class="kpi">
          <span class="kpi-label">Lãi (đơn đã có cost)</span>
          <span class="kpi-value" [class.profit]="summary.totalProfit >= 0" [class.loss]="summary.totalProfit < 0">
            {{ summary.totalProfit | number }} đ
          </span>
        </div>
        <div class="kpi warn" *ngIf="summary.ordersWithoutCost > 0">
          <span class="kpi-label">Chưa nhập cost</span><span class="kpi-value">{{ summary.ordersWithoutCost | number }} đơn</span>
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
              <th class="num edit-col">Giá cost</th>
              <th class="num edit-col">CP ship hàng</th>
              <th class="num edit-col">CP gửi hàng đi</th>
              <th class="num edit-col">CP khác</th>
              <th class="num">Tổng cost</th>
              <th class="num">Lãi/lỗ</th>
              <th></th>
            </tr>
          </thead>
          <tbody>
            <tr *ngIf="!loading && rows.length === 0">
              <td colspan="12" class="empty">Không có đơn hàng nào khớp bộ lọc.</td>
            </tr>
            <tr *ngIf="loading">
              <td colspan="12" class="empty">Đang tải...</td>
            </tr>
            <tr *ngFor="let r of rows; trackBy: trackByOrderId" [class.dirty]="isDirty(r)">
              <td>
                <a [routerLink]="['/orders', r.orderId]" target="_blank" class="order-link">{{ r.orderNumber }}</a>
                <span class="badge missing" *ngIf="!r.hasCost">Chưa nhập</span>
                <span class="badge locked" *ngIf="r.isFinalized" title="Đã chốt sổ">Đã chốt</span>
              </td>
              <td>{{ r.customerName || '—' }}</td>
              <td class="nowrap">{{ (r.confirmedDate || r.orderDate) | date:'dd/MM/yyyy' }}</td>
              <td><span class="status">{{ r.statusName }}</span></td>
              <td class="num">{{ r.revenue | number }}</td>
              <td class="num"><input type="number" min="0" [(ngModel)]="r.costAmount" (ngModelChange)="onEdit(r)" [disabled]="isLocked(r)"></td>
              <td class="num"><input type="number" min="0" [(ngModel)]="r.shippingCost" (ngModelChange)="onEdit(r)" [disabled]="isLocked(r)"></td>
              <td class="num"><input type="number" min="0" [(ngModel)]="r.outboundShippingCost" (ngModelChange)="onEdit(r)" [disabled]="isLocked(r)"></td>
              <td class="num"><input type="number" min="0" [(ngModel)]="r.otherCost" (ngModelChange)="onEdit(r)" [disabled]="isLocked(r)"></td>
              <td class="num strong">{{ rowTotal(r) | number }}</td>
              <td class="num strong" [class.profit]="rowProfit(r) >= 0" [class.loss]="rowProfit(r) < 0">
                {{ r.hasCost || isDirty(r) ? (rowProfit(r) | number) : '—' }}
              </td>
              <td class="actions">
                <button class="btn btn-sm" (click)="pickAttachment(r)" [title]="r.costFileName || 'Đính kèm file giá cost'">
                  {{ r.costFileUrl ? '📎' : '＋' }}
                </button>
              </td>
            </tr>
          </tbody>
          <tfoot *ngIf="rows.length > 0">
            <tr>
              <td colspan="4">Tổng trang này</td>
              <td class="num strong">{{ pageRevenue | number }}</td>
              <td colspan="4"></td>
              <td class="num strong">{{ pageCost | number }}</td>
              <td class="num strong" [class.profit]="pageProfit >= 0" [class.loss]="pageProfit < 0">{{ pageProfit | number }}</td>
              <td></td>
            </tr>
          </tfoot>
        </table>
      </div>

      <input #attachInput type="file" accept=".xlsx,.xls,.csv,.pdf,.png,.jpg,.jpeg" hidden (change)="onAttachmentFile($event)">

      <div class="pager" *ngIf="totalPages > 1">
        <button class="btn btn-sm" [disabled]="page <= 1" (click)="goPage(page - 1)">‹ Trước</button>
        <span>Trang {{ page }} / {{ totalPages }} — {{ totalCount | number }} đơn</span>
        <button class="btn btn-sm" [disabled]="page >= totalPages" (click)="goPage(page + 1)">Sau ›</button>
      </div>

      <!-- Kết quả import -->
      <div class="modal-overlay" *ngIf="importResult as ir" (click)="importResult = null">
        <div class="modal" (click)="$event.stopPropagation()">
          <div class="modal-header">
            <h3>Kết quả import file giá cost</h3>
            <button class="btn-close" (click)="importResult = null">×</button>
          </div>
          <div class="modal-body">
            <p>
              Đọc <strong>{{ ir.totalRows }}</strong> dòng —
              thành công <strong class="ok">{{ ir.successCount }}</strong>,
              lỗi <strong class="bad">{{ ir.errors.length }}</strong>.
            </p>
            <div class="err-list" *ngIf="ir.errors.length">
              <div class="err-row" *ngFor="let e of ir.errors">
                <span class="err-line">Dòng {{ e.rowNumber }}</span>
                <span class="err-order" *ngIf="e.orderNumber">{{ e.orderNumber }}</span>
                <span>{{ e.error }}</span>
              </div>
            </div>
          </div>
          <div class="modal-footer">
            <button class="btn btn-primary" (click)="importResult = null">Đóng</button>
          </div>
        </div>
      </div>
    </div>
  `,
  styles: [`
    .page-container { padding:24px; }
    .page-header { display:flex; justify-content:space-between; align-items:center; margin-bottom:8px; flex-wrap:wrap; gap:12px; }
    .page-header h1 { margin:0; font-size:22px; font-weight:600; }
    .header-actions { display:flex; gap:8px; flex-wrap:wrap; }
    .hint { color:#64748b; font-size:13px; margin:0 0 14px; }
    .filter-bar { display:flex; align-items:center; gap:12px; flex-wrap:wrap; margin-bottom:14px; }
    .filter-search { flex:1; min-width:220px; padding:8px 12px; border:1px solid #cbd5e1; border-radius:6px; font-size:14px; box-sizing:border-box; }
    .filter-date { display:flex; align-items:center; gap:6px; }
    .filter-date span { font-size:13px; color:#64748b; white-space:nowrap; }
    .filter-date input { padding:8px 10px; border:1px solid #cbd5e1; border-radius:6px; font-size:14px; }
    .filter-check { display:flex; align-items:center; gap:6px; font-size:13px; color:#475569; white-space:nowrap; }
    .kpi-row { display:flex; gap:12px; flex-wrap:wrap; margin-bottom:14px; }
    .kpi { background:#fff; border-radius:8px; padding:12px 16px; box-shadow:0 1px 3px rgba(0,0,0,.06); min-width:150px; display:flex; flex-direction:column; gap:4px; }
    .kpi.warn { background:#fffbeb; }
    .kpi-label { font-size:12px; color:#64748b; }
    .kpi-value { font-size:18px; font-weight:600; color:#1e293b; }
    .kpi-value.cost { color:#b45309; }
    .table-wrap { overflow-x:auto; }
    .table { width:100%; border-collapse:collapse; font-size:13px; background:#fff; border-radius:8px; overflow:hidden; box-shadow:0 1px 3px rgba(0,0,0,.06); }
    .table th { background:#f8fafc; font-weight:600; color:#475569; white-space:nowrap; }
    .table th, .table td { padding:8px 10px; border-bottom:1px solid #e2e8f0; text-align:left; }
    .table th.num, .table td.num { text-align:right; }
    .table th.edit-col { background:#eef2ff; }
    .table tfoot td { background:#f8fafc; font-weight:600; }
    .table tr.dirty { background:#fefce8; }
    .table td input { width:110px; padding:5px 8px; border:1px solid #cbd5e1; border-radius:5px; font-size:13px; text-align:right; }
    .table td input:disabled { background:#f1f5f9; color:#94a3b8; }
    .empty { text-align:center; color:#94a3b8; padding:24px; }
    .nowrap { white-space:nowrap; }
    .strong { font-weight:600; }
    .profit { color:#16a34a; }
    .loss { color:#dc2626; }
    .order-link { color:#4f46e5; font-weight:600; text-decoration:none; }
    .order-link:hover { text-decoration:underline; }
    .status { font-size:12px; color:#475569; white-space:nowrap; }
    .badge { padding:2px 6px; border-radius:10px; font-size:10px; margin-left:6px; white-space:nowrap; }
    .badge.missing { background:#fef3c7; color:#92400e; }
    .badge.locked { background:#e0e7ff; color:#3730a3; }
    .actions { white-space:nowrap; }
    .pager { display:flex; align-items:center; gap:12px; justify-content:center; margin-top:16px; font-size:13px; color:#475569; }
    .btn { padding:8px 16px; border:none; border-radius:6px; cursor:pointer; font-size:14px; }
    .btn:disabled { opacity:.5; cursor:not-allowed; }
    .btn-primary { background:#6366f1; color:#fff; }
    .btn-secondary { background:#e2e8f0; color:#1e293b; }
    .btn-sm { padding:4px 10px; font-size:12px; background:#e2e8f0; }
    .btn-close { background:none; border:none; font-size:24px; cursor:pointer; color:#64748b; }
    .modal-overlay { position:fixed; inset:0; background:rgba(0,0,0,.5); display:flex; align-items:center; justify-content:center; z-index:1000; }
    .modal { background:#fff; border-radius:8px; max-width:640px; width:90%; max-height:90vh; overflow-y:auto; }
    .modal-header { display:flex; justify-content:space-between; align-items:center; padding:16px 20px; border-bottom:1px solid #e2e8f0; }
    .modal-header h3 { margin:0; font-size:18px; }
    .modal-body { padding:20px; }
    .modal-footer { display:flex; justify-content:flex-end; padding:16px 20px; border-top:1px solid #e2e8f0; }
    .ok { color:#16a34a; } .bad { color:#dc2626; }
    .err-list { margin-top:12px; max-height:320px; overflow-y:auto; border:1px solid #e2e8f0; border-radius:6px; }
    .err-row { display:flex; gap:10px; padding:8px 12px; border-bottom:1px solid #f1f5f9; font-size:13px; }
    .err-row:last-child { border-bottom:none; }
    .err-line { color:#64748b; white-space:nowrap; }
    .err-order { font-family:monospace; background:#f1f5f9; padding:0 6px; border-radius:4px; white-space:nowrap; }
  `]
})
export class OrderCostsComponent implements OnInit {
  @ViewChild('attachInput') attachInput?: ElementRef<HTMLInputElement>;

  rows: OrderCostListItem[] = [];
  summary: OrderCostSummary = {
    totalOrders: 0, ordersWithCost: 0, ordersWithoutCost: 0,
    totalRevenue: 0, totalCost: 0, totalProfit: 0
  };
  loading = false;
  saving = false;

  search = '';
  dateFrom = '';
  dateTo = '';
  onlyMissing = false;

  page = 1;
  pageSize = 100;
  totalPages = 1;
  totalCount = 0;

  importResult: CostImportResult | null = null;

  /** Bản gốc để biết dòng nào bị sửa — key = orderId. */
  private original = new Map<string, string>();
  private attachTarget: OrderCostListItem | null = null;

  constructor(private finance: FinanceService, private toast: ToastService) {}

  ngOnInit(): void {
    // Mặc định xem tháng hiện tại.
    const now = new Date();
    this.dateFrom = this.toDateInput(new Date(now.getFullYear(), now.getMonth(), 1));
    this.dateTo = this.toDateInput(new Date(now.getFullYear(), now.getMonth() + 1, 0));
    this.load();
  }

  load(): void {
    this.loading = true;
    const filter: { [key: string]: any } = { page: this.page, pageSize: this.pageSize };
    if (this.search.trim()) filter['search'] = this.search.trim();
    if (this.dateFrom) filter['dateFrom'] = new Date(`${this.dateFrom}T00:00:00`).toISOString();
    if (this.dateTo) filter['dateTo'] = new Date(`${this.dateTo}T23:59:59.999`).toISOString();
    if (this.onlyMissing) filter['hasCost'] = false;

    this.finance.getOrderCosts(filter).subscribe({
      next: res => {
        this.rows = res.items || [];
        this.summary = res.summary;
        this.totalCount = res.totalCount;
        this.totalPages = res.totalPages || 1;
        this.original.clear();
        this.rows.forEach(r => this.original.set(r.orderId, this.snapshot(r)));
        this.loading = false;
      },
      error: () => { this.loading = false; this.toast.error('Không tải được danh sách chi phí.'); }
    });
  }

  clearFilters(): void {
    this.search = '';
    this.dateFrom = '';
    this.dateTo = '';
    this.onlyMissing = false;
    this.page = 1;
    this.load();
  }

  goPage(p: number): void {
    if (this.dirtyCount > 0 && !confirm('Còn thay đổi chưa lưu. Chuyển trang sẽ mất thay đổi. Tiếp tục?')) return;
    this.page = p;
    this.load();
  }

  // ── Sửa inline ────────────────────────────────────────────────────────
  onEdit(row: OrderCostListItem): void {
    row.totalCost = this.rowTotal(row);
    row.profit = this.rowProfit(row);
  }

  isLocked(row: OrderCostListItem): boolean { return row.isFinalized; }

  isDirty(row: OrderCostListItem): boolean {
    return this.original.get(row.orderId) !== this.snapshot(row);
  }

  get dirtyCount(): number {
    return this.rows.filter(r => this.isDirty(r)).length;
  }

  rowTotal(r: OrderCostListItem): number {
    return this.n(r.costAmount) + this.n(r.shippingCost) + this.n(r.outboundShippingCost) + this.n(r.otherCost);
  }

  rowProfit(r: OrderCostListItem): number {
    return this.n(r.revenue) - this.rowTotal(r);
  }

  get pageRevenue(): number { return this.rows.reduce((s, r) => s + this.n(r.revenue), 0); }
  get pageCost(): number { return this.rows.reduce((s, r) => s + this.rowTotal(r), 0); }
  get pageProfit(): number { return this.pageRevenue - this.pageCost; }

  saveAll(): void {
    const dirty = this.rows.filter(r => this.isDirty(r));
    if (dirty.length === 0) return;

    const items: BulkOrderCostItem[] = dirty.map(r => ({
      orderId: r.orderId,
      costAmount: this.n(r.costAmount),
      shippingCost: this.n(r.shippingCost),
      outboundShippingCost: this.n(r.outboundShippingCost),
      otherCost: this.n(r.otherCost),
      notes: r.notes,
      isFinalized: r.isFinalized
    }));

    this.saving = true;
    this.finance.bulkSaveOrderCosts(items).subscribe({
      next: saved => {
        this.saving = false;
        this.toast.success(`Đã lưu chi phí cho ${saved} đơn.`);
        this.load();
      },
      error: err => {
        this.saving = false;
        this.toast.error(err?.error?.message || 'Lưu chi phí thất bại.');
      }
    });
  }

  // ── Import / template / đính kèm ──────────────────────────────────────
  downloadTemplate(): void {
    this.finance.downloadImportTemplate().subscribe({
      next: blob => this.saveBlob(blob, 'mau-gia-cost.xlsx'),
      error: () => this.toast.error('Không tải được file mẫu.')
    });
  }

  onImportFile(event: Event): void {
    const input = event.target as HTMLInputElement;
    const file = input.files?.[0];
    if (!file) return;

    this.finance.importOrderCosts(file).subscribe({
      next: res => {
        this.importResult = res.data;
        this.toast.success(res.message || 'Import xong.');
        this.load();
      },
      error: err => this.toast.error(err?.error?.message || 'Import thất bại.')
    });
    input.value = '';
  }

  pickAttachment(row: OrderCostListItem): void {
    this.attachTarget = row;
    this.attachInput?.nativeElement.click();
  }

  onAttachmentFile(event: Event): void {
    const input = event.target as HTMLInputElement;
    const file = input.files?.[0];
    const target = this.attachTarget;
    if (!file || !target) { input.value = ''; return; }

    this.finance.uploadCostAttachment(target.orderId, file).subscribe({
      next: updated => {
        target.costFileUrl = updated.costFileUrl;
        target.costFileName = updated.costFileName;
        this.toast.success('Đã đính kèm file giá cost.');
      },
      error: err => this.toast.error(err?.error?.message || 'Đính kèm thất bại.')
    });
    input.value = '';
    this.attachTarget = null;
  }

  trackByOrderId(_: number, row: OrderCostListItem): string { return row.orderId; }

  // ── helpers ───────────────────────────────────────────────────────────
  private n(v: any): number { return Number(v) || 0; }

  private snapshot(r: OrderCostListItem): string {
    return [r.costAmount, r.shippingCost, r.outboundShippingCost, r.otherCost, r.isFinalized]
      .map(v => this.n(v)).join('|');
  }

  private toDateInput(d: Date): string {
    const m = String(d.getMonth() + 1).padStart(2, '0');
    const day = String(d.getDate()).padStart(2, '0');
    return `${d.getFullYear()}-${m}-${day}`;
  }

  private saveBlob(blob: Blob, fileName: string): void {
    const url = URL.createObjectURL(blob);
    const a = document.createElement('a');
    a.href = url;
    a.download = fileName;
    a.click();
    URL.revokeObjectURL(url);
  }
}
