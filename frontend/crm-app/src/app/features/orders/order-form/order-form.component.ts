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
import { Collection, LookupItem, ProductionDaysOption, DepositTransaction } from '../../../core/models/lookup.model';
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
  collections: Collection[] = [];
  productionDaysOptions: ProductionDaysOption[] = [];

  // Filtered by selected collection
  filteredMaterials: LookupItem[] = [];
  filteredColors: ColorFabric[] = [];
  filteredForms: LookupItem[] = [];
  filteredSpecs: LookupItem[] = [];

  users: UserListItem[] = [];
  designers: UserListItem[] = [];
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
    { value: DeliveryMethod.GHTK,    label: DeliveryMethodLabels[DeliveryMethod.GHTK] }
  ];

  // null = chưa biết (đang load), true = đã cấu hình token + kho, false = chưa cấu hình.
  ghtkConfigured: boolean | null = null;

  // Size grid
  readonly SIZE_LIST = ['S', 'M', 'L', 'XL', 'XXL', 'NC1', 'NC2', 'NC3'];
  sizeQty: Record<string, number> = {};

  getSizeQtyInput(size: string): number { return this.sizeQty[size] || 0; }
  onSizeQtyChange(size: string, event: Event): void {
    const val = parseInt((event.target as HTMLInputElement).value, 10);
    this.sizeQty[size] = isNaN(val) || val < 0 ? 0 : val;
  }
  getTotalQtyInput(): number {
    return Object.values(this.sizeQty).reduce((a, b) => a + b, 0);
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
      completionDate: [{ value: '', disabled: true }],
      returnDate: [{ value: '', disabled: true }],
      depositCode: [''],
      assignedToUserId: [''],
      designerUserId: [''],
      designId: [''],
      deliveryMethod: [''],
      shippingContactName: [''],
      shippingPhone: [''],
      shippingAddress: [''],
      shippingProvinceCode: [''],
      shippingWardCode: [''],
      shippingNotes: [''],
      discountPercent: [0, [Validators.min(0), Validators.max(100)]],
      taxPercent: [0, [Validators.min(0), Validators.max(100)]],
      shippingFee: [0, [Validators.min(0)]],
      paymentMethod: [''],
      styleNotes: [''],
      internalNotes: [''],
      customerNotes: [''],
      productInfo: this.fb.group({
        collectionId: [''],
        materialId: [''],
        mainColorId: [''],
        accentColorId: [''],
        formId: [''],
        specificationId: [''],
        unitPrice: [0, [Validators.min(0)]],
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

    forkJoin({
      customers: this.customerService.getCustomers({ page: 1, pageSize: 200 }),
      colors: this.designService.getAllColorFabrics(),
      materials: this.settingsService.getLookups('materials'),
      shirtForms: this.settingsService.getLookups('product-forms'),
      styleSpecs: this.settingsService.getLookups('product-specifications'),
      collections: this.settingsService.getCollections(),
      productionDaysOptions: this.settingsService.getProductionDaysOptions(),
      users: this.userService.getUsers({ page: 1, pageSize: 200, isActive: true }),
      designers: this.userService.getUsers({ page: 1, pageSize: 200, isActive: true, role: 'Designer' }),
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
        this.collections = res.collections || [];
        this.productionDaysOptions = (res.productionDaysOptions || []).filter(o => o.isActive);
        this.users = res.users?.items || [];
        this.designers = res.designers?.items || [];
        this.provinces = res.provinces || [];
        this.availableDesigns = res.availableDesigns || [];
        this.deposits = res.deposits || [];

        this.resetAttributeFilters();

        if (this.isEditMode) {
          this.loadOrder();
        } else {
          const currentUser = this.authService.getCurrentUser();
          if (currentUser) {
            this.orderForm.get('assignedToUserId')?.setValue(currentUser.id);
          }
        }

        // Re-match deposit nếu đã có sẵn depositCode (edit mode hoặc form đã prefill).
        this.onDepositCodeChange(this.orderForm.get('depositCode')?.value || '');
      }
    });

    this.orderForm.get('productionDaysOptionId')?.valueChanges.subscribe(() => this.recalcDates());
    this.orderForm.get('orderDate')?.valueChanges.subscribe(() => this.recalcDates());
    this.orderForm.get('productInfo.collectionId')?.valueChanges.subscribe((id: string) => this.onCollectionChange(id));
    this.orderForm.get('shippingProvinceCode')?.valueChanges.subscribe((code: string) => this.onProvinceChange(code));
    this.orderForm.get('designId')?.valueChanges.subscribe((id: string) => this.onDesignChange(id));
    this.orderForm.get('depositCode')?.valueChanges.subscribe((code: string) => this.onDepositCodeChange(code));
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
    if (!col) { this.resetAttributeFilters(); return; }
    this.filteredMaterials = this.materials.filter(m => col.materialIds.includes(m.id));
    this.filteredColors = this.colorFabrics.filter(c => col.colorFabricIds.includes(c.id));
    this.filteredForms = this.shirtForms.filter(f => col.formIds.includes(f.id));
    this.filteredSpecs = this.styleSpecs.filter(s => col.specificationIds.includes(s.id));

    // Clear selections not in filtered set
    const pi = this.orderForm.get('productInfo');
    const clearIfMissing = (key: string, arr: { id: string }[]) => {
      const v = pi?.get(key)?.value;
      if (v && !arr.find(x => x.id === v)) pi?.get(key)?.setValue('');
    };
    clearIfMissing('materialId', this.filteredMaterials);
    clearIfMissing('mainColorId', this.filteredColors);
    clearIfMissing('accentColorId', this.filteredColors);
    clearIfMissing('formId', this.filteredForms);
    clearIfMissing('specificationId', this.filteredSpecs);
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
          completionDate: order.completionDate ? this.formatDate(new Date(order.completionDate)) : '',
          returnDate: order.returnDate ? this.formatDate(new Date(order.returnDate)) : '',
          depositCode: order.depositCode || '',
          assignedToUserId: order.assignedToUserId || '',
          designerUserId: order.designerUserId || '',
          designId: order.designId || '',
          deliveryMethod: order.deliveryMethod ?? '',
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

        this.sizeQty = {};
        order.items.forEach(item => {
          if (item.size) {
            const sz = item.size.trim().toUpperCase();
            this.sizeQty[sz] = (this.sizeQty[sz] || 0) + item.quantity;
          }
        });

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
    if (this.orderForm.invalid) { this.orderForm.markAllAsTouched(); return; }
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
      depositCode: f.depositCode || undefined,
      assignedToUserId: f.assignedToUserId || undefined,
      designerUserId: f.designerUserId || undefined,
      designId: f.designId || undefined,
      deliveryMethod: f.deliveryMethod === '' || f.deliveryMethod === null || f.deliveryMethod === undefined
        ? undefined
        : Number(f.deliveryMethod) as DeliveryMethod,
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
      items: this.SIZE_LIST
        .filter(s => (this.sizeQty[s] || 0) > 0)
        .map(s => ({
          collectionId: pi.collectionId || undefined,
          materialId: pi.materialId || undefined,
          mainColorId: pi.mainColorId || undefined,
          accentColorId: pi.accentColorId || undefined,
          formId: pi.formId || undefined,
          specificationId: pi.specificationId || undefined,
          size: s,
          quantity: this.sizeQty[s],
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
}
