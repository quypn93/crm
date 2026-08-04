export enum LeaderboardScope {
  Sales = 0,
  Design = 1,
  Production = 2,
  Delivery = 3
}

export enum LeaderboardPeriod {
  Week = 0,
  Month = 1,
  Quarter = 2
}

export interface LeaderboardEntry {
  userId: string;
  fullName: string;
  rank: number;
  revenue: number;
  count: number;
  growthPercent?: number | null;
  kpiProgressPercent?: number | null;
}

export interface LeaderboardResult {
  scope: LeaderboardScope;
  period: LeaderboardPeriod;
  periodStart: string;
  periodEnd: string;
  periodLabel: string;
  updatedAt: string;
  primaryMetric: 'revenue' | 'count';
  countLabel: string;
  entries: LeaderboardEntry[];
}
