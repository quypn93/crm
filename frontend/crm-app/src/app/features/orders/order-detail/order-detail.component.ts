import { Component, OnInit } from '@angular/core';
import { ActivatedRoute, Router } from '@angular/router';
import { forkJoin } from 'rxjs';
import { OrderService } from '../../../core/services/order.service';
import { AuthService } from '../../../core/services/auth.service';
import { ToastService } from '../../../core/services/toast.service';
import { ProductionService, OrderProductionProgress, OrderProductionStep } from '../../../core/services/production.service';
import { UserManagementService, UserListItem } from '../../../core/services/user-management.service';
import { SettingsService } from '../../../core/services/settings.service';
import { LocationService } from '../../../core/services/location.service';
import { SenderAddress, VtpCategory } from '../../../core/models/lookup.model';
import { Province, Ward } from '../../../core/models/location.model';
import { ProcessWaybillPayload } from '../../../core/services/production.service';
import { environment } from '../../../../environments/environment';
import {
  Order,
  OrderStatus,
  PaymentStatus,
  DeliveryMethod,
  OrderStatusLabels,
  PaymentStatusLabels,
  DeliveryMethodLabels,
  OrderStatusColors,
  PaymentStatusColors,
  UpdateOrderStatusRequest,
  UpdateDeliveryMethodRequest,
  UpdateDepositCodeRequest,
  UpdatePaymentRequest
} from '../../../core/models/order.model';

@Component({
  selector: 'app-order-detail',
  templateUrl: './order-detail.component.html',
  styleUrls: ['./order-detail.component.scss']
})
export class OrderDetailComponent implements OnInit {
  order: Order | null = null;
  isLoading = true;
  errorMessage = '';
  productionProgress: OrderProductionProgress | null = null;
  isGeneratingQr = false;

  // Status update modal
  showStatusModal = false;
  newStatus: OrderStatus | null = null;
  statusNotes = '';
  availableStatuses: OrderStatus[] = [];

  // Payment update modal
  showPaymentModal = false;
  paymentAmount = 0;
  paymentMethod = '';
  paymentNotes = '';

  // Deposit code inline edit
  isEditingDepositCode = false;
  editDepositCode = '';
  isSavingDepositCode = false;
  depositCodeError = '';

  // Delivery method update modal
  showDeliveryModal = false;
  editDeliveryMethod: DeliveryMethod | null = null;
  editShipperUserId = '';
  isSavingDelivery = false;
  deliveryError = '';
  shippers: UserListItem[] = [];
  // GHTK đã bỏ khỏi danh sách lựa chọn (khớp order-form) — chỉ giữ hiển thị cho đơn cũ
  readonly deliveryMethodOptions: { value: DeliveryMethod; label: string }[] = [
    { value: DeliveryMethod.InHouse, label: DeliveryMethodLabels[DeliveryMethod.InHouse] },
    { value: DeliveryMethod.Vehicle, label: DeliveryMethodLabels[DeliveryMethod.Vehicle] },
    { value: DeliveryMethod.ViettelPost, label: DeliveryMethodLabels[DeliveryMethod.ViettelPost] }
  ];

  // Complete production step modal
  showCompleteStepModal = false;
  completingStageId = '';
  completingStageName = '';
  completeStepNotes = '';
  isCompletingStep = false;
  completeStepError = '';

  constructor(
    private orderService: OrderService,
    private authService: AuthService,
    private productionService: ProductionService,
    private userService: UserManagementService,
    private settingsService: SettingsService,
    private locationService: LocationService,
    private route: ActivatedRoute,
    private router: Router,
    private toast: ToastService
  ) {}

  ngOnInit(): void {
    const id = this.route.snapshot.paramMap.get('id');
    if (id) {
      this.loadOrder(id);
    }
  }

  loadOrder(id: string): void {
    this.isLoading = true;

    // Load order and allowed statuses in parallel
    forkJoin({
      order: this.orderService.getOrder(id),
      allowedStatuses: this.orderService.getAllowedStatuses(id)
    }).subscribe({
      next: (result) => {
        this.order = result.order;
        this.availableStatuses = result.allowedStatuses;
        this.isLoading = false;
        // Load production progress for any non-draft order
        if (result.order.status > OrderStatus.Draft) {
          this.loadProductionProgress(result.order.id);
        }
      },
      error: () => {
        this.errorMessage = 'Không thể tải thông tin đơn hàng';
        this.isLoading = false;
      }
    });
  }

  // Permission checks for UI
  canUpdateStatus(): boolean {
    return this.availableStatuses.length > 0;
  }

  canUpdatePayment(): boolean {
    return this.authService.canUpdatePayment();
  }

  canDeleteOrder(): boolean {
    // Chỉ cho xóa đơn Nháp (khớp ràng buộc backend)
    return this.order?.status === OrderStatus.Draft && this.authService.canDeleteOrders();
  }

  canCancelOrder(): boolean {
    if (!this.order) return false;
    if (this.order.status === OrderStatus.Confirmed) {
      return this.authService.hasAnyRole(['Admin', 'SalesManager', 'SalesRep']);
    }
    if (this.order.status === OrderStatus.InProduction) {
      return this.authService.hasAnyRole(['Admin', 'SalesManager']);
    }
    return false;
  }

  canEditOrder(): boolean {
    const editableStatuses = [OrderStatus.Draft, OrderStatus.Confirmed];
    return editableStatuses.includes(this.order?.status as OrderStatus) &&
           this.authService.hasAnyRole(['Admin', 'SalesManager', 'SalesRep']);
  }

  canConfirmOrder(): boolean {
    return this.order?.status === OrderStatus.Draft &&
           this.authService.hasAnyRole(['Admin', 'SalesManager', 'SalesRep']);
  }

  // Đơn được coi là "có thiết kế" khi đã upload ĐỦ CẢ ảnh VÀ file thiết kế
  hasDesign(): boolean {
    return !!(this.order?.designImageUrl && this.order?.designFileUrl);
  }

  // Nêu rõ đang thiếu ảnh hay thiếu file, để sale biết bảo thiết kế bổ sung cái gì.
  missingDesignText(): string {
    const noImage = !this.order?.designImageUrl;
    const noFile = !this.order?.designFileUrl;
    if (noImage && noFile) return 'ảnh và file thiết kế';
    return noImage ? 'ảnh thiết kế' : 'file thiết kế';
  }

  // Đơn Đã xác nhận nhưng chưa có thiết kế → chưa thể chuyển sang sản xuất
  showMissingDesignWarning(): boolean {
    return this.order?.status === OrderStatus.Confirmed && !this.hasDesign();
  }

  // Chưa đến khâu vận chuyển (Shipping) thì vẫn đổi được hình thức vận chuyển
  canEditDeliveryMethod(): boolean {
    if (!this.order) return false;
    const editableStatuses = [
      OrderStatus.Draft, OrderStatus.Confirmed, OrderStatus.InProduction,
      OrderStatus.QualityCheck, OrderStatus.ReadyToShip
    ];
    return editableStatuses.includes(this.order.status) &&
           this.authService.hasAnyRole(['Admin', 'SalesManager', 'SalesRep', 'DeliveryManager']);
  }

  // Mã cọc tiền: sale sửa được ở mọi trạng thái đơn
  canEditDepositCode(): boolean {
    return this.authService.hasAnyRole(['Admin', 'SalesManager', 'SalesRep']);
  }

  startEditDepositCode(): void {
    this.editDepositCode = this.order?.depositCode || '';
    this.depositCodeError = '';
    this.isEditingDepositCode = true;
  }

  cancelEditDepositCode(): void {
    this.isEditingDepositCode = false;
    this.depositCodeError = '';
  }

  saveDepositCode(): void {
    if (!this.order) return;
    this.isSavingDepositCode = true;
    this.depositCodeError = '';
    const request: UpdateDepositCodeRequest = { depositCode: this.editDepositCode.trim() || undefined };
    this.orderService.updateDepositCode(this.order.id, request).subscribe({
      next: (updated) => {
        this.order = updated;
        this.isSavingDepositCode = false;
        this.isEditingDepositCode = false;
        this.toast.success('Đã cập nhật mã cọc tiền.');
      },
      error: (err) => {
        this.isSavingDepositCode = false;
        this.depositCodeError = err?.error?.message || 'Cập nhật mã cọc tiền thất bại.';
      }
    });
  }

  openDeliveryModal(): void {
    if (!this.order) return;
    this.editDeliveryMethod = this.order.deliveryMethod ?? DeliveryMethod.InHouse;
    this.editShipperUserId = this.order.shipperUserId || '';
    this.deliveryError = '';
    this.showDeliveryModal = true;
    if (this.shippers.length === 0) {
      this.userService.getUsers({ page: 1, pageSize: 200, isActive: true, role: 'DeliveryStaff' }).subscribe({
        next: (res) => { this.shippers = res?.items || []; },
        error: () => { /* not critical */ }
      });
    }
  }

  closeDeliveryModal(): void {
    this.showDeliveryModal = false;
    this.deliveryError = '';
  }

  saveDeliveryMethod(): void {
    if (!this.order || this.editDeliveryMethod === null) return;
    this.isSavingDelivery = true;
    this.deliveryError = '';
    const request: UpdateDeliveryMethodRequest = {
      deliveryMethod: this.editDeliveryMethod,
      shipperUserId: this.editDeliveryMethod === DeliveryMethod.InHouse ? (this.editShipperUserId || undefined) : undefined
    };
    this.orderService.updateDeliveryMethod(this.order.id, request).subscribe({
      next: (updated) => {
        this.order = updated;
        this.isSavingDelivery = false;
        this.closeDeliveryModal();
        this.toast.success('Đã cập nhật hình thức vận chuyển.');
      },
      error: (err) => {
        this.isSavingDelivery = false;
        this.deliveryError = err?.error?.message || 'Cập nhật hình thức vận chuyển thất bại.';
      }
    });
  }

  confirmOrder(): void {
    if (!this.order) return;
    const req: UpdateOrderStatusRequest = { status: OrderStatus.Confirmed, notes: 'Sale xác nhận đơn hàng' };
    this.orderService.updateOrderStatus(this.order.id, req).subscribe({
      next: (updated) => {
        this.order = updated;
        this.orderService.getAllowedStatuses(updated.id).subscribe(s => this.availableStatuses = s);
        this.loadProductionProgress(updated.id);
      },
      error: (err) => { this.errorMessage = err.error?.message || 'Xác nhận đơn thất bại'; }
    });
  }

  getStatusLabel(status: OrderStatus): string {
    return OrderStatusLabels[status] || 'Không xác định';
  }

  getPaymentStatusLabel(status: PaymentStatus): string {
    return PaymentStatusLabels[status] || 'Không xác định';
  }

  getStatusColor(status: OrderStatus): string {
    return OrderStatusColors[status] || '#6c757d';
  }

  getPaymentStatusColor(status: PaymentStatus): string {
    return PaymentStatusColors[status] || '#6c757d';
  }

  formatCurrency(value: number): string {
    return new Intl.NumberFormat('vi-VN', { style: 'currency', currency: 'VND' }).format(value);
  }

  editOrder(): void {
    if (this.order) {
      this.router.navigate(['/orders', this.order.id, 'edit']);
    }
  }

  goBack(): void {
    this.router.navigate(['/orders']);
  }

  // Status Update
  openStatusModal(): void {
    if (this.availableStatuses.length > 0) {
      this.newStatus = this.availableStatuses[0];
      this.statusNotes = '';
      this.showStatusModal = true;
    }
  }

  closeStatusModal(): void {
    this.showStatusModal = false;
    this.newStatus = null;
    this.statusNotes = '';
  }

  updateStatus(): void {
    if (!this.order || this.newStatus === null) return;

    // Gửi xưởng: phải có đủ cả ảnh VÀ file thiết kế (backend cũng chặn)
    if (this.newStatus === OrderStatus.InProduction && !this.hasDesign()) {
      this.toast.error(`Đơn hàng chưa có ${this.missingDesignText()}. Thiết kế cần upload đủ cả ảnh và file trước khi gửi xưởng.`);
      return;
    }

    // Xác nhận trước khi chuyển sang InProduction — sau bước này không sửa được đơn nữa.
    if (this.newStatus === OrderStatus.InProduction) {
      const confirmed = confirm(
        `Chuyển đơn ${this.order.orderNumber} sang trạng thái "Đang sản xuất"?\n\n` +
        `Sau khi chuyển, đơn hàng sẽ KHÔNG thể chỉnh sửa được nữa.`
      );
      if (!confirmed) return;
    }

    const request: UpdateOrderStatusRequest = {
      status: this.newStatus,
      notes: this.statusNotes
    };

    this.orderService.updateOrderStatus(this.order.id, request).subscribe({
      next: (updatedOrder) => {
        this.order = updatedOrder;
        // Reload allowed statuses from API
        this.orderService.getAllowedStatuses(updatedOrder.id).subscribe({
          next: (statuses) => {
            this.availableStatuses = statuses;
          }
        });
        this.closeStatusModal();
      },
      error: (error) => {
        this.errorMessage = error.error?.message || 'Cập nhật trạng thái thất bại';
      }
    });
  }

  // Payment Update
  openPaymentModal(): void {
    this.paymentAmount = this.order?.remainingAmount || 0;
    this.paymentMethod = '';
    this.paymentNotes = '';
    this.showPaymentModal = true;
  }

  closePaymentModal(): void {
    this.showPaymentModal = false;
    this.paymentAmount = 0;
    this.paymentMethod = '';
    this.paymentNotes = '';
  }

  updatePayment(): void {
    if (!this.order) return;

    const request: UpdatePaymentRequest = {
      paidAmount: this.paymentAmount,
      paymentMethod: this.paymentMethod,
      paymentNotes: this.paymentNotes
    };

    this.orderService.updatePayment(this.order.id, request).subscribe({
      next: (updatedOrder) => {
        this.order = updatedOrder;
        this.closePaymentModal();
      },
      error: (error) => {
        this.errorMessage = error.error?.message || 'Cập nhật thanh toán thất bại';
      }
    });
  }

  deleteOrder(): void {
    if (!this.order) return;
    const orderNumber = this.order.orderNumber;
    if (!confirm(`Bạn có chắc muốn xóa đơn hàng "${orderNumber}"?\n\nChỉ có thể xóa đơn ở trạng thái Nháp.`)) return;
    this.orderService.deleteOrder(this.order.id).subscribe({
      next: () => {
        this.toast.success(`Đã xóa đơn hàng ${orderNumber}.`);
        this.router.navigate(['/orders']);
      },
      error: (error) => {
        const msg = error?.error?.message || 'Xóa đơn hàng thất bại';
        this.errorMessage = msg;
        this.toast.error(msg);
      }
    });
  }

  cancelOrder(): void {
    if (!this.order) return;
    const orderNumber = this.order.orderNumber;
    if (!confirm(`Bạn có chắc muốn hủy đơn hàng "${orderNumber}"?\n\nTrạng thái sẽ chuyển sang "Đã hủy" và không thể hoàn tác.`)) return;
    const req: UpdateOrderStatusRequest = { status: OrderStatus.Cancelled, notes: 'Hủy đơn từ trang chi tiết' };
    const orderId = this.order.id;
    this.orderService.updateOrderStatus(orderId, req).subscribe({
      next: () => {
        this.toast.success(`Đã hủy đơn hàng ${orderNumber}.`);
        this.loadOrder(orderId);
      },
      error: (error) => {
        const msg = error?.error?.message || 'Hủy đơn hàng thất bại';
        this.errorMessage = msg;
        this.toast.error(msg);
      }
    });
  }

  generateQr(): void {
    if (!this.order) return;
    this.isGeneratingQr = true;
    this.orderService.generateQr(this.order.id).subscribe({
      next: (updated) => { this.order = updated; this.isGeneratingQr = false; },
      error: () => { this.isGeneratingQr = false; alert('Không thể tạo QR code.'); }
    });
  }

  canCompleteProductionStep(): boolean {
    return this.authService.hasAnyRole(['Admin', 'ProductionManager', 'ProductionStaff', 'QualityControl', 'QualityManager']);
  }

  // Kiểm tra chất lượng & Đóng gói bắt buộc các khâu trước phải xong; các khâu khác tự do thứ tự
  private static readonly STRICT_ORDER_ROLES = ['QualityControl', 'QualityManager', 'PackagingStaff'];

  isStepBlockedBySequence(step: OrderProductionStep): boolean {
    if (!step.responsibleRole || !OrderDetailComponent.STRICT_ORDER_ROLES.includes(step.responsibleRole)) {
      return false;
    }
    return !!this.productionProgress?.steps?.some(s => !s.isCompleted && s.stageOrder < step.stageOrder);
  }

  canCompleteStepNow(step: OrderProductionStep): boolean {
    return !step.isCompleted && this.canCompleteProductionStep() && !this.isStepBlockedBySequence(step);
  }

  openCompleteStepModal(stageId: string, stageName: string): void {
    this.completingStageId = stageId;
    this.completingStageName = stageName;
    this.completeStepNotes = '';
    this.completeStepError = '';
    this.showCompleteStepModal = true;
  }

  closeCompleteStepModal(): void {
    this.showCompleteStepModal = false;
    this.completingStageId = '';
    this.completingStageName = '';
  }

  confirmCompleteStep(): void {
    if (!this.order || !this.completingStageId) return;
    this.isCompletingStep = true;
    this.completeStepError = '';
    this.productionService.completeStep(this.order.id, this.completingStageId, { notes: this.completeStepNotes }).subscribe({
      next: () => {
        this.isCompletingStep = false;
        this.closeCompleteStepModal();
        // Reload cả đơn hàng: hoàn thành khâu cuối sẽ tự chuyển trạng thái đơn (QualityCheck)
        this.loadOrder(this.order!.id);
      },
      error: (err) => {
        this.isCompletingStep = false;
        this.completeStepError = err.error?.message || 'Hoàn thành thất bại.';
      }
    });
  }

  loadProductionProgress(orderId: string): void {
    this.productionService.getOrderProgress(orderId).subscribe({
      next: (p) => {
        this.productionProgress = p;
        if (this.isAtWaybillStage()) this.loadWaybillData();
      },
      error: () => { /* not critical */ }
    });
  }

  // ── Khâu Vận đơn (sau Đóng gói) — chỉ chọn KHO GỬI rồi tạo vận đơn ──
  wbBusy = false;
  wbError = '';
  wbLoaded = false;
  wbSenders: SenderAddress[] = [];
  wb = { senderAddressId: '', notes: '' };

  // Đơn đang ở khâu Vận đơn: bước "Vận đơn" chưa xong + mọi khâu trước đã xong.
  isAtWaybillStage(): boolean {
    const steps = this.productionProgress?.steps;
    if (!steps?.length) return false;
    const wb = steps.find(s => s.stageName === 'Vận đơn');
    if (!wb || wb.isCompleted) return false;
    return steps.filter(s => s.stageOrder < wb.stageOrder).every(s => s.isCompleted);
  }

  private loadWaybillData(): void {
    if (this.wbLoaded) return;
    this.wbLoaded = true;
    this.wb.senderAddressId = this.order?.senderAddressId || '';
    this.settingsService.getSenderAddresses().subscribe(a => {
      this.wbSenders = (a || []).filter(x => x.isActive);
      if (!this.wb.senderAddressId) {
        const def = this.wbSenders.find(x => x.isDefault);
        if (def) this.wb.senderAddressId = def.id;
      }
    });
  }

  wbSenderLine(): string {
    const s = this.wbSenders.find(x => x.id === this.wb.senderAddressId);
    return s ? [s.address, s.wardName, s.districtName, s.provinceName].filter(Boolean).join(', ') : '';
  }

  // Địa chỉ người nhận (đã nhập lúc tạo đơn) — hiển thị để đối chiếu.
  wbReceiverLine(): string {
    if (!this.order) return '';
    return [this.order.shippingAddress, this.order.shippingWardName, this.order.shippingProvinceName]
      .filter(Boolean).join(', ');
  }

  processWaybill(): void {
    if (!this.order) return;
    const isVtp = this.isViettelPostOrder();
    if (isVtp && !this.wb.senderAddressId) {
      this.wbError = 'Vui lòng chọn kho gửi.'; return;
    }
    const payload: ProcessWaybillPayload = {
      senderAddressId: this.wb.senderAddressId || undefined,
      notes: this.wb.notes || undefined,
    };
    if (!confirm(isVtp ? 'Tạo vận đơn Viettel Post và hoàn tất khâu Vận đơn?' : 'Hoàn tất khâu Vận đơn?')) return;
    this.wbBusy = true; this.wbError = '';
    this.productionService.processWaybill(this.order.id, payload).subscribe({
      next: () => { this.wbBusy = false; this.toast.success('Đã xử lý vận đơn và hoàn tất khâu.'); this.wbLoaded = false; this.loadOrder(this.order!.id); },
      error: (err) => { this.wbBusy = false; this.wbError = err?.error?.message || 'Xử lý vận đơn thất bại.'; }
    });
  }

  // ── Sửa địa chỉ giao hàng (cho tới trước khâu vận chuyển, kể cả đang SX) ──
  showShippingModal = false;
  saBusy = false;
  saError = '';
  saProvinces: VtpCategory[] = [];
  saDistricts: VtpCategory[] = [];
  saWards: VtpCategory[] = [];
  saCrmProvinces: Province[] = [];
  saCrmWards: Ward[] = [];
  sa = { contactName: '', phone: '', address: '', provinceCode: '', wardCode: '', provinceId: 0, districtId: 0, wardId: 0, notes: '' };

  canEditShippingAddress(): boolean {
    if (!this.order) return false;
    const editable = [OrderStatus.Draft, OrderStatus.Confirmed, OrderStatus.InProduction, OrderStatus.QualityCheck, OrderStatus.ReadyToShip];
    return editable.includes(this.order.status) &&
           this.authService.hasAnyRole(['Admin', 'SalesManager', 'SalesRep', 'DeliveryManager']);
  }

  openEditShipping(): void {
    if (!this.order) return;
    this.saError = '';
    this.sa = {
      contactName: this.order.shippingContactName || '',
      phone: this.order.shippingPhone || '',
      address: this.order.shippingAddress || '',
      provinceCode: this.order.shippingProvinceCode || '',
      wardCode: this.order.shippingWardCode || '',
      provinceId: this.order.receiverProvinceId || 0,
      districtId: this.order.receiverDistrictId || 0,
      wardId: this.order.receiverWardId || 0,
      notes: this.order.shippingNotes || ''
    };
    if (this.isViettelPostOrder()) {
      this.settingsService.getVtpProvinces().subscribe(p => this.saProvinces = p || []);
      if (this.sa.provinceId) this.settingsService.getVtpDistricts(this.sa.provinceId).subscribe(d => this.saDistricts = d || []);
      if (this.sa.districtId) this.settingsService.getVtpWards(this.sa.districtId).subscribe(w => this.saWards = w || []);
    } else {
      this.locationService.getProvinces().subscribe(p => this.saCrmProvinces = p || []);
      if (this.sa.provinceCode) this.locationService.getWardsByProvince(this.sa.provinceCode).subscribe(w => this.saCrmWards = w || []);
    }
    this.showShippingModal = true;
  }

  onSaVtpProvince(): void {
    this.sa.districtId = 0; this.sa.wardId = 0; this.saDistricts = []; this.saWards = [];
    if (this.sa.provinceId) this.settingsService.getVtpDistricts(Number(this.sa.provinceId)).subscribe(d => this.saDistricts = d || []);
  }
  onSaVtpDistrict(): void {
    this.sa.wardId = 0; this.saWards = [];
    if (this.sa.districtId) this.settingsService.getVtpWards(Number(this.sa.districtId)).subscribe(w => this.saWards = w || []);
  }
  onSaCrmProvince(): void {
    this.sa.wardCode = ''; this.saCrmWards = [];
    if (this.sa.provinceCode) this.locationService.getWardsByProvince(this.sa.provinceCode).subscribe(w => this.saCrmWards = w || []);
  }

  saveShippingAddress(): void {
    if (!this.order) return;
    const isVtp = this.isViettelPostOrder();
    if (!this.sa.contactName || !this.sa.phone || !this.sa.address) { this.saError = 'Nhập đủ người nhận, số điện thoại, địa chỉ.'; return; }
    let payload: any = {
      shippingContactName: this.sa.contactName,
      shippingPhone: this.sa.phone,
      shippingAddress: this.sa.address,
      shippingNotes: this.sa.notes || undefined
    };
    if (isVtp) {
      if (!this.sa.provinceId || !this.sa.districtId || !this.sa.wardId) { this.saError = 'Chọn đủ Tỉnh / Quận-Huyện / Phường-Xã (VTP).'; return; }
      const p = this.saProvinces.find(x => x.PROVINCE_ID === Number(this.sa.provinceId));
      const w = this.saWards.find(x => x.WARDS_ID === Number(this.sa.wardId));
      payload.shippingProvinceName = p?.PROVINCE_NAME;
      payload.shippingWardName = w?.WARDS_NAME;
      payload.receiverProvinceId = Number(this.sa.provinceId);
      payload.receiverDistrictId = Number(this.sa.districtId);
      payload.receiverWardId = Number(this.sa.wardId);
    } else {
      if (!this.sa.provinceCode || !this.sa.wardCode) { this.saError = 'Chọn Tỉnh/Thành và Phường/Xã.'; return; }
      const p = this.saCrmProvinces.find(x => x.code === this.sa.provinceCode);
      const w = this.saCrmWards.find(x => x.code === this.sa.wardCode);
      payload.shippingProvinceCode = this.sa.provinceCode;
      payload.shippingProvinceName = p?.fullName;
      payload.shippingWardCode = this.sa.wardCode;
      payload.shippingWardName = w?.name;
    }
    this.saBusy = true; this.saError = '';
    this.orderService.updateShippingAddress(this.order.id, payload).subscribe({
      next: (o) => { this.saBusy = false; this.order = o; this.showShippingModal = false; this.toast.success('Đã cập nhật địa chỉ giao hàng.'); },
      error: (err) => { this.saBusy = false; this.saError = err?.error?.message || 'Cập nhật địa chỉ thất bại.'; }
    });
  }

  printQr(): void {
    if (!this.order?.qrCodeImageBase64) return;
    const w = window.open('', '_blank');
    if (!w) return;
    w.document.write(`
      <html><head><title>QR - ${this.order.orderNumber}</title>
      <style>body{display:flex;flex-direction:column;align-items:center;justify-content:center;height:100vh;margin:0;font-family:sans-serif;}
      img{width:200px;height:200px;} p{font-size:18px;font-weight:bold;margin-top:8px;}</style></head>
      <body>
        <img src="data:image/png;base64,${this.order.qrCodeImageBase64}">
        <p>${this.order.orderNumber}</p>
        <script>window.onload=()=>{window.print();window.close();}</script>
      </body></html>`);
    w.document.close();
  }

  // Designer upload
  isUploadingDesign = false;
  isUploadingDesignFile = false;
  designUploadError = '';

  // GHTK state
  readonly DeliveryMethod = DeliveryMethod;
  isGhtkBusy = false;
  ghtkError = '';

  isGhtkOrder(): boolean {
    return this.order?.deliveryMethod === DeliveryMethod.GHTK;
  }

  canCreateGhtk(): boolean {
    return this.isGhtkOrder()
      && !this.order?.ghtkLabel
      && this.authService.hasAnyRole(['Admin', 'SalesManager', 'SalesRep', 'DeliveryManager', 'DeliveryStaff']);
  }

  canCancelGhtk(): boolean {
    return this.isGhtkOrder()
      && !!this.order?.ghtkLabel
      && this.order?.status !== OrderStatus.Delivered
      && this.order?.status !== OrderStatus.Completed
      && this.authService.hasAnyRole(['Admin', 'SalesManager', 'DeliveryManager']);
  }

  estimateGhtkFee(): void {
    if (!this.order) return;
    this.isGhtkBusy = true;
    this.ghtkError = '';
    this.orderService.estimateGhtkFee(this.order.id).subscribe({
      next: (fee) => {
        this.isGhtkBusy = false;
        this.toast.success(`Phí GHTK ước tính: ${this.formatCurrency(fee.fee + fee.insuranceFee)}`);
        this.loadOrder(this.order!.id);
      },
      error: (err) => {
        this.isGhtkBusy = false;
        this.ghtkError = err?.error?.message || 'Không lấy được phí GHTK.';
      }
    });
  }

  createGhtkShipment(): void {
    if (!this.order) return;
    if (!confirm(`Tạo vận đơn GHTK cho đơn ${this.order.orderNumber}?`)) return;
    this.isGhtkBusy = true;
    this.ghtkError = '';
    this.orderService.createGhtkShipment(this.order.id).subscribe({
      next: () => {
        this.isGhtkBusy = false;
        this.toast.success('Đã tạo vận đơn GHTK.');
        this.loadOrder(this.order!.id);
      },
      error: (err) => {
        this.isGhtkBusy = false;
        this.ghtkError = err?.error?.message || 'Tạo vận đơn GHTK thất bại.';
      }
    });
  }

  cancelGhtkShipment(): void {
    if (!this.order) return;
    if (!confirm(`Huỷ vận đơn GHTK ${this.order.ghtkLabel}?`)) return;
    this.isGhtkBusy = true;
    this.ghtkError = '';
    this.orderService.cancelGhtkShipment(this.order.id).subscribe({
      next: () => {
        this.isGhtkBusy = false;
        this.toast.success('Đã huỷ vận đơn GHTK.');
        this.loadOrder(this.order!.id);
      },
      error: (err) => {
        this.isGhtkBusy = false;
        this.ghtkError = err?.error?.message || 'Huỷ vận đơn GHTK thất bại.';
      }
    });
  }

  syncGhtkStatus(): void {
    if (!this.order) return;
    this.isGhtkBusy = true;
    this.ghtkError = '';
    this.orderService.syncGhtkStatus(this.order.id).subscribe({
      next: () => {
        this.isGhtkBusy = false;
        this.loadOrder(this.order!.id);
      },
      error: (err) => {
        this.isGhtkBusy = false;
        this.ghtkError = err?.error?.message || 'Đồng bộ trạng thái GHTK thất bại.';
      }
    });
  }

  // ── Viettel Post ────────────────────────────────────────────────────
  isViettelPostBusy = false;
  viettelPostError = '';

  isViettelPostOrder(): boolean {
    return this.order?.deliveryMethod === DeliveryMethod.ViettelPost;
  }

  canCreateViettelPost(): boolean {
    return this.isViettelPostOrder()
      && !this.order?.viettelPostLabel
      && this.authService.hasAnyRole(['Admin', 'SalesManager', 'SalesRep', 'DeliveryManager', 'DeliveryStaff']);
  }

  canCancelViettelPost(): boolean {
    return this.isViettelPostOrder()
      && !!this.order?.viettelPostLabel
      && this.order?.status !== OrderStatus.Delivered
      && this.order?.status !== OrderStatus.Completed
      && this.authService.hasAnyRole(['Admin', 'SalesManager', 'DeliveryManager']);
  }

  estimateViettelPostFee(): void {
    if (!this.order) return;
    this.isViettelPostBusy = true;
    this.viettelPostError = '';
    this.orderService.estimateViettelPostFee(this.order.id).subscribe({
      next: (fee) => {
        this.isViettelPostBusy = false;
        this.toast.success(`Phí Viettel Post ước tính: ${this.formatCurrency(fee.fee + fee.insuranceFee)}`);
        this.loadOrder(this.order!.id);
      },
      error: (err) => {
        this.isViettelPostBusy = false;
        this.viettelPostError = err?.error?.message || 'Không lấy được phí Viettel Post.';
      }
    });
  }

  createViettelPostShipment(): void {
    if (!this.order) return;
    if (!confirm(`Tạo vận đơn Viettel Post cho đơn ${this.order.orderNumber}?`)) return;
    this.isViettelPostBusy = true;
    this.viettelPostError = '';
    this.orderService.createViettelPostShipment(this.order.id).subscribe({
      next: () => {
        this.isViettelPostBusy = false;
        this.toast.success('Đã tạo vận đơn Viettel Post.');
        this.loadOrder(this.order!.id);
      },
      error: (err) => {
        this.isViettelPostBusy = false;
        this.viettelPostError = err?.error?.message || 'Tạo vận đơn Viettel Post thất bại.';
      }
    });
  }

  cancelViettelPostShipment(): void {
    if (!this.order) return;
    if (!confirm(`Huỷ vận đơn Viettel Post ${this.order.viettelPostLabel}?`)) return;
    this.isViettelPostBusy = true;
    this.viettelPostError = '';
    this.orderService.cancelViettelPostShipment(this.order.id).subscribe({
      next: () => {
        this.isViettelPostBusy = false;
        this.toast.success('Đã huỷ vận đơn Viettel Post.');
        this.loadOrder(this.order!.id);
      },
      error: (err) => {
        this.isViettelPostBusy = false;
        this.viettelPostError = err?.error?.message || 'Huỷ vận đơn Viettel Post thất bại.';
      }
    });
  }

  syncViettelPostStatus(): void {
    if (!this.order) return;
    this.isViettelPostBusy = true;
    this.viettelPostError = '';
    this.orderService.syncViettelPostStatus(this.order.id).subscribe({
      next: () => {
        this.isViettelPostBusy = false;
        this.loadOrder(this.order!.id);
      },
      error: (err) => {
        this.isViettelPostBusy = false;
        this.viettelPostError = err?.error?.message || 'Đồng bộ trạng thái Viettel Post thất bại.';
      }
    });
  }

  canUploadDesignImage(): boolean {
    return this.authService.hasAnyRole(['Admin', 'Designer']);
  }

  onDesignImageSelected(event: Event): void {
    const input = event.target as HTMLInputElement;
    const file = input.files?.[0];
    if (!file || !this.order) return;
    this.isUploadingDesign = true;
    this.designUploadError = '';
    this.orderService.uploadDesignImage(this.order.id, file).subscribe({
      next: (updated) => {
        this.order = updated;
        this.isUploadingDesign = false;
        input.value = '';
        this.toast.success('Đã cập nhật ảnh thiết kế.');
      },
      error: (err) => {
        this.isUploadingDesign = false;
        const msg = err?.error?.message || 'Upload thất bại.';
        this.designUploadError = msg;
        this.toast.error(msg);
      }
    });
  }

  onDesignFileSelected(event: Event): void {
    const input = event.target as HTMLInputElement;
    const file = input.files?.[0];
    if (!file || !this.order) return;
    this.isUploadingDesignFile = true;
    this.designUploadError = '';
    this.orderService.uploadDesignFile(this.order.id, file).subscribe({
      next: (updated) => {
        this.order = updated;
        this.isUploadingDesignFile = false;
        input.value = '';
        this.toast.success('Đã cập nhật file thiết kế.');
      },
      error: (err) => {
        this.isUploadingDesignFile = false;
        const msg = err?.error?.message || 'Upload file thiết kế thất bại.';
        this.designUploadError = msg;
        this.toast.error(msg);
      }
    });
  }

  apiOrigin(): string {
    // Static files (ví dụ /uploads/designs/...) được API serve cùng origin với apiUrl (bỏ đuôi /api).
    const url = environment.apiUrl || '';
    return url.replace(/\/api\/?$/, '');
  }

  resolveImageUrl(path?: string): string {
    if (!path) return '';
    if (path.startsWith('http')) return path;
    // path có thể là '/uploads/...' → prepend origin của API
    const origin = this.apiOrigin();
    return origin + (path.startsWith('/') ? path : '/' + path);
  }

  resolveFileUrl(path?: string): string {
    return this.resolveImageUrl(path);
  }

  printOrder(): void {
    window.print();
  }
}
