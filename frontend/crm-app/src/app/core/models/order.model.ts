export enum OrderStatus {
  Draft = 0,
  Confirmed = 1,
  InProduction = 2,
  QualityCheck = 3,
  ReadyToShip = 4,
  Shipping = 5,
  Delivered = 6,
  Completed = 7,
  Cancelled = 8
}

export enum PaymentStatus {
  Pending = 0,
  PartialPaid = 1,
  Paid = 2,
  Refunded = 3,
  Cancelled = 4
}

export const OrderStatusLabels: Record<OrderStatus, string> = {
  [OrderStatus.Draft]: 'Nháp',
  [OrderStatus.Confirmed]: 'Đã xác nhận',
  [OrderStatus.InProduction]: 'Đang sản xuất',
  [OrderStatus.QualityCheck]: 'Kiểm tra chất lượng',
  [OrderStatus.ReadyToShip]: 'Sẵn sàng giao',
  [OrderStatus.Shipping]: 'Đang giao hàng',
  [OrderStatus.Delivered]: 'Đã giao hàng',
  [OrderStatus.Completed]: 'Hoàn thành',
  [OrderStatus.Cancelled]: 'Đã hủy'
};

export const PaymentStatusLabels: Record<PaymentStatus, string> = {
  [PaymentStatus.Pending]: 'Chờ thanh toán',
  [PaymentStatus.PartialPaid]: 'Thanh toán một phần',
  [PaymentStatus.Paid]: 'Đã thanh toán',
  [PaymentStatus.Refunded]: 'Đã hoàn tiền',
  [PaymentStatus.Cancelled]: 'Đã hủy'
};

export const OrderStatusColors: Record<OrderStatus, string> = {
  [OrderStatus.Draft]: '#6c757d',
  [OrderStatus.Confirmed]: '#0d6efd',
  [OrderStatus.InProduction]: '#ffc107',
  [OrderStatus.QualityCheck]: '#17a2b8',
  [OrderStatus.ReadyToShip]: '#20c997',
  [OrderStatus.Shipping]: '#fd7e14',
  [OrderStatus.Delivered]: '#198754',
  [OrderStatus.Completed]: '#28a745',
  [OrderStatus.Cancelled]: '#dc3545'
};

export const PaymentStatusColors: Record<PaymentStatus, string> = {
  [PaymentStatus.Pending]: '#ffc107',
  [PaymentStatus.PartialPaid]: '#17a2b8',
  [PaymentStatus.Paid]: '#28a745',
  [PaymentStatus.Refunded]: '#6c757d',
  [PaymentStatus.Cancelled]: '#dc3545'
};

export interface OrderItem {
  id: string;
  orderId: string;
  collectionId?: string;
  collectionName?: string;
  productCode?: string;
  description?: string;
  size?: string;
  mainColorId?: string;
  accentColorId?: string;
  materialId?: string;
  formId?: string;
  specificationId?: string;
  mainColorName?: string;
  accentColorName?: string;
  materialName?: string;
  formName?: string;
  specificationName?: string;
  quantity: number;
  unit: string;
  unitPrice: number;
  discountPercent: number;
  discountAmount: number;
  lineTotal: number;
  notes?: string;
  sortOrder?: number;
}

export interface Order {
  id: string;
  orderNumber: string;
  customerId: string;
  customerName?: string;
  dealId?: string;
  dealTitle?: string;
  status: OrderStatus;
  statusName?: string;
  orderDate: Date;
  requiredDate?: Date;
  completionDate?: Date;
  returnDate?: Date;
  shippedDate?: Date;
  deliveryAddress?: string;
  deliveryCity?: string;
  deliveryDistrict?: string;
  deliveryWard?: string;
  deliveryPhone?: string;
  deliveryContactName?: string;
  subTotal: number;
  discountPercent: number;
  discountAmount: number;
  taxPercent: number;
  taxAmount: number;
  shippingFee: number;
  totalAmount: number;
  paidAmount: number;
  remainingAmount: number;
  paymentStatus: PaymentStatus;
  paymentStatusName?: string;
  paymentMethod?: string;
  paymentNotes?: string;
  internalNotes?: string;
  customerNotes?: string;
  styleNotes?: string;
  productionDaysOptionId?: string;
  productionDaysOptionName?: string;
  productionDays?: number;
  depositCode?: string;
  designImageUrl?: string;
  createdAt: Date;
  updatedAt?: Date;
  createdByUserId: string;
  createdByUserName?: string;
  assignedToUserId?: string;
  assignedToUserName?: string;
  items: OrderItem[];
  itemsCount: number;
  qrCodeToken?: string;
  qrCodeImageBase64?: string;
  designerUserId?: string;
  designerUserName?: string;
}

export interface CreateOrderItemRequest {
  collectionId?: string;
  productCode?: string;
  description?: string;
  size?: string;
  mainColorId?: string;
  accentColorId?: string;
  materialId?: string;
  formId?: string;
  specificationId?: string;
  quantity: number;
  unit?: string;
  unitPrice: number;
  discountPercent?: number;
  notes?: string;
}

export interface CreateOrderRequest {
  customerId?: string;
  customerName?: string;
  dealId?: string;
  orderDate?: Date;
  requiredDate?: Date;
  completionDate?: Date;
  returnDate?: Date;
  deliveryAddress?: string;
  deliveryCity?: string;
  deliveryDistrict?: string;
  deliveryWard?: string;
  deliveryPhone?: string;
  deliveryContactName?: string;
  discountPercent?: number;
  taxPercent?: number;
  shippingFee?: number;
  paymentMethod?: string;
  internalNotes?: string;
  customerNotes?: string;
  styleNotes?: string;
  productionDaysOptionId?: string;
  depositCode?: string;
  assignedToUserId?: string;
  designerUserId?: string;
  items: CreateOrderItemRequest[];
}

export interface UpdateOrderRequest extends CreateOrderRequest {
  id: string;
}

export interface UpdateOrderStatusRequest {
  status: OrderStatus;
  notes?: string;
}

export interface UpdatePaymentRequest {
  paidAmount: number;
  paymentMethod?: string;
  paymentNotes?: string;
}

export interface OrderFilter {
  search?: string;
  customerId?: string;
  status?: OrderStatus;
  paymentStatus?: PaymentStatus;
  assignedToUserId?: string;
  orderDateFrom?: Date;
  orderDateTo?: Date;
  minAmount?: number;
  maxAmount?: number;
  page: number;
  pageSize: number;
  sortBy?: string;
  sortOrder?: 'asc' | 'desc';
}

export interface OrderSummary {
  totalOrders: number;
  totalRevenue: number;
  pendingPaymentAmount: number;
  ordersByStatus: { status: OrderStatus; count: number; total: number }[];
}
