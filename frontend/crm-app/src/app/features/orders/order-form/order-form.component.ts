import { Component, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, FormArray, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { forkJoin, of } from 'rxjs';
import { OrderService } from '../../../core/services/order.service';
import { CustomerService, Customer } from '../../../core/services/customer.service';
import { DealService, Deal } from '../../../core/services/deal.service';
import { DesignService, ColorFabric } from '../../../core/services/design.service';
import { SettingsService } from '../../../core/services/settings.service';
import { UserManagementService, UserListItem } from '../../../core/services/user-management.service';
import { AuthService } from '../../../core/services/auth.service';
import { CreateOrderItemRequest } from '../../../core/models/order.model';
import { Collection, LookupItem, ProductionDaysOption } from '../../../core/models/lookup.model';

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
  customerSearchText = '';
  showCustomerDropdown = false;

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
      deliveryContactName: [''],
      deliveryPhone: [''],
      deliveryAddress: [''],
      deliveryWard: [''],
      deliveryDistrict: [''],
      deliveryCity: [''],
      discountPercent: [0, [Validators.min(0), Validators.max(100)]],
      taxPercent: [10, [Validators.min(0), Validators.max(100)]],
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

    forkJoin({
      customers: this.customerService.getCustomers({ page: 1, pageSize: 200 }),
      colors: this.designService.getAllColorFabrics(),
      materials: this.settingsService.getLookups('materials'),
      shirtForms: this.settingsService.getLookups('product-forms'),
      styleSpecs: this.settingsService.getLookups('product-specifications'),
      collections: this.settingsService.getCollections(),
      productionDaysOptions: this.settingsService.getProductionDaysOptions(),
      users: this.userService.getUsers({ page: 1, pageSize: 200, isActive: true }),
      designers: this.userService.getUsers({ page: 1, pageSize: 200, isActive: true, role: 'Designer' })
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

        this.resetAttributeFilters();

        if (this.isEditMode) {
          this.loadOrder();
        } else {
          const currentUser = this.authService.getCurrentUser();
          if (currentUser) {
            this.orderForm.get('assignedToUserId')?.setValue(currentUser.id);
          }
        }
      }
    });

    this.orderForm.get('productionDaysOptionId')?.valueChanges.subscribe(() => this.recalcDates());
    this.orderForm.get('orderDate')?.valueChanges.subscribe(() => this.recalcDates());
    this.orderForm.get('productInfo.collectionId')?.valueChanges.subscribe((id: string) => this.onCollectionChange(id));
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
        deliveryAddress: customer.address || '',
        deliveryCity: customer.city || '',
        deliveryPhone: customer.phone || '',
        deliveryContactName: customer.name || ''
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
          deliveryAddress: order.deliveryAddress || '',
          deliveryCity: order.deliveryCity || '',
          deliveryDistrict: order.deliveryDistrict || '',
          deliveryWard: order.deliveryWard || '',
          deliveryPhone: order.deliveryPhone || '',
          deliveryContactName: order.deliveryContactName || '',
          discountPercent: order.discountPercent || 0,
          taxPercent: order.taxPercent || 10,
          shippingFee: order.shippingFee || 0,
          paymentMethod: order.paymentMethod || '',
          styleNotes: order.styleNotes || '',
          internalNotes: order.internalNotes || '',
          customerNotes: order.customerNotes || ''
        });

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

  calculateTaxAmount(): number {
    return (this.calculateSubTotal() - this.calculateDiscountAmount()) * ((this.orderForm.get('taxPercent')?.value || 0) / 100);
  }

  calculateTotal(): number {
    return this.calculateSubTotal() - this.calculateDiscountAmount() + this.calculateTaxAmount() + (this.orderForm.get('shippingFee')?.value || 0);
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
    const orderData = {
      customerId: f.customerId || undefined,
      customerName: !f.customerId ? this.customerSearchText || undefined : undefined,
      dealId: f.dealId || null,
      orderDate: f.orderDate ? new Date(f.orderDate) : undefined,
      productionDaysOptionId: f.productionDaysOptionId || undefined,
      depositCode: f.depositCode || undefined,
      assignedToUserId: f.assignedToUserId || undefined,
      designerUserId: f.designerUserId || undefined,
      deliveryAddress: f.deliveryAddress,
      deliveryCity: f.deliveryCity,
      deliveryDistrict: f.deliveryDistrict,
      deliveryWard: f.deliveryWard,
      deliveryPhone: f.deliveryPhone,
      deliveryContactName: f.deliveryContactName,
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
