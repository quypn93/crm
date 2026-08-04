import { Component, OnInit } from '@angular/core';
import { LeaderboardService } from '../../../core/services/leaderboard.service';
import { LeaderboardEntry, LeaderboardPeriod, LeaderboardResult, LeaderboardScope } from '../../../core/models/leaderboard.model';

interface Particle {
  left: number;
  top: number;
  size: number;
  color: string;
  delay: number;
  duration: number;
}

interface ScopeOption { value: LeaderboardScope; label: string; icon: string; }
interface PeriodOption { value: LeaderboardPeriod; label: string; }

@Component({
  selector: 'app-leaderboard',
  templateUrl: './leaderboard.component.html',
  styleUrls: ['./leaderboard.component.scss']
})
export class LeaderboardComponent implements OnInit {
  readonly LeaderboardScope = LeaderboardScope;

  readonly scopeOptions: ScopeOption[] = [
    { value: LeaderboardScope.Sales, label: 'Sales', icon: '💼' },
    { value: LeaderboardScope.Design, label: 'Thiết kế', icon: '🎨' },
    { value: LeaderboardScope.Production, label: 'Sản xuất', icon: '🏭' },
    { value: LeaderboardScope.Delivery, label: 'Giao hàng', icon: '🚚' }
  ];

  readonly periodOptions: PeriodOption[] = [
    { value: LeaderboardPeriod.Week, label: 'Tuần' },
    { value: LeaderboardPeriod.Month, label: 'Tháng' },
    { value: LeaderboardPeriod.Quarter, label: 'Quý' }
  ];

  readonly avatarPalette = ['#6366f1', '#ec4899', '#f59e0b', '#10b981', '#06b6d4', '#8b5cf6', '#ef4444', '#0ea5e9'];

  scope: LeaderboardScope = LeaderboardScope.Sales;
  period: LeaderboardPeriod = LeaderboardPeriod.Month;
  referenceDate = new Date();
  showScopeMenu = false;

  // Sales có 2 chỉ số đầu bảng (Doanh số/Số đơn) như bản mẫu; các bộ phận khác chỉ có 1 chỉ số (số lượng việc hoàn thành).
  headlineMode: 'revenue' | 'count' = 'revenue';

  result: LeaderboardResult | null = null;
  isLoading = false;
  searchTerm = '';
  particles: Particle[] = [];

  constructor(private leaderboardService: LeaderboardService) {}

  ngOnInit(): void {
    this.particles = this.generateParticles(36);
    this.load();
  }

  private generateParticles(count: number): Particle[] {
    const colors = ['#f59e0b', '#ef4444', '#3b82f6', '#10b981', '#f472b6', '#facc15'];
    return Array.from({ length: count }, () => ({
      left: Math.random() * 100,
      top: Math.random() * 100,
      size: 6 + Math.random() * 14,
      color: colors[Math.floor(Math.random() * colors.length)],
      delay: Math.random() * 6,
      duration: 6 + Math.random() * 8
    }));
  }

  load(): void {
    this.isLoading = true;
    const dateStr = this.formatDateParam(this.referenceDate);
    this.leaderboardService.getLeaderboard(this.scope, this.period, dateStr).subscribe({
      next: (res) => {
        this.result = res;
        this.headlineMode = res.primaryMetric === 'revenue' ? this.headlineMode : 'count';
        this.isLoading = false;
      },
      error: () => {
        this.result = null;
        this.isLoading = false;
      }
    });
  }

  selectScope(scope: LeaderboardScope): void {
    this.scope = scope;
    this.showScopeMenu = false;
    this.headlineMode = scope === LeaderboardScope.Sales ? 'revenue' : 'count';
    this.load();
  }

  selectPeriod(period: LeaderboardPeriod): void {
    this.period = period;
    this.load();
  }

  navigatePeriod(step: number): void {
    const d = new Date(this.referenceDate);
    if (this.period === LeaderboardPeriod.Week) d.setDate(d.getDate() + step * 7);
    else if (this.period === LeaderboardPeriod.Quarter) d.setMonth(d.getMonth() + step * 3);
    else d.setMonth(d.getMonth() + step);
    this.referenceDate = d;
    this.load();
  }

  toggleHeadlineMode(): void {
    if (this.scope !== LeaderboardScope.Sales) return;
    this.headlineMode = this.headlineMode === 'revenue' ? 'count' : 'revenue';
  }

  get scopeLabel(): string {
    return this.scopeOptions.find(s => s.value === this.scope)?.label || '';
  }

  get scopeIcon(): string {
    return this.scopeOptions.find(s => s.value === this.scope)?.icon || '';
  }

  get top3(): LeaderboardEntry[] {
    return (this.result?.entries || []).slice(0, 3);
  }

  // Thứ tự hiển thị bục: [Hạng 2, Hạng 1, Hạng 3] — hạng 1 ở giữa và cao nhất.
  get podiumOrder(): (LeaderboardEntry | null)[] {
    const [first, second, third] = this.top3;
    return [second || null, first || null, third || null];
  }

  get rest(): LeaderboardEntry[] {
    const list = (this.result?.entries || []).slice(3);
    const term = this.searchTerm.trim().toLowerCase();
    if (!term) return list;
    return list.filter(e => e.fullName.toLowerCase().includes(term));
  }

  // # | Nhân viên | (Số đơn — chỉ Sales) | Doanh thu/countLabel | Tiến độ KPI | Tăng trưởng
  get visibleColumnCount(): number {
    return this.scope === LeaderboardScope.Sales ? 6 : 5;
  }

  headlineValue(entry: LeaderboardEntry): number {
    return this.scope === LeaderboardScope.Sales && this.headlineMode === 'revenue' ? entry.revenue : entry.count;
  }

  headlineText(entry: LeaderboardEntry): string {
    if (this.scope === LeaderboardScope.Sales && this.headlineMode === 'revenue') {
      return this.formatMillions(entry.revenue);
    }
    return `${entry.count} ${this.countUnitLabel}`;
  }

  // Mọi bộ phận đều xếp hạng theo số đơn hàng.
  get countUnitLabel(): string {
    return 'đơn';
  }

  formatMillions(value: number): string {
    return `${(value / 1_000_000).toLocaleString('vi-VN', { minimumFractionDigits: 2, maximumFractionDigits: 2 })} triệu`;
  }

  formatCurrency(value: number): string {
    return new Intl.NumberFormat('vi-VN', { style: 'currency', currency: 'VND' }).format(value);
  }

  getInitials(fullName: string): string {
    const parts = fullName.trim().split(/\s+/).filter(Boolean);
    if (!parts.length) return '?';
    const first = parts[0].charAt(0);
    const last = parts.length > 1 ? parts[parts.length - 1].charAt(0) : '';
    return (first + last).toUpperCase();
  }

  getAvatarColor(fullName: string): string {
    let hash = 0;
    for (let i = 0; i < fullName.length; i++) hash = fullName.charCodeAt(i) + ((hash << 5) - hash);
    return this.avatarPalette[Math.abs(hash) % this.avatarPalette.length];
  }

  private formatDateParam(d: Date): string {
    const y = d.getFullYear();
    const m = String(d.getMonth() + 1).padStart(2, '0');
    const day = String(d.getDate()).padStart(2, '0');
    return `${y}-${m}-${day}`;
  }

  formatUpdatedAt(iso: string | undefined): string {
    if (!iso) return '';
    const d = new Date(iso);
    return `${String(d.getHours()).padStart(2, '0')}:${String(d.getMinutes()).padStart(2, '0')}`;
  }
}
