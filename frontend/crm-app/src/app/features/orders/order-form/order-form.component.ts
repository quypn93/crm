import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, FormArray, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { forkJoin, of } from 'rxjs';
import { OrderService } from '../../../core/services/order.service';
import { CustomerService, Customer } from '../../../core/services/customer.service';
import { DealService, Deal } from '../../../core/services/deal.service';
import { DesignService, ColorFabric, Design } from '../../../core/services/design.service';
import { SettingsService } from '../../../core/services/settings.service';
import { UserManagementService, UserListItem } from '../../../core/services/user-management.service';
import { AuthService } from '../../../core/services/auth.service';
import { LocationService } from '../../../core/services/location.service';
import { CreateOrderItemRequest, DeliveryMethod, DeliveryMethodLabels } from '../../../core/models/order.model';
import { Collection, LookupItem, ProductionDaysOption, DepositTransaction, SenderAddress } from '../../../core/models/lookup.model';
import { Province, Ward } from '../../../core/models/location.model';

@Component({
  selector: 'app-order-form',
  templateUrl: './order-form.component.html',
  styleUrls: ['./order-form.component.scss']
})
export class OrderFormComponent implements OnInit {
  orderForm: FormGroup;
  isLoading = false;
  isEditMode = false;
  orderId: string | null = null;
  errorMessage = '';

  customers: Customer[] = [];
  deals: Deal[] = [];
  filteredDeals: Deal[] = [];
  filteredCustomers: Customer[] = [];

  colorFabrics: ColorFabric[] = [];
  materials: LookupItem[] = [];
  shirtForms: LookupItem[] = [];
  styleSpecs: LookupItem[] = [];
  orderTypes: LookupItem[] = [];
  collections: Collection[] = [];
  senderAddresses: SenderAddress[] = [];
  productionDaysOptions: ProductionDaysOption[] = [];

  // Filtered by selected collection
  filteredMaterials: LookupItem[] = [];
  filteredColors: ColorFabric[] = [];
  filteredForms: LookupItem[] = [];
  filteredSpecs: LookupItem[] = [];

  users: UserListItem[] = [];
  designers: UserListItem[] = [];
  shippers: UserListItem[] = [];
  availableDesigns: Design[] = [];
  customerSearchText = '';
  showCustomerDropdown = false;

  provinces: Province[] = [];
  wards: Ward[] = [];
  isLoadingWards = false;

  // Deposit lookup theo mã giao dịch (sale gõ vào ô "Mã cọc tiền"):
  // null = chưa nhập mã, undefined = đã nhập nhưng không khớp, object = đã khớp.
  deposits: DepositTransaction[] = [];
  matchedDeposit: DepositTransaction | null | undefined = null;

  readonly DeliveryMethod = DeliveryMethod;
  readonly deliveryMethodOptions = [
    { value: DeliveryMethod.InHouse, label: DeliveryMethodLabels[DeliveryMethod.InHouse] },
    { value: DeliveryMethod.Vehicle, label: DeliveryMethodLabels[DeliveryMethod.Vehicle] },
    { value: DeliveryMethod.ViettelPost, label: DeliveryMethodLabels[DeliveryMethod.ViettelPost] }
  ];

  // null = chưa biết (đang load), true = đã cấu hình token + kho, false = chưa cấu hình.
  ghtkConfigured: boolean | null = null;
  viettelPostConfigured: boolean | null = null;

  // Size grid mirrors the order-card template: adult sizes split by gender, children sizes in a separate row.
  readonly ADULT_SIZES = ['S', 'M', 'L', 'XL', 'XXL', 'NC', 'TE'];
  readonly CHILD_SIZES = ['NC1', 'NC2', 'NC3'];
  readonly SIZE_LIST = [...this.ADULT_SIZES, ...this.CHILD_SIZES];
  readonly SIZE_GENDERS = [
    { key: 'NAM', label: 'NAM' },
    { key: 'NU', label: 'NỮ' }
  ];
  sizeQty: Record<string, number> = {};

  sizeKey(size: string, gender?: string): string {
    return gender ? `${gender}:${size}` : size;
  }

  getSizeQtyInput(size: string, gender?: string): number {
    return this.sizeQty[this.sizeKey(size, gender)] || 0;
  }

  onSizeQtyChange(size: string, event: Event, gender?: string): void {
    const val = parseInt((event.target as HTMLInputElement).value, 10);
    this.sizeQty[this.sizeKey(size, gender)] = isNaN(val) || val < 0 ? 0 : val;
  }
  getTotalQtyInput(): number {
    return Object.values(this.sizeQty).reduce((a, b) => a + b, 0);
  }

  // Form "Classic" chia NAM/NỮ; "Oversize"/"Unisex" dùng 1 dòng, không chia giới tính.
  // Mặc định (chưa chọn form) = chia giới tính như Classic.
  isGenderedForm(): boolean {
    const id = this.orderForm.get('productInfo.formId')?.value;
    const name = (this.shirtForms.find(f => f.id === id)?.name || '').toLowerCase();
    return !(name.includes('oversize') || name.includes('unisex'));
  }

  // Khi đổi giữa dạng chia giới tính ↔ 1 dòng, key size không tương thích (VD "NAM:S" vs "S")
  // nên xoá sizeQty để tránh cộng dồn nhầm số lượng cũ.
  private lastGendered: boolean | null = null;
  private onFormModeChange(): void {
    const gendered = this.isGenderedForm();
    if (this.lastGendered !== null && this.lastGendered !== gendered) {
      this.sizeQty = {};
    }
    this.lastGendered = gendered;
  }

  constructor(
    private fb: FormBuilder,
    private orderService: OrderService,
    private customerService: CustomerService,
    private dealService: DealService,
    private designService: DesignService,
    private settingsService: SettingsService,
    private userService: UserManagementService,
    private authService: AuthService,
    private locationService: LocationService,
    private router: Router,
    private route: ActivatedRoute
  ) {
    this.orderForm = this.fb.group({
      customerId: [''],
      dealId: [''],
      orderDate: [this.formatDate(new Date())],
      productionDaysOptionId: [''],
      orderTypeId: ['', Validators.required],
      completionDate: [{ value: '', disabled: true }],
      returnDate: [{ value: '', disabled: true }],
      depositCode: [''],
      assignedToUserId: [''],
      designerUserId: [''],
      shipperUserId: [''],
      designId: [''],
      deliveryMethod: ['', Validators.required],
      senderAddressId: [''],
      shippingContactName: ['', Validators.required],
      shippingPhone: ['', Validators.required],
      shippingAddress: ['', Validators.required],
      shippingProvinceCode: ['', Validators.required],
      shippingWardCode: ['', Validators.required],
      shippingNotes: [''],
      discountPercent: [0, [Validators.min(0), Validators.max(100)]],
      taxPercent: [0, [Validators.min(0), Validators.max(100)]],
      shippingFee: [0, [Validators.min(0)]],
      paymentMethod: [''],
      styleNotes: [''],
      internalNotes: [''],
      customerNotes: [''],
      productInfo: this.fb.group({
        collectionId: ['', Validators.required],
        materialId: ['', Validators.required],
        mainColorId: ['', Validators.required],
        accentColorId: [''],
        formId: ['', Validators.required],
        specificationId: ['', Validators.required],
        unitPrice: [0, [Validators.required, Validators.min(1)]],
        itemDiscountPercent: [0, [Validators.min(0), Validators.max(100)]],
      }),
      items: this.fb.array([])
    });
  }

  ngOnInit(): void {
    const id = this.route.snapshot.paramMap.get('id');
    if (id && id !== 'new') {
      this.isEditMode = true;
      this.orderId = id;
    }

    // Check GHTK config trạng thái — quyết định hint khi chọn hình thức GHTK.
    this.orderService.getGhtkStatus().subscribe({
      next: (s) => { this.ghtkConfigured = !!s?.configured; },
      error: () => { this.ghtkConfigured = false; }
    });
    this.orderService.getViettelPostStatus().subscribe({
      next: (s) => { this.viettelPostConfigured = !!s?.configured; },
      error: () => { this.viettelPostConfigured = false; }
    });

    forkJoin({
      customers: this.customerService.getCustomers({ page: 1, pageSize: 200 }),
      colors: this.designService.getAllColorFabrics(),
      materials: this.settingsService.getLookups('materials'),
      shirtForms: this.settingsService.getLookups('product-forms'),
      styleSpecs: this.settingsService.getLookups('product-specifications'),
      orderTypes: this.settingsService.getLookups('order-types'),
      collections: this.settingsService.getCollections(),
      senderAddresses: this.settingsService.getSenderAddresses(),
      productionDaysOptions: this.settingsService.getProductionDaysOptions(),
      users: this.userService.getUsers({ page: 1, pageSize: 200, isActive: true }),
      designers: this.userService.getUsers({ page: 1, pageSize: 200, isActive: true, role: 'Designer' }),
      shippers: this.userService.getUsers({ page: 1, pageSize: 200, isActive: true, role: 'DeliveryStaff' }),
      provinces: this.locationService.getProvinces(),
      availableDesigns: this.designService.getAvailableDesigns(),
      deposits: this.settingsService.getDeposits()
    }).subscribe({
      next: (res) => {
        this.customers = res.customers?.items || [];
        this.colorFabrics = res.colors || [];
        this.materials = res.materials || [];
        this.shirtForms = res.shirtForms || [];
        this.styleSpecs = res.styleSpecs || [];
        this.orderTypes = (res.orderTypes || []).filter(x => x.isActive);
        this.collections = res.collections || [];
        this.senderAddresses = (res.senderAddresses || []).filter(a => a.isActive);
        this.productionDaysOptions = (res.productionDaysOptions || []).filter(o => o.isActive);
        this.users = res.users?.items || [];
        this.designers = res.designers?.items || [];
        this.shippers = res.shippers?.items || [];
        this.provinces = res.provinces || [];
        this.availableDesigns = res.availableDesigns || [];
        this.deposits = res.deposits || [];

        this.resetAttributeFilters();
        this.updateColorControlsState();

        if (this.isEditMode) {
          this.loadOrder();
        } else {
          const currentUser = this.authService.getCurrentUser();
          if (currentUser) {
            this.orderForm.get('assignedToUserId')?.setValue(currentUser.id);
          }
          // Chọn sẵn địa chỉ gửi mặc định.
          const defaultSender = this.senderAddresses.find(a => a.isDefault);
          if (defaultSender) this.orderForm.get('senderAddressId')?.setValue(defaultSender.id);
        }

        // Re-match deposit nếu đã có sẵn depositCode (edit mode hoặc form đã prefill).
        this.onDepositCodeChange(this.orderForm.get('depositCode')?.value || '');
      }
    });

    this.orderForm.get('productionDaysOptionId')?.valueChanges.subscribe(() => this.recalcDates());
    this.orderForm.get('orderDate')?.valueChanges.subscribe(() => this.recalcDates());
    this.orderForm.get('productInfo.collectionId')?.valueChanges.subscribe((id: string) => this.onCollectionChange(id));
    this.orderForm.get('productInfo.formId')?.valueChanges.subscribe(() => this.onFormModeChange());
    this.orderForm.get('productInfo.materialId')?.valueChanges.subscribe(() => this.onMaterialChange());
    this.orderForm.get('shippingProvinceCode')?.valueChanges.subscribe((code: string) => this.onProvinceChange(code));
    this.orderForm.get('designId')?.valueChanges.subscribe((id: string) => this.onDesignChange(id));
    this.orderForm.get('depositCode')?.valueChanges.subscribe((code: string) => this.onDepositCodeChange(code));
    // Đổi sang Vehicle/GHTK → clear NV giao hàng (chỉ áp dụng cho hình thức "Nhà giao").
    this.orderForm.get('deliveryMethod')?.valueChanges.subscribe((v: any) => {
      if (Number(v) !== DeliveryMethod.InHouse) {
        this.orderForm.get('shipperUserId')?.setValue('', { emitEvent: false });
      }
    });
  }

  isInHouseDelivery(): boolean {
    const v = this.orderForm.get('deliveryMethod')?.value;
    return Number(v) === DeliveryMethod.InHouse;
  }

  /**
   * Khớp mã cọc tiền sale gõ vào với danh sách giao dịch ngân hàng.
   * - Trim + so sánh case-insensitive để chấp nhận cả gõ tay lẫn paste từ SMS.
   * - Trống → reset matchedDeposit về null (không trừ).
   * - Có gõ nhưng không khớp → set undefined (để hint UI báo "không khớp").
   */
  onDepositCodeChange(rawCode: string): void {
    const code = (rawCode || '').trim();
    if (!code) { this.matchedDeposit = null; return; }
    const found = this.deposits.find(d => (d.code || '').trim().toLowerCase() === code.toLowerCase());
    this.matchedDeposit = found ?? undefined;
  }

  getDepositAmount(): number {
    return this.matchedDeposit ? this.matchedDeposit.amount : 0;
  }

  /**
   * Khi sale chọn "Design có sẵn": disable designer select (không cho chọn),
   * auto-assign order về designer đã làm ra design đó.
   */
  onDesignChange(designId: string): void {
    const designerCtrl = this.orderForm.get('designerUserId');
    if (!designId) {
      designerCtrl?.enable({ emitEvent: false });
      return;
    }
    const design = this.availableDesigns.find(d => d.id === designId);
    if (design?.assignedToUserId) {
      designerCtrl?.setValue(design.assignedToUserId, { emitEvent: false });
    }
    designerCtrl?.disable({ emitEvent: false });
  }

  isDesignLocked(): boolean {
    return !!this.orderForm.get('designId')?.value;
  }

  onProvinceChange(provinceCode: string, keepWard = false): void {
    if (!keepWard) this.orderForm.get('shippingWardCode')?.setValue('');
    if (!provinceCode) {
      this.wards = [];
      return;
    }
    this.isLoadingWards = true;
    this.locationService.getWardsByProvince(provinceCode).subscribe({
      next: (wards) => { this.wards = wards; this.isLoadingWards = false; },
      error: () => { this.wards = []; this.isLoadingWards = false; }
    });
  }

  get items(): FormArray { return this.orderForm.get('items') as FormArray; }

  private resetAttributeFilters(): void {
    this.filteredMaterials = this.materials;
    this.filteredColors = this.colorFabrics;
    this.filteredForms = this.shirtForms;
    this.filteredSpecs = this.styleSpecs;
  }

  onCollectionChange(collectionId: string): void {
    const col = this.collections.find(c => c.id === collectionId);
    if (!col) { this.resetAttributeFilters(); this.recomputeFilteredColors(); return; }
    // Nếu bộ sưu tập không giới hạn (danh sách rỗng, VD sau khi đổi data chất liệu) thì hiện toàn bộ.
    const mats = this.materials.filter(m => col.materialIds.includes(m.id));
    const forms = this.shirtForms.filter(f => col.formIds.includes(f.id));
    const specs = this.styleSpecs.filter(s => col.specificationIds.includes(s.id));
    this.filteredMaterials = mats.length ? mats : this.materials;
    this.filteredForms = forms.length ? forms : this.shirtForms;
    this.filteredSpecs = specs.length ? specs : this.styleSpecs;

    // Clear selections not in filtered set
    const pi = this.orderForm.get('productInfo');
    const clearIfMissing = (key: string, arr: { id: string }[]) => {
      const v = pi?.get(key)?.value;
      if (v && !arr.find(x => x.id === v)) pi?.get(key)?.setValue('');
    };
    clearIfMissing('materialId', this.filteredMaterials);
    clearIfMissing('formId', this.filteredForms);
    clearIfMissing('specificationId', this.filteredSpecs);

    // Màu lọc theo cả bộ sưu tập lẫn chất liệu đang chọn.
    this.recomputeFilteredColors();
  }

  // Màu "ăn theo" chất liệu: chỉ hiển thị màu của chất liệu đang chọn (kèm màu dùng chung không gán).
  // Kết hợp với lọc theo bộ sưu tập (nếu có).
  onMaterialChange(): void {
    this.recomputeFilteredColors();
    this.updateColorControlsState();
  }

  // Phải chọn chất liệu trước khi chọn màu.
  hasMaterialSelected(): boolean {
    return !!this.orderForm.get('productInfo.materialId')?.value;
  }

  // Khoá 2 ô màu (chính + phối) khi chưa chọn chất liệu.
  private updateColorControlsState(): void {
    const pi = this.orderForm.get('productInfo');
    const main = pi?.get('mainColorId');
    const accent = pi?.get('accentColorId');
    if (this.hasMaterialSelected()) {
      main?.enable({ emitEvent: false });
      accent?.enable({ emitEvent: false });
    } else {
      main?.setValue('', { emitEvent: false });
      accent?.setValue('', { emitEvent: false });
      main?.disable({ emitEvent: false });
      accent?.disable({ emitEvent: false });
    }
  }

  private recomputeFilteredColors(): void {
    const pi = this.orderForm.get('productInfo');
    const collectionId = pi?.get('collectionId')?.value;
    const materialId = pi?.get('materialId')?.value;
    const col = this.collections.find(c => c.id === collectionId);

    let colors = this.colorFabrics;
    if (col) {
      const colColors = this.colorFabrics.filter(c => col.colorFabricIds.includes(c.id));
      // Bộ sưu tập không giới hạn màu → giữ toàn bộ (tránh dropdown rỗng sau khi đổi data).
      if (colColors.length) colors = colColors;
    }
    if (materialId) {
      colors = colors.filter(c => c.materialId === materialId || !c.materialId);
    }
    this.filteredColors = colors;

    const clearIfMissing = (key: string) => {
      const v = pi?.get(key)?.value;
      if (v && !this.filteredColors.find(x => x.id === v)) pi?.get(key)?.setValue('');
    };
    clearIfMissing('mainColorId');
    clearIfMissing('accentColorId');
  }

  private recalcDates(): void {
    const optId = this.orderForm.get('productionDaysOptionId')?.value;
    const orderDateStr = this.orderForm.get('orderDate')?.value;
    if (!optId || !orderDateStr) {
      this.orderForm.get('completionDate')?.setValue('');
      this.orderForm.get('returnDate')?.setValue('');
      return;
    }
    const opt = this.productionDaysOptions.find(o => o.id === optId);
    if (!opt) return;
    const base = new Date(orderDateStr);
    const completion = new Date(base); completion.setDate(base.getDate() + opt.days);
    const ret = new Date(completion); ret.setDate(completion.getDate() + 1);
    this.orderForm.get('completionDate')?.setValue(this.formatDate(completion));
    this.orderForm.get('returnDate')?.setValue(this.formatDate(ret));
  }

  loadDealsForCustomer(customerId: string): void {
    if (!customerId) { this.filteredDeals = []; return; }
    this.dealService.getDeals({ customerId, page: 1, pageSize: 50 }).subscribe({
      next: (response) => { this.filteredDeals = response?.items || []; }
    });
  }

  getCurrentUserDisplayName(): string {
    const user = this.authService.getCurrentUser();
    return user ? `${user.firstName} ${user.lastName}` : '';
  }

  getCustomerDisplayName(customer: Customer): string {
    return customer.name + (customer.companyName ? ` (${customer.companyName})` : '');
  }

  onCustomerInput(event: Event): void {
    const inputVal = (event.target as HTMLInputElement).value;
    this.customerSearchText = inputVal;
    this.showCustomerDropdown = true;
    const q = inputVal.toLowerCase();
    this.filteredCustomers = q
      ? this.customers.filter(c =>
          c.name.toLowerCase().includes(q) ||
          (c.companyName ?? '').toLowerCase().includes(q)
        )
      : this.customers;
    this.orderForm.get('customerId')?.setValue('');
    this.orderForm.get('dealId')?.setValue('');
    this.filteredDeals = [];
  }

  onCustomerFocus(): void {
    const q = this.customerSearchText.toLowerCase();
    this.filteredCustomers = q
      ? this.customers.filter(c =>
          c.name.toLowerCase().includes(q) ||
          (c.companyName ?? '').toLowerCase().includes(q)
        )
      : this.customers;
    this.showCustomerDropdown = true;
  }

  onCustomerBlur(): void { setTimeout(() => { this.showCustomerDropdown = false; }, 150); }

  selectCustomer(customer: Customer): void {
    this.customerSearchText = this.getCustomerDisplayName(customer);
    this.orderForm.get('customerId')?.setValue(customer.id);
    this.orderForm.get('customerId')?.markAsTouched();
    this.showCustomerDropdown = false;
    this.onCustomerChange();
  }

  onCustomerChange(): void {
    const customerId = this.orderForm.get('customerId')?.value;
    this.orderForm.get('dealId')?.setValue('');
    this.loadDealsForCustomer(customerId);
    const customer = this.customers.find(c => c.id === customerId);
    if (customer) {
      this.orderForm.patchValue({
        shippingAddress: customer.address || '',
        shippingPhone: customer.phone || '',
        shippingContactName: customer.name || ''
      });
    }
  }

  loadOrder(): void {
    if (!this.orderId) return;
    this.isLoading = true;
    this.orderService.getOrder(this.orderId).subscribe({
      next: (order) => {
        this.orderForm.patchValue({
          customerId: order.customerId,
          dealId: order.dealId || '',
          orderDate: this.formatDate(new Date(order.orderDate)),
          productionDaysOptionId: order.productionDaysOptionId || '',
          orderTypeId: order.orderTypeId || '',
          completionDate: order.completionDate ? this.formatDate(new Date(order.completionDate)) : '',
          returnDate: order.returnDate ? this.formatDate(new Date(order.returnDate)) : '',
          depositCode: order.depositCode || '',
          assignedToUserId: order.assignedToUserId || '',
          designerUserId: order.designerUserId || '',
          shipperUserId: order.shipperUserId || '',
          designId: order.designId || '',
          deliveryMethod: order.deliveryMethod ?? '',
          senderAddressId: order.senderAddressId || '',
          shippingContactName: order.shippingContactName || '',
          shippingPhone: order.shippingPhone || '',
          shippingAddress: order.shippingAddress || '',
          shippingNotes: order.shippingNotes || '',
          discountPercent: order.discountPercent || 0,
          taxPercent: order.taxPercent || 0,
          shippingFee: order.shippingFee || 0,
          paymentMethod: order.paymentMethod || '',
          styleNotes: order.styleNotes || '',
          internalNotes: order.internalNotes || '',
          customerNotes: order.customerNotes || ''
        });

        // Nếu đơn đã gắn design có sẵn — khoá designer dropdown.
        if (order.designId) {
          this.orderForm.get('designerUserId')?.disable({ emitEvent: false });
        }

        // Patch province + ward tách khỏi patchValue chính để không trigger cascade wipe ward.
        if (order.shippingProvinceCode) {
          this.orderForm.get('shippingProvinceCode')?.setValue(order.shippingProvinceCode, { emitEvent: false });
          this.onProvinceChange(order.shippingProvinceCode, true);
          this.orderForm.get('shippingWardCode')?.setValue(order.shippingWardCode || '', { emitEvent: false });
        }

        const selectedCustomer = this.customers.find(c => c.id === order.customerId);
        if (selectedCustomer) this.customerSearchText = this.getCustomerDisplayName(selectedCustomer);
        else if (order.customerName) this.customerSearchText = order.customerName;
        this.loadDealsForCustomer(order.customerId);

        const firstItem = order.items[0];
        if (firstItem) {
          this.orderForm.get('productInfo')?.patchValue({
            collectionId: firstItem.collectionId || '',
            materialId: firstItem.materialId || '',
            mainColorId: firstItem.mainColorId || '',
            accentColorId: firstItem.accentColorId || '',
            formId: firstItem.formId || '',
            specificationId: firstItem.specificationId || '',
            unitPrice: firstItem.unitPrice || 0,
            itemDiscountPercent: firstItem.discountPercent || 0,
          });
          if (firstItem.collectionId) this.onCollectionChange(firstItem.collectionId);
        }

        // Xác định form của đơn để biết size lưu dạng chia giới tính hay 1 dòng.
        const orderFormName = (this.shirtForms.find(f => f.id === firstItem?.formId)?.name || '').toLowerCase();
        const orderGendered = !(orderFormName.includes('oversize') || orderFormName.includes('unisex'));
        this.sizeQty = {};
        order.items.forEach(item => {
          if (item.size) {
            const sz = this.normalizeSizeKey(item.size, orderGendered);
            this.sizeQty[sz] = (this.sizeQty[sz] || 0) + item.quantity;
          }
        });
        this.lastGendered = orderGendered;

        this.items.clear();
        this.isLoading = false;
      },
      error: () => { this.errorMessage = 'Không thể tải thông tin đơn hàng'; this.isLoading = false; }
    });
  }

  calculateItemTotal(index: number): number {
    const item = this.items.at(index);
    const qty = item.get('quantity')?.value || 0;
    const price = item.get('unitPrice')?.value || 0;
    const disc = item.get('discountPercent')?.value || 0;
    return qty * price * (1 - disc / 100);
  }

  calculateSubTotal(): number {
    const pi = this.orderForm.get('productInfo')?.value || {};
    const totalQty = this.getTotalQtyInput();
    const disc = pi.itemDiscountPercent || 0;
    return totalQty * (pi.unitPrice || 0) * (1 - disc / 100);
  }

  calculateDiscountAmount(): number {
    return this.calculateSubTotal() * ((this.orderForm.get('discountPercent')?.value || 0) / 100);
  }

  calculateTotal(): number {
    return this.calculateSubTotal() - this.calculateDiscountAmount() + (this.orderForm.get('shippingFee')?.value || 0);
  }

  // Còn phải thu sau khi đã trừ tiền cọc (nếu mã cọc khớp).
  calculateRemaining(): number {
    return Math.max(0, this.calculateTotal() - this.getDepositAmount());
  }

  formatCurrency(value: number): string {
    return new Intl.NumberFormat('vi-VN', { style: 'currency', currency: 'VND' }).format(value);
  }

  formatDate(date: Date): string { return date.toISOString().split('T')[0]; }

  onSubmit(): void {
    if (this.orderForm.invalid) {
      this.orderForm.markAllAsTouched();
      this.errorMessage = 'Vui lòng kiểm tra lại các trường bắt buộc.';
      return;
    }
    if (this.getTotalQtyInput() < 1) {
      this.errorMessage = 'Tổng số lượng sản phẩm phải ít nhất là 1.';
      return;
    }
    this.isLoading = true;
    this.errorMessage = '';

    const f = this.orderForm.getRawValue();
    const pi = f.productInfo || {};
    const selectedProvince = this.provinces.find(p => p.code === f.shippingProvinceCode);
    const selectedWard = this.wards.find(w => w.code === f.shippingWardCode);
    const orderData = {
      customerId: f.customerId || undefined,
      customerName: !f.customerId ? this.customerSearchText || undefined : undefined,
      dealId: f.dealId || null,
      orderDate: f.orderDate ? new Date(f.orderDate) : undefined,
      productionDaysOptionId: f.productionDaysOptionId || undefined,
      orderTypeId: f.orderTypeId || undefined,
      depositCode: f.depositCode || undefined,
      assignedToUserId: f.assignedToUserId || undefined,
      designerUserId: f.designerUserId || undefined,
      // Chỉ gửi shipper khi hình thức = Nhà giao; Vehicle/GHTK luôn null phía backend.
      shipperUserId: this.isInHouseDelivery() ? (f.shipperUserId || undefined) : undefined,
      designId: f.designId || undefined,
      deliveryMethod: f.deliveryMethod === '' || f.deliveryMethod === null || f.deliveryMethod === undefined
        ? undefined
        : Number(f.deliveryMethod) as DeliveryMethod,
      senderAddressId: f.senderAddressId || undefined,
      shippingContactName: f.shippingContactName || undefined,
      shippingPhone: f.shippingPhone || undefined,
      shippingAddress: f.shippingAddress || undefined,
      shippingProvinceCode: f.shippingProvinceCode || undefined,
      shippingProvinceName: selectedProvince?.fullName || undefined,
      shippingWardCode: f.shippingWardCode || undefined,
      shippingWardName: selectedWard?.name || undefined,
      shippingNotes: f.shippingNotes || undefined,
      discountPercent: f.discountPercent,
      taxPercent: f.taxPercent,
      shippingFee: f.shippingFee,
      paymentMethod: f.paymentMethod,
      styleNotes: f.styleNotes,
      internalNotes: f.internalNotes,
      customerNotes: f.customerNotes,
      items: Object.entries(this.sizeQty)
        .filter(([, quantity]) => quantity > 0)
        .map(([size, quantity]) => ({
          collectionId: pi.collectionId || undefined,
          materialId: pi.materialId || undefined,
          mainColorId: pi.mainColorId || undefined,
          accentColorId: pi.accentColorId || undefined,
          formId: pi.formId || undefined,
          specificationId: pi.specificationId || undefined,
          size,
          quantity,
          unit: 'cái',
          unitPrice: pi.unitPrice || 0,
          discountPercent: pi.itemDiscountPercent || 0,
        } as CreateOrderItemRequest))
    };

    const obs$ = this.isEditMode && this.orderId
      ? this.orderService.updateOrder(this.orderId, { ...orderData, id: this.orderId })
      : this.orderService.createOrder(orderData);

    obs$.subscribe({
      next: (order) => this.router.navigate(['/orders', order.id]),
      error: (error) => {
        this.isLoading = false;
        this.errorMessage = error.error?.message || 'Lưu thất bại. Vui lòng thử lại.';
      }
    });
  }

  cancel(): void { this.router.navigate(['/orders']); }

  get f() { return this.orderForm.controls; }

  private normalizeSizeKey(size: string, gendered: boolean): string {
    const normalized = size.trim().toUpperCase();
    if (normalized.includes(':')) {
      const [rawGender, rawSize] = normalized.split(':', 2);
      if (!gendered) return rawSize;
      const gender = rawGender === 'NỮ' || rawGender === 'NU' ? 'NU' : 'NAM';
      return this.sizeKey(rawSize, gender);
    }
    return gendered ? this.sizeKey(normalized, 'NAM') : normalized;
  }
}
