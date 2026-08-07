// Model cho module Tài chính (chi phí & báo cáo lãi/lỗ). Chỉ Admin + Kế toán truy cập.

export interface OrderCostListItem {
  orderId: string;
  orderNumber: string;
  customerName?: string;
  status: number;
  statusName: string;
  orderDate: string;
  confirmedDate?: string;
  completionDate?: string;
  createdByUserName?: string;

  revenue: number;
  paidAmount: number;

  /** Tổng SL sản phẩm của đơn (cộng mọi dòng size) — nhân với đơn giá cost. */
  totalQuantity: number;

  unitCost: number;          // đơn giá 1 sản phẩm
  costAmount: number;        // = unitCost × totalQuantity
  giftUnitCost: number;      // đơn giá 1 phần quà
  giftQuantity: number;      // SL quà, khác SL áo
  giftAmount: number;        // = giftUnitCost × giftQuantity
  shippingCost: number;
  outboundShippingCost: number;
  otherCost: number;
  totalCost: number;

  shippingCode?: string;
  /** true = mã do hãng vận chuyển sinh → ô chỉ đọc. */
  shippingCodeFromCarrier: boolean;
  settlementAmount: number;

  profit: number;
  profitMargin: number;

  hasCost: boolean;
  isFinalized: boolean;
  notes?: string;
  costFileUrl?: string;
  costFileName?: string;
  enteredByUserName?: string;
  enteredAt?: string;
}

export interface OrderCostSummary {
  totalOrders: number;
  ordersWithCost: number;
  ordersWithoutCost: number;
  totalRevenue: number;
  totalCost: number;
  totalProfit: number;
}

export interface OrderCostListResult {
  items: OrderCostListItem[];
  totalCount: number;
  page: number;
  pageSize: number;
  totalPages: number;
  summary: OrderCostSummary;
}

export interface UpsertOrderCost {
  unitCost: number;
  giftUnitCost: number;
  giftQuantity: number;
  shippingCost: number;
  outboundShippingCost: number;
  otherCost: number;
  shippingCode?: string;
  settlementAmount: number;
  notes?: string;
  isFinalized: boolean;
}

export interface BulkOrderCostItem extends UpsertOrderCost {
  orderId: string;
}

export interface CostImportRowError {
  rowNumber: number;
  orderNumber?: string;
  error: string;
}

export interface CostImportResult {
  totalRows: number;
  successCount: number;
  skippedCount: number;
  errors: CostImportRowError[];
}

// ── Đầu mục chi phí cố định ───────────────────────────────────────────────
export interface ExpenseCategory {
  id: string;
  name: string;
  description?: string;
  sortOrder: number;
  isActive: boolean;
  isSystem: boolean;
  usageCount: number;
}

export interface CreateExpenseCategory {
  name: string;
  description?: string;
  sortOrder: number;
  isActive: boolean;
}

// ── Chi phí cố định (theo ngày) ───────────────────────────────────────────
export interface FixedExpense {
  id: string;
  expenseDate: string;          // yyyy-MM-dd
  expenseCategoryId: string;
  categoryName: string;
  amount: number;
  notes?: string;
  attachmentUrl?: string;
  attachmentName?: string;
  createdByUserName?: string;
  createdAt: string;
}

export interface CreateFixedExpense {
  expenseDate: string;
  expenseCategoryId: string;
  amount: number;
  notes?: string;
}

export interface ExpenseCategoryTotal {
  categoryId: string;
  categoryName: string;
  amount: number;
}

export interface FixedExpenseListResult {
  items: FixedExpense[];
  totalCount: number;
  page: number;
  pageSize: number;
  totalPages: number;
  grandTotal: number;
  totalsByCategory: ExpenseCategoryTotal[];
}

// ── Chi phí nhân sự (theo tháng) ──────────────────────────────────────────
export interface PayrollEntry {
  id: string;
  year: number;
  month: number;
  userId?: string;
  employeeName: string;
  position?: string;
  salary: number;
  allowance: number;
  insurance: number;
  otherCost: number;
  totalAmount: number;
  notes?: string;
}

export interface PayrollPeriod {
  year: number;
  month: number;
  items: PayrollEntry[];
  totalSalary: number;
  totalAllowance: number;
  totalInsurance: number;
  totalOther: number;
  grandTotal: number;
}

// ── Báo cáo lãi/lỗ ────────────────────────────────────────────────────────
export interface OrderProfit {
  orderId: string;
  orderNumber: string;
  customerName?: string;
  revenueDate: string;
  statusName: string;
  revenue: number;
  costAmount: number;
  shippingCost: number;
  outboundShippingCost: number;
  otherCost: number;
  totalCost: number;
  profit: number;
  profitMargin: number;
  hasCost: boolean;
}

export interface OrderProfitResult {
  items: OrderProfit[];
  totalCount: number;
  page: number;
  pageSize: number;
  totalPages: number;
  totalRevenue: number;
  totalCost: number;
  totalProfit: number;
  averageMargin: number;
  ordersWithoutCost: number;
  revenueWithoutCost: number;
}

export interface MonthlyProfit {
  year: number;
  month: number;
  label: string;
  orderCount: number;
  ordersWithoutCost: number;
  revenue: number;
  cogs: number;
  grossProfit: number;
  payrollCost: number;
  fixedCost: number;
  netProfit: number;
  netMargin: number;
}

export interface MonthlyProfitResult {
  year: number;
  revenueBasis: string;
  months: MonthlyProfit[];
  total: MonthlyProfit;
}

export interface MonthlyProfitDetail {
  summary: MonthlyProfit;
  fixedByCategory: ExpenseCategoryTotal[];
  payrollEntries: PayrollEntry[];
  orders: OrderProfit[];
}
